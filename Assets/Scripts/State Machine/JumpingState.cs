using UnityEngine;

public class JumpingState : State
{
    private bool grounded;
    private Vector3 airVelocity;

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
        playerController.Jump();
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

        //If not gronded, move with reduced control. Update grounded.
        if (!grounded)
        {
            playerController.Move(moveInput * playerController.airControl);
            grounded = playerController.characterController.isGrounded;
        }
    }
}
