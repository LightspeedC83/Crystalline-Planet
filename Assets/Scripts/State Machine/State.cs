using UnityEngine;
using UnityEngine.InputSystem;

public class State
{
    public PlayerController playerController;
    public StateMachine stateMachine;

    // These are just useful for multiple States
    protected Vector2 moveInput;
    public static bool isMoving;
    public static bool jumpKeyDown;

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

        moveAction.performed += StartMoving;
        moveAction.canceled += StopMoving;
        jumpAction.performed += StartJumping;
        jumpAction.canceled += StopJumping;
    }

    public virtual void Enter()
    {
        //This debug statement prints the new state on each state change
        //Debug.Log("enter state: " + this.ToString());
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

    public void StopMoving(InputAction.CallbackContext context)
    {
        isMoving = false;
    }

    public void StartMoving(InputAction.CallbackContext context)
    {
        isMoving = true;
    }

    public void StartJumping(InputAction.CallbackContext context)
    {
        jumpKeyDown = true;
    }

    public void StopJumping(InputAction.CallbackContext context)
    {
        jumpKeyDown = false;
    }
}
