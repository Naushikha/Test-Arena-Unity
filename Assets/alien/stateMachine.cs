// https://forum.unity.com/threads/c-proper-state-machine.380612/
public interface IState
{
    public void Enter();
    public void Update();
    public void Exit();
}

public class StateMachine
{
    IState currentState;
    IState previousState;

    public void ChangeState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();

        previousState = currentState;
        currentState = newState;
        currentState.Enter();
    }
    public IState GetPreviousState()
    {
        return previousState;
    }
    public IState GetCurrentState()
    {
        return currentState;
    }

    public void Update()
    {
        if (currentState != null) currentState.Update();
    }
}