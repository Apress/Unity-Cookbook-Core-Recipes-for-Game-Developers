using UnityEngine;
using UnityEngine.AI;
using System;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class DeathState : MonoBehaviour, IState
{
    public event Action PlayNpcDeadAnim;//Being listened for in Actions script.

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the warrior NPC Agent
    private float destroyDelay = 5f;
    private bool isNpcDead;

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if(stateMachine == null)
        {
            Debug.LogError($"[DeathState - {gameObject.name}] :  No State Machine found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[DeathState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {        
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
        isNpcDead = true;
        PlayNpcDeadAnim?.Invoke();//Plays NPC Death Animation.
        Debug.Log("DeathState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (isNpcDead)
        {
            IState Idle = stateMachine.states.Find(s => s.GetType() == typeof(IdleState));
            stateMachine.SwitchState(Idle);
        }

        Debug.Log("DeathState: Update");
    }
        
    void IState.OnStateExit()
    {
        Debug.Log("DeathState: Exit");
        Destroy(gameObject, destroyDelay);
    }  
}
