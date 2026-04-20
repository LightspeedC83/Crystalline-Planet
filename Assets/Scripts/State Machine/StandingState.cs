using UnityEngine;

public class StandingState : State
{
    float gravityValue;
    bool isJumping;
    bool isMoving;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;

    Vector3 characterVelocity;

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
        currentVelocity = Vector3.zero;
        grounded = playerController.characterController.isGrounded;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        // Identifies which actions have been taken, records inputs that matter
        if (moveAction.triggered)
        {
            isMoving = true;
            moveInput = moveAction.ReadValue<Vector2>();
        }
        if (jumpAction.triggered && grounded)
        {
            isJumping = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //Animator logic update here

        // Updates the State Machine. The order of these matters, as if multiple are true, the last one will override.
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

        if (isMoving)
        {
            playerController.Move(moveInput);
        } else
        {
            playerController.Move(Vector2.zero);
        }
    }

    public override void Exit()
    {
        base.Exit();

        //Don't think I need to do anything else here at the moment
    }
}
