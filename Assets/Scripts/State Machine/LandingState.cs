using UnityEngine;

public class LandingState : State
{
    private bool isJumping;

    public LandingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        //Play an animation if I need to
        isJumping = false;
        playerController.hasDoubleJump = true;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if (isMoving)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        if (jumpKeyDown)
        {
            isJumping = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (isMoving)
        {
            stateMachine.ChangeState(playerController.moving);
        } else
        {
            stateMachine.ChangeState(playerController.standing);
        }
        if (isJumping)
        {
            stateMachine.ChangeState(playerController.jumping);
        }
    }
}
