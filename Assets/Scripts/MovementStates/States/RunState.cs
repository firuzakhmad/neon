using UnityEngine;

public class RunState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.animator.SetBool("Running", true);

    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            ExitState(movement, movement.walk);
        }
        else if (movement.direction.magnitude < 0.1f)
        {
            ExitState(movement, movement.idle);
        }

        if (movement.vInput < 0)
        {
            movement.currentMoveSpeed = movement.runBackSpeed;
        }
        else
        {
            movement.currentMoveSpeed = movement.RunSpeed;
        }
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.animator.SetBool("Running", false);
        movement.SwitchState(state);
        
    }
}
