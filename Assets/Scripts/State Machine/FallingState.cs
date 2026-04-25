using UnityEngine;

public class FallingState : State
{
    private bool grounded;
    private float coyoteTimer;

    //private Vector3 airVelocity;

    public FallingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        grounded = false;
        playerController.verticalVelocity = 0;
        coyoteTimer = 0;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        moveInput = moveAction.ReadValue<Vector2>();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (grounded)
        {
            stateMachine.ChangeState(playerController.landing);
        } else if (diveAction.triggered && playerController.hasDive)
        {
            stateMachine.ChangeState(playerController.diving);
        } else if (jumpKeyDown && coyoteTimer <= playerController.coyoteTime)
        {
            stateMachine.ChangeState(playerController.jumping);
        } else if (jumpKeyDown && coyoteTimer > playerController.coyoteTime && playerController.hasDoubleJump)
        {
            stateMachine.ChangeState(playerController.doubleJumping);
        }
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (moveInput != Vector2.zero)
        {
            Vector3 move = playerController.transform.forward * moveInput.y + playerController.transform.right * moveInput.x;
            move *= playerController.movementSpeed * Time.deltaTime;

            playerController.characterVelocity = move;
        }
        else
        {
            playerController.characterVelocity -= playerController.characterVelocity * playerController.aerialFrictionConstant; // If the player is in the air, drag is reduced
            //Debug.Log("friction applied");
        }
        playerController.verticalVelocity += playerController.gravity * Time.deltaTime;
        playerController.characterVelocity.y = playerController.verticalVelocity * Time.deltaTime;

        playerController.characterController.Move(playerController.characterVelocity * playerController.airControl);

        //update grounded state and coyote Timer
        grounded = playerController.characterController.isGrounded;
        coyoteTimer += Time.deltaTime;
    }
}