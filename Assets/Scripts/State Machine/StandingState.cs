using UnityEngine;

public class StandingState : State
{
    bool isJumping;
    bool grounded;


    public StandingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        isJumping = false;
        isMoving = false;
        moveInput = Vector2.zero;
        grounded = playerController.characterController.isGrounded;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        // Identifies which actions have been taken, records inputs that matter
        if (isMoving)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        if (jumpKeyDown && grounded)
        {
            isJumping = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //Animator logic update here

        // Updates the State Machine. The order of these matters, as if multiple are true, the last one will override.
        if (!grounded)
        {
            stateMachine.ChangeState(playerController.falling);
        }
        if (isMoving)
        {
            stateMachine.ChangeState(playerController.moving);
        }
        if (isJumping)
        {
            stateMachine.ChangeState(playerController.jumping);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        playerController.characterVelocity -= playerController.characterVelocity * playerController.frictionConstant;

        playerController.characterVelocity.y = playerController.verticalVelocity * Time.deltaTime;
        playerController.characterController.Move(playerController.characterVelocity);

        grounded = playerController.characterController.isGrounded;
    }

    public override void Exit()
    {
        base.Exit();

        //Don't think I need to do anything else here at the moment
    }
}
