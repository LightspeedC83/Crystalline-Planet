using UnityEngine;

public class DivingState : State
{

    private Vector3 diveVector;
    private bool grounded;
    private float fallTimer;
    public DivingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        grounded = false;
        Dive();
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (grounded)
        {
            stateMachine.ChangeState(playerController.hardLanding);
        } else if (jumpAction.triggered && playerController.hasDoubleJump)
        {
            stateMachine.ChangeState(playerController.doubleJumping);
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        playerController.characterVelocity = diveVector * Time.deltaTime;
        //Multiplies gravity by fall timer, less gravity than normal until past 1 second
        playerController.verticalVelocity += playerController.gravity * Time.deltaTime * fallTimer;

        playerController.characterVelocity.y = playerController.verticalVelocity * Time.deltaTime;

        //Actually perform the movement
        playerController.characterController.Move(playerController.characterVelocity);

        grounded = playerController.characterController.isGrounded;
        fallTimer += Time.deltaTime;
    }

    private void Dive()
    {
        diveVector = playerController.cineCamera.transform.forward * playerController.diveForce;

        // Make the dive weaker the more up it points, to a minimum of 30%
        float dot = Vector3.Dot(Vector3.up, diveVector.normalized);
        if (dot > 0)
        {
            diveVector *= (1.3f - dot);
        }

        playerController.verticalVelocity = diveVector.y;

        playerController.hasDive = false;
        fallTimer = 0.5f;
    }

    //Called when the dive runs into a non flat surface
    public void LandDive()
    {
        //If the player landed on a slope that counts as grounded, change to hardLanding. Otherwise, play sound and start falling.
        if (playerController.characterController.isGrounded)
        {
            stateMachine.ChangeState(playerController.hardLanding);
        } else
        {
            playerController.audioSource.PlayOneShot(playerController.hardLandingSound);
            stateMachine.ChangeState(playerController.falling);
        }
    }
}
