using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CivilianAI : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 50f;
    public float wanderTimer = 25f;
    public float walkSpeed = 2f;
    
    [Header("Fear Settings")]
    public float fearRange = 10f;
    public float fleeSpeed = 4f;
    public float faceToFaceDistance = 1.5f;
    [Header("Social Settings")]
    public float meetingDistance = 1.5f; 
    public float meetingDuration = 5f; 
    public float separationDistance = 0.5f; 
    
    [Header("References")]
    public Transform player;
    
    NavMeshAgent agent;
    MovementStateManager movement;
    ICharacterMotor motor;
    float timer;
    
    enum CivilianState { Wandering, Fleeing, WatchingPlayer, Meeting }
    CivilianState currentState;
    
    bool isWatchingPlayer = false;
    float watchTimer = 0f;
    float watchDuration = 3f;
    
    bool isMeeting = false;
    float meetingTimer = 0f;
    CivilianAI meetingPartner = null;
    Vector3 meetingPosition;
    Vector3 partnerMeetingPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<MovementStateManager>();
        motor = GetComponent<ICharacterMotor>();

        agent.avoidancePriority = Random.Range(30, 70);
        
        if (movement != null)
        {
            movement.isPlayerControlled = false;
        }
        
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.speed = walkSpeed;
        }
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
                
        }
    }

    void Start()
    {
        SetNewWanderDestination();
        currentState = CivilianState.Wandering;
        timer = Random.Range(0, wanderTimer);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        
        if (motor != null)
        {
            motor.SetAnimationBool("Walking", true);
            motor.SetAnimationBool("Running", false);
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (currentState != CivilianState.Meeting)
        {
            CheckPlayerBehavior();
        }
        
        switch (currentState)
        {
            case CivilianState.Wandering:
                UpdateWandering();
                break;
            case CivilianState.WatchingPlayer:
                UpdateWatchingPlayer();
                break;
            case CivilianState.Fleeing:
                UpdateFleeing();
                break;
            case CivilianState.Meeting:
                UpdateMeeting();
                break;
        }

        SyncAgentWithTransform();
    }

    void CheckPlayerBehavior()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        bool isPlayerRunning = playerController != null && playerController.IsRunning;
        
        if (distanceToPlayer <= fearRange)
        {
            if (isPlayerRunning)
            {
                if (currentState != CivilianState.Fleeing)
                {
                    currentState = CivilianState.Fleeing;
                    agent.speed = fleeSpeed;
                }
            }
            else if (distanceToPlayer <= faceToFaceDistance)
            {
                if (currentState != CivilianState.WatchingPlayer && currentState != CivilianState.Fleeing)
                {
                    currentState = CivilianState.WatchingPlayer;
                    agent.speed = 0f;
                    isWatchingPlayer = true;
                    watchTimer = 0f;
                    agent.ResetPath();
                }
            }
            else if (currentState == CivilianState.WatchingPlayer && distanceToPlayer > faceToFaceDistance)
            {
                ReturnToWandering();
            }
        }
        else if (currentState == CivilianState.WatchingPlayer || currentState == CivilianState.Fleeing)
        {
            ReturnToWandering();
        }
    }

    void UpdateWandering()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            SetNewWanderDestination();
            timer = 0;
        }

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            SetNewWanderDestination();
        }

        CheckForNearbyCivilians();

        if (motor != null)
        {
            motor.SetAnimationBool("Walking", true);
            motor.SetAnimationBool("Running", false);
        }
    }

    void CheckForNearbyCivilians()
    {
        if (currentState != CivilianState.Wandering) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, meetingDistance);
        foreach (Collider col in colliders)
        {
            CivilianAI otherCivilian = col.GetComponent<CivilianAI>();
            
            if (otherCivilian != null && 
                otherCivilian != this && 
                otherCivilian.currentState == CivilianState.Wandering &&
                !otherCivilian.isMeeting)
            {
                Vector3 directionToOther = (otherCivilian.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, directionToOther);
                
                if (dotProduct > 0.5f)
                {
                    StartMeeting(otherCivilian);
                    break;
                }
            }
        }
    }

    void StartMeeting(CivilianAI other)
    {
        currentState = CivilianState.Meeting;
        other.currentState = CivilianState.Meeting;

        isMeeting = true;
        other.isMeeting = true;

        meetingPartner = other;
        other.meetingPartner = this;

        meetingTimer = 0f;

        agent.isStopped = false;
        other.agent.isStopped = false;

        Vector3 mid = (transform.position + other.transform.position) * 0.5f;
        Vector3 dir = (other.transform.position - transform.position).normalized;
        dir.y = 0;

        float gap = 1.5f;

        Vector3 mySpot = mid - dir * (gap * 0.5f);
        Vector3 otherSpot = mid + dir * (gap * 0.5f);

        MoveToNavMeshPosition(this, mySpot);
        MoveToNavMeshPosition(other, otherSpot);
    }

    void UpdateMeeting()
    {
        if (meetingPartner == null)
        {
            ReturnToWandering();
            return;
        }

        meetingTimer += Time.deltaTime;

        Vector3 directionToPartner = (meetingPartner.transform.position - transform.position).normalized;
        directionToPartner.y = 0;
        if (directionToPartner != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(directionToPartner), Time.deltaTime * 5f);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
        }

        if (motor != null)
        {
            motor.SetAnimationBool("Walking", false);
            motor.SetAnimationBool("Running", false);
            motor.SetAnimationFloat("hzInput", 0);
            motor.SetAnimationFloat("vInput", 0);
        }

        if (meetingTimer >= meetingDuration)
        {
            EndMeeting();
        }
    }

    void EndMeeting()
    {
        if (meetingPartner == null)
        {
            ReturnToWandering();
            return;
        }

        Vector3 dir = (meetingPartner.transform.position - transform.position).normalized;
        dir.y = 0;

        float escapeDistance = separationDistance;

        Vector3 myTarget = transform.position - dir * escapeDistance;
        Vector3 partnerTarget = meetingPartner.transform.position + dir * escapeDistance;

        agent.isStopped = false;
        meetingPartner.agent.isStopped = false;

        agent.ResetPath();
        meetingPartner.agent.ResetPath();

        MoveToNavMeshPosition(this, myTarget);
        MoveToNavMeshPosition(meetingPartner, partnerTarget);

        currentState = CivilianState.Wandering;
        meetingPartner.currentState = CivilianState.Wandering;

        isMeeting = false;
        meetingPartner.isMeeting = false;

        meetingPartner.meetingPartner = null;
        meetingPartner = null;
    }

    void MoveToNavMeshPosition(CivilianAI civ, Vector3 target)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(target, out hit, separationDistance * 2f, NavMesh.AllAreas))
        {
            civ.agent.speed = walkSpeed;
            civ.agent.isStopped = false;
            civ.agent.SetDestination(hit.position);
        }
        else
        {
            civ.SetNewWanderDestination();
        }
    }

    void ReturnToWandering()
    {
        currentState = CivilianState.Wandering;
        agent.speed = walkSpeed;
        isWatchingPlayer = false;
        isMeeting = false;
        meetingPartner = null;
        
        // Move apart before resuming wandering
        Vector3 moveApartDirection = Random.insideUnitSphere.normalized;
        moveApartDirection.y = 0;
        Vector3 newPosition = transform.position + moveApartDirection * separationDistance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPosition, out hit, separationDistance * 2, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            SetNewWanderDestination();
        }
    }

    void UpdateWatchingPlayer()
    {
        if (player == null)
        {
            ReturnToWandering();
            return;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        agent.velocity = Vector3.zero;
        agent.ResetPath();

        if (motor != null)
        {
            motor.SetAnimationBool("Walking", false);
            motor.SetAnimationBool("Running", false);
            motor.SetAnimationFloat("hzInput", 0);
            motor.SetAnimationFloat("vInput", 0);
        }

        watchTimer += Time.deltaTime;
        
        if (watchTimer >= watchDuration)
        {
            ReturnToWandering();
        }
    }

    void UpdateFleeing()
    {
        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * wanderRadius;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        if (motor != null)
        {
            motor.SetAnimationBool("Walking", false);
            motor.SetAnimationBool("Running", true);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        PlayerController playerController = player.GetComponent<PlayerController>();
        bool isPlayerRunning = playerController != null && playerController.IsRunning;
        
        if (distanceToPlayer > fearRange * 1.5f && !isPlayerRunning)
        {
            ReturnToWandering();
        }
    }

    void SetNewWanderDestination()
    {
        if (!agent.isOnNavMesh) return;
        
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                agent.isStopped = false;
                return;
            }
        }
    }

    void SyncAgentWithTransform()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 worldVel = agent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        
        if (motor != null && currentState != CivilianState.WatchingPlayer && currentState != CivilianState.Meeting)
        {
            motor.SetAnimationFloat("hzInput", localVel.x);
            motor.SetAnimationFloat("vInput", localVel.z);
            
            motor.MoveCharacter(new Vector2(localVel.x, localVel.z));
        }

        agent.nextPosition = transform.position;

        if (currentState != CivilianState.WatchingPlayer && 
            currentState != CivilianState.Meeting && 
            worldVel.sqrMagnitude > 0.05f)
        {
            Quaternion targetRot = Quaternion.LookRotation(worldVel.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 8f
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentState == CivilianState.Wandering)
        {
            CivilianAI otherCivilian = other.GetComponent<CivilianAI>();
            if (otherCivilian != null && 
                otherCivilian != this && 
                otherCivilian.currentState == CivilianState.Wandering &&
                !isMeeting && !otherCivilian.isMeeting)
            {
                // Check distance manually since trigger might be too small
                float distance = Vector3.Distance(transform.position, otherCivilian.transform.position);
                if (distance <= meetingDistance)
                {
                    StartMeeting(otherCivilian);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fearRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, faceToFaceDistance);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meetingDistance);
    }
}