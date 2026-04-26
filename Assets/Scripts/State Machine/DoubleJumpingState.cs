using System;
using UnityEngine;

public class DoubleJumpingState : State
{
    private bool grounded;
    private Vector3 boostVector;
    private Vector3 parallelVector;
    private Vector3 perpendicularVector;

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
        playerController.audioSource.PlayOneShot(playerController.doubleJumpSound);
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
        } else if (diveAction.triggered && playerController.hasDive){
            stateMachine.ChangeState(playerController.diving);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        Vector3 move = playerController.transform.forward * moveInput.y + playerController.transform.right * moveInput.x;
        //Component of the input vector that is parallel to the boost
        parallelVector = Vector3.Project(move, boostVector);
        //Component of the input vector that is perpendicular to the boost
        perpendicularVector = move - parallelVector;

        float dot = Vector3.Dot(parallelVector.normalized, boostVector.normalized);
        boostVector += parallelVector * playerController.doubleJumpSteeringConstant * (1.5f - dot);
        boostVector += perpendicularVector * playerController.doubleJumpSteeringConstant;

        playerController.characterVelocity = Time.deltaTime * boostVector;

        playerController.characterVelocity -= playerController.characterVelocity * playerController.aerialFrictionConstant;


        playerController.verticalVelocity += playerController.gravity * Time.deltaTime;
        playerController.characterVelocity.y = playerController.verticalVelocity * Time.deltaTime;

        //Move the character
        playerController.characterController.Move(playerController.characterVelocity);

        //update grounded state
        grounded = playerController.characterController.isGrounded;
    }

    private void DoubleJump()
    {
        //Double jump's vertical force is additive, with a minimum upward velocity of the double jump force
        playerController.verticalVelocity = Math.Max(playerController.doubleJumpForce, playerController.characterVelocity.y + playerController.doubleJumpForce);
        boostVector =  playerController.doubleJumpForwardBoost * playerController.transform.forward;
        Debug.Log("playerController.transform.forward: " + playerController.transform.forward);
    }
}
