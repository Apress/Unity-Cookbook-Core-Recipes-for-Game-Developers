using UnityEngine;
public abstract class StateMachine : MonoBehaviour
{
    private IState currentState;
    private IState previousState;  //Track the previous state

    //Properties
    public IState PreviousState => previousState;
    public void SwitchState(IState newState)
    {
        currentState?.OnStateExit();
        previousState = currentState;  //Update previous state before switching
        currentState = newState;
        currentState?.OnStateEnter();
    }

    private void Update()
    {
        //passing in the Time.deltaTime as a parameter is optional
        currentState?.OnStateUpdate(Time.deltaTime);
    }
}
