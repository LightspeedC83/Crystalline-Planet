using UnityEngine;

public class StateMachine
{
    private State currentState;

    public void Initialize(State startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(State newState)
    {
        //leaves current state in case it has any ending effects
        currentState.Exit();

        //Updates current state, then enters the new one
        currentState = newState;
        currentState.Enter();
    }

    public State GetActiveState()
    {
        return currentState;
    }
}
