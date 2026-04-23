using UnityEngine;

public class DoubleJumpingState : State
{
    private bool grounded;
    private Vector3 boostVector;
    //private Vector3 airVelocity;

    public DoubleJumpingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        grounded = false;
        //Animation should trigger here
        DoubleJump();
        playerController.hasDoubleJump = false;
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
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (moveInput != Vector2.zero)
        {
            Vector3 move = playerController.transform.forward * moveInput.y + playerController.transform.right * moveInput.x;
            move *= playerController.movementSpeed * Time.deltaTime * playerController.airControl;
            float dot = Vector3.Dot(move, boostVector);

            //TODO: Use projection to get movement as vectors parallel and perpendicular to boostVector. Increment boostVector up or down based on magnitude of parallel input vector, lerp boostVector to the side by a factor related to the magnitude of the perpendicular vectors
            //SEE DIAGRAM IN SKETCHBOOK

            //playerController.characterVelocity = move;
            playerController.characterVelocity = Time.deltaTime * boostVector;

            //Lerps the direction of boostVector toward the direction of input, based on the adjustment constant.
            boostVector = Vector3.Lerp(boostVector, move.normalized * boostVector.magnitude, playerController.doubleJumpAdjustmentConstant);

        }
        else
        {
            playerController.characterVelocity -= playerController.characterVelocity * playerController.aerialFrictionConstant; // If the player is in the air, drag is reduced
            /*if (playerController.characterVelocity.magnitude < boostVector.magnitude * Time.deltaTime)
            {
                playerController.characterVelocity = boostVector * Time.deltaTime;
            }*/
            //Debug.Log("friction applied");
        }
        playerController.verticalVelocity += playerController.gravity * Time.deltaTime;
        playerController.characterVelocity.y = playerController.verticalVelocity * Time.deltaTime;

        //Move the character
        playerController.characterController.Move(playerController.characterVelocity);

        //update grounded state
        grounded = playerController.characterController.isGrounded;
    }

    private void DoubleJump()
    {
        playerController.verticalVelocity = playerController.doubleJumpForce;
        boostVector =  playerController.doubleJumpForwardBoost * playerController.transform.forward;
        Debug.Log("playerController.transform.forward: " + playerController.transform.forward);
    }
}
