using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovementStateManager movement;
    AimStateManager aim;

    public bool IsRunning { get; private set; }

    public bool HasSecurityPassTool { get; private set; }
    
    void Awake()
    {
        movement = GetComponent<MovementStateManager>();
        aim = GetComponent<AimStateManager>();

        if (movement != null)
        {
            movement.isPlayerControlled = true;
        }

        HasSecurityPassTool = true;
    }

    void Update()
    {
        if (movement == null) return;

        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        
        IsRunning = Input.GetKey(KeyCode.LeftShift);
        
        if (IsRunning)
        {
            movement.currentMoveSpeed = movement.RunSpeed;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            movement.currentMoveSpeed = movement.crunchSpeed;
        }
        else
        {
            movement.currentMoveSpeed = movement.walkSpeed;
        }

        movement.Move(input);
    }

    public void SetSecurityPass(bool value)
    {
        HasSecurityPassTool = value;
    }
}