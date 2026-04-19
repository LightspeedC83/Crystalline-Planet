using UnityEngine;

public class StandingState : State
{
    float gravityValue;
    bool jump;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;

    Vector3 characterVelocity;

    public StandingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }
}
