using UnityEngine;

public class LandingState : State
{
    private bool isMoving;
    private bool isJumping;

    public LandingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        if (!moveAction.ReadValue<Vector2>().Equals(Vector2.zero)) {
            isMoving = true;
        } else { isMoving = false; }
        //Begin animation/particles/whatever
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if (moveAction.triggered)
        {
            //This is lowkey terrible code, but not a problem yet. isMoving can't be reset to false, its only set to true when buttons are pressed. Same problem in other places
            isMoving = true;
            moveInput = moveAction.ReadValue<Vector2>();
        }
        if (jumpAction.triggered)
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
