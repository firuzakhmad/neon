using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    int currentPoint;
    
    [Header("Detection Settings")]
    public float detectionRange = 15f;
    public float fieldOfView = 140f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    public float losePlayerTime = 3f;
    
    [Header("References")]
    public Transform player;
    
    NavMeshAgent agent;
    MovementStateManager movement;
    ICharacterMotor motor;
    
    enum GuardState { Patrolling, Chasing, Searching }
    GuardState currentState;
    
    float losePlayerTimer;
    Vector3 lastKnownPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<MovementStateManager>();
        motor = GetComponent<ICharacterMotor>();
        
        if (movement != null)
        {
            movement.isPlayerControlled = false;
        }
        
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Start()
    {
        currentState = GuardState.Patrolling;
        if (patrolPoints.Length > 0 && agent != null)
        {
            agent.SetDestination(patrolPoints[currentPoint].position);
            agent.speed = patrolSpeed;
        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        switch (currentState)
        {
            case GuardState.Patrolling:
                UpdatePatrol();
                break;
            case GuardState.Chasing:
                UpdateChase();
                break;
            case GuardState.Searching:
                UpdateSearch();
                break;
        }

        
        CheckForPlayer();

        
        UpdateMovementAnimation();
    }

    void UpdatePatrol()
    {
        if (movement != null)
            movement.SetSpeed(patrolSpeed);

        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }

        
        SyncAgentWithTransform();
    }

    void UpdateChase()
    {
        
        agent.SetDestination(player.position);
        agent.speed = chaseSpeed;

        if (movement != null)
            movement.SetSpeed(chaseSpeed);

        if (player != null)
        {
            agent.SetDestination(player.position);
            
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= agent.stoppingDistance)
            {
                
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Die();
                }
            }
        }
        
        
        if (!CanSeePlayer())
        {
            losePlayerTimer += Time.deltaTime;
            if (losePlayerTimer >= losePlayerTime)
            {
                lastKnownPosition = player.position;
                currentState = GuardState.Searching;
                agent.SetDestination(lastKnownPosition);
                losePlayerTimer = 0;
            }
        }
        else
        {
            losePlayerTimer = 0;
        }

        SyncAgentWithTransform();
    }

    void UpdateSearch()
    {
        if (movement != null)
            movement.SetSpeed(patrolSpeed);

        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            
            currentState = GuardState.Patrolling;
            agent.speed = patrolSpeed;
            if (patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[currentPoint].position);
            }
        }

        SyncAgentWithTransform();
    }

    void CheckForPlayer()
    {
        if (CanSeePlayer())
        {
            currentState = GuardState.Chasing;
            agent.speed = chaseSpeed;
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRange)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angle <= fieldOfView * 0.5f)
            {
                
                Vector3 rayStart = transform.position + Vector3.up * 1.5f; 
                Vector3 rayDirection = (player.position - rayStart).normalized;
                float rayDistance = Vector3.Distance(rayStart, player.position);
                
                Debug.DrawRay(rayStart, rayDirection * rayDistance, Color.red, 0.1f); 
                
                RaycastHit hit;
                if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    void SyncAgentWithTransform()
    {
        if (agent == null) return;

        
        Vector3 worldVel = agent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        
        if (motor != null)
        {
            motor.SetAnimationFloat("hzInput", localVel.x);
            motor.SetAnimationFloat("vInput", localVel.z);
            
            
            motor.MoveCharacter(new Vector2(localVel.x, localVel.z));
        }

        
        agent.nextPosition = transform.position;

        
        if (worldVel.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(worldVel.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 8f
            );
        }
    }

    void UpdateMovementAnimation()
    {
        if (motor != null)
        {
            switch (currentState)
            {
                case GuardState.Chasing:
                    motor.SetAnimationBool("Running", true);
                    motor.SetAnimationBool("Walking", false);
                    break;
                case GuardState.Patrolling:
                    motor.SetAnimationBool("Running", false);
                    motor.SetAnimationBool("Walking", true);
                    break;
                case GuardState.Searching:
                    motor.SetAnimationBool("Running", false);
                    motor.SetAnimationBool("Walking", true);
                    break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView * 0.5f, transform.up) * transform.forward * detectionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView * 0.5f, transform.up) * transform.forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
    }
}