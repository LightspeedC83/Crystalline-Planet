using UnityEngine;
using UnityEngine.InputSystem;

public class MovingState : State
{
    private bool isMoving;
    private bool isJumping;
    private bool grounded;
    public MovingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        isMoving = true;
        isJumping = false;
        grounded = playerController.characterController.isGrounded;

        moveAction.canceled += StopMoving;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        moveInput = moveAction.ReadValue<Vector2>();
        if (jumpAction.triggered && grounded)
        {
            isJumping = true;
        }
        // Movement stopping is handled separately (hopefully that's fine)
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!isMoving)
        {
            stateMachine.ChangeState(playerController.standing);
        }
        if (isJumping)
        {
            stateMachine.ChangeState(playerController.jumping);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        playerController.Move(moveInput);
    }

    public override void Exit()
    {
        base.Exit();

        //Removes StopMoving from the event call while not in this state
        moveAction.canceled -= StopMoving;
    }

    private void StopMoving(InputAction.CallbackContext context)
    {
        isMoving = false;
    }


}
