using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class IdleState : MonoBehaviour, IState
{
    public event Action OnNpcIdle; //Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[IdleState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        //(this as IState).IsActive = true; //Idle is the default starting State.

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[IdleState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {
        if(agent.enabled)
            agent.isStopped = true;
        
        OnNpcIdle?.Invoke(); //Being listened for in Actions script.
        //(this as IState).IsActive = true; //helpful in debugging
        Debug.Log("IdleState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (stateMachine.IsPlayerVisible())
        {
            IState chase = stateMachine.states.Find(s => s.GetType() == typeof(ChaseState));
            stateMachine.SwitchState(chase);
        }
        else//Player not visible - provide  provide 50% chance of entering either Wander or Patrol states
        {
            if (Random.Range(0, 100) < 50)
            {
                IState patrol = stateMachine.states.Find(s => s.GetType() == typeof(PatrolState));
                Debug.Log("IdleState: Switching to Patrol State");
                stateMachine.SwitchState(patrol);
            }
            else
            {
                IState wander = stateMachine.states.Find(s => s.GetType() == typeof(WanderState));
                Debug.Log("IdleState: Switching to Wander State");
                stateMachine.SwitchState(wander);
            }
        }
                
    }

    void IState.OnStateExit()
    {        
        //(this as IState).IsActive = false; //helpful in debugging

        Debug.Log("IdleState: Exit");
    }

}
