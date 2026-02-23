using UnityEngine;

public class WalkState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.animator.SetBool("Walking", true);
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ExitState(movement, movement.run);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ExitState(movement, movement.crunch);
        }
        else if (movement.direction.magnitude < 0.1f)
        {
            ExitState(movement, movement.idle);
        }

        if (movement.vInput < 0)
        {
            movement.currentMoveSpeed = movement.walkBackSpeed;
        }
        else
        {
            movement.currentMoveSpeed = movement.walkSpeed;
        }
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.animator.SetBool("Walking", false);
        movement.SwitchState(state);
        
    }
}
