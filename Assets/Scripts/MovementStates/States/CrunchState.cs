using UnityEngine;

public class CrunchState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.animator.SetBool("Crunching", true);
        
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ExitState(movement, movement.run);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (movement.direction.magnitude < 0.1f)
            {
                ExitState(movement, movement.idle);
            }
            else
            {
                ExitState(movement, movement.walk);
            }
        }

        if (movement.vInput < 0)
        {
            movement.currentMoveSpeed = movement.crunchBackSpeed;
        }
        else
        {
            movement.currentMoveSpeed = movement.crunchSpeed;
        }
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.animator.SetBool("Crunching", false);
        movement.SwitchState(state);
        
    }
}
