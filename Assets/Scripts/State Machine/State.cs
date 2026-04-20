using UnityEngine;
using UnityEngine.InputSystem;

public class State
{
    public PlayerController playerController;
    public StateMachine stateMachine;

    protected Vector2 moveInput;

    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;

    public State(PlayerController playerController, StateMachine stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;

        moveAction = playerController.GetPlayerInput().actions["Move"];
        lookAction = playerController.GetPlayerInput().actions["Look"];
        jumpAction = playerController.GetPlayerInput().actions["Jump"];
    }

    public virtual void Enter()
    {
        Debug.Log("enter state: " + this.ToString());
    }

    public virtual void HandleInput()
    {
        //Check each action that can be triggered in this state, and if triggered, store information in the State's variables
    }

    public virtual void LogicUpdate()
    {
        // animation updates, change states based on updated variables from HandleInput
    }

    public virtual void PhysicsUpdate()
    {
        // Actually move the character based on updated variables from HandleInput
    }

    public virtual void Exit()
    {

    }


}
