using UnityEngine;

public class HardLandingState : State
{
    private bool isJumping;
    private float stunTimer;
    private float jumpTimer;
    private bool jumpReleased;

    public HardLandingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        //TODO: animation, sound
        isJumping = false;
        stunTimer = 0f;
        jumpTimer = 0;
        jumpReleased = false;
        playerController.hasDoubleJump = true;
        playerController.hasDive = true;
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
            jumpTimer += Time.deltaTime;
            jumpReleased = false;
        } else
        {
            jumpReleased = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (jumpTimer >= playerController.superJumpChargeTime)
        {
            //TODO: sound to indicate jump is ready. Visual if time allows.
        }
        if (jumpReleased && jumpTimer >= playerController.superJumpChargeTime)
        {
            //If we've charged up a jump and the button is released
            stateMachine.ChangeState(playerController.superJumping);
        } else if (stunTimer >= 0.5f && jumpReleased)
        {
            if (isMoving)
            {
                stateMachine.ChangeState(playerController.moving);
            }
            else
            {
                stateMachine.ChangeState(playerController.standing);
            }
            if (isJumping)
            {
                stateMachine.ChangeState(playerController.jumping);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        stunTimer += Time.deltaTime;
    }
}
