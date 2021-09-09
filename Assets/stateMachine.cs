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

    public void ChangeState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null) currentState.Update();
    }
}