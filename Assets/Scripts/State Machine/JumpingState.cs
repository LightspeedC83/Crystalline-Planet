using UnityEngine;

public class JumpingState : State
{
    public JumpingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }
}
