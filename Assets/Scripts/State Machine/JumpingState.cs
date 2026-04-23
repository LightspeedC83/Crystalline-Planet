using UnityEngine;

public class JumpingState : State
{
    private bool grounded;
    //private Vector3 airVelocity;

    public JumpingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        grounded = false;
        //Animation should trigger here
        Jump();
        jumpKeyDown = false; //This is bad code, since the space key might still be pressed. Guarantees that the key has to be hit again to activate the double jump.
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
        }
        if (jumpKeyDown && playerController.hasDoubleJump)
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

        //update grounded state
        grounded = playerController.characterController.isGrounded;
    }

    private void Jump()
    {
        playerController.verticalVelocity = playerController.jumpForce;
    }
}
