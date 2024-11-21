using UnityEngine;
using UnityEngine.AI;
//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class NPCTestState : MonoBehaviour, IState
{
    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    private float timer;
    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent
    private Transform player;  // Reference to the Player transform

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
    }  

    void IState.OnStateEnter()
    {
        //(this as IState).IsActive = true;
        Debug.Log("NPCTestState: Enter");
        timer = 5f;
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        timer -= deltaTime;
        Debug.Log($"NPCTestState: Update : {timer}");

        if(timer <= 0)
            stateMachine.SwitchState(this);
    }

    void IState.OnStateExit()
    {
        Debug.Log("NPCTestState: Exit");
        //(this as IState).IsActive = false;//helpful in debugging. 
    }

}
