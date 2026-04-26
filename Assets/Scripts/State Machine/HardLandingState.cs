using UnityEngine;

public class HardLandingState : State
{
    private bool isJumping;
    private float stunTimer;
    private float jumpTimer;
    private bool jumpReleased;
    private bool minSoundPlayed;
    private bool maxSoundPlayed;

    public HardLandingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        //TODO: animation, sound
        playerController.audioSource.PlayOneShot(playerController.hardLandingSound);

        playerController.characterVelocity = Vector3.zero;
        playerController.jumpChargeDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);

        isJumping = false;
        stunTimer = 0f;
        jumpTimer = 0;
        jumpReleased = false;
        minSoundPlayed = false;
        maxSoundPlayed = false;
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
            //If the charge is over the max, set it to the max
            jumpTimer = Mathf.Min(jumpTimer, playerController.superJumpMaxCharge);
            jumpReleased = false;
        } else
        {
            jumpReleased = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //Play sounds if needed
        if (jumpTimer < playerController.superJumpMinCharge)
        {
            playerController.jumpChargeDisplay.localScale.Set((jumpTimer / playerController.superJumpMinCharge), 1, 1);
            playerController.jumpChargeDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (jumpTimer / playerController.superJumpMinCharge) * 200);
            
        } else if (jumpTimer >= playerController.superJumpMinCharge && !minSoundPlayed)
        {
            playerController.audioSource.PlayOneShot(playerController.minChargeSound);
            minSoundPlayed = true;
        }
        if (jumpTimer >= playerController.superJumpMaxCharge && !maxSoundPlayed)
        {
            playerController.jumpChargeDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (jumpTimer / playerController.superJumpMinCharge) * 300);
            playerController.audioSource.PlayOneShot(playerController.maxChargeSound);
            maxSoundPlayed = true;
        }

        //If we've charged up a jump and the button is released, set the jump charge and jump. If the jump is released and we are out of stun, move to other states.
        if (jumpReleased && jumpTimer >= playerController.superJumpMinCharge)
        {
            playerController.superJumping.SetCharge(jumpTimer / playerController.superJumpMaxCharge);
            stateMachine.ChangeState(playerController.superJumping);
        } else if (stunTimer >= playerController.hardLandStunTime && jumpReleased)
        {
            if (playerController.characterController.isGrounded)
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
            } else
            {
                stateMachine.ChangeState(playerController.falling);
            }
            
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        stunTimer += Time.deltaTime;
    }
}
