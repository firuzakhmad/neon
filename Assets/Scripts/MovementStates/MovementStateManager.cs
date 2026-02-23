using UnityEngine;

public class MovementStateManager : MonoBehaviour, ICharacterMotor
{
    public float currentMoveSpeed;
    public float walkSpeed = 3, walkBackSpeed = 2;
    public float RunSpeed = 7, runBackSpeed = 5;
    public float crunchSpeed = 2, crunchBackSpeed = 1;

    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float hzInput, vInput;

    CharacterController controller;
    [SerializeField] float groundYOffset;
    [SerializeField] LayerMask groundMask;
    Vector3 spherePos;
    [SerializeField] float gravity = -9.81f;
    Vector3 velocity;

    MovementBaseState currentState;
    
    public IdleState idle = new IdleState();
    public WalkState walk = new WalkState();
    public CrunchState crunch = new CrunchState();
    public RunState run = new RunState();

    [HideInInspector] public Animator animator;
    

    public bool isPlayerControlled = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        currentMoveSpeed = walkSpeed;
        
        if (isPlayerControlled)
        {
            SwitchState(idle);
        }
    }

    void Update()
    {
        Gravity();
        
        if (animator != null)
        {
            animator.SetFloat("hzInput", hzInput);
            animator.SetFloat("vInput", vInput);
        }
        
    
        if (isPlayerControlled)
        {
            currentState?.UpdateState(this);
        }
    }

    public void SwitchState(MovementBaseState state)
    {
        currentState = state;
        currentState?.EnterState(this);
    }

    bool isGrounded()
    {
        spherePos = new Vector3(
            transform.position.x,
            transform.position.y - groundYOffset,
            transform.position.z);
        return Physics.CheckSphere(spherePos, controller.radius - 0.05f, groundMask);
    }

    void Gravity()
    {
        if (!isGrounded())
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2;
        }

        controller.Move(velocity * Time.deltaTime);
    }


    public void Move(Vector2 input)
    {
        hzInput = input.x;
        vInput = input.y;

        direction = transform.forward * vInput + transform.right * hzInput;
        controller.Move(direction.normalized * currentMoveSpeed * Time.deltaTime);
    }


    public void MoveCharacter(Vector2 input)
    {
        hzInput = input.x;
        vInput = input.y;

        direction = transform.forward * vInput + transform.right * hzInput;
        controller.Move(direction.normalized * currentMoveSpeed * Time.deltaTime);
    }

    public void SetSpeed(float speed)
    {
        currentMoveSpeed = speed;
    }

    public void SetAnimationBool(string parameter, bool value)
    {
        if (animator != null)
            animator.SetBool(parameter, value);
    }

    public void SetAnimationFloat(string parameter, float value)
    {
        if (animator != null)
            animator.SetFloat(parameter, value);
    }

    private void OnDrawGizmos()
    {
        if (!controller) controller = GetComponent<CharacterController>();
        if (!controller) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spherePos, controller.radius - 0.05f);
    }
}