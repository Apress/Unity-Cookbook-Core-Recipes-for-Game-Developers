using System;
using UnityEngine;
using UnityEngine.AI;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class ChaseState : MonoBehaviour, IState
{
    public event Action OnNpcChase;//Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    [Tooltip("Speed at which NavMeshAgent moves while Chasing")]
    [SerializeField] private float npcSpeed = 4f;

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent
    private IState previousState;

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[ChaseState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[ChaseState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {
        if (agent.enabled)
        {
            agent.speed = npcSpeed;
            agent.isStopped = false;
        }
        OnNpcChase?.Invoke(); //Being listened for in Actions script.

        //(this as IState).IsActive = true; //Chase not default starting State, so Not initialized in Awake()
        Debug.Log("ChaseState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        //Check if the previous state is not of type ChaseState as NPC could continuously chase player 
        if (stateMachine.PreviousState.GetType() != typeof(ChaseState)) //ACC
        {
            //previous state can be either Patrol or Wander
            previousState = stateMachine.PreviousState;
        }

        if (!stateMachine.IsPlayerVisible())
        {
            //If coming from Patrol to Chase state, switch back to Patrol
            //If coming from Wander to Chase state, switch back to Wander
            stateMachine.SwitchState(previousState);
            return;
        }

        //if Player is visible to NPC
        if(agent.enabled)
            agent.SetDestination(stateMachine.Player.position);

        if(agent.enabled && agent.hasPath)
        {
            if (stateMachine.IsPlayerAttackable())
            {
                Debug.Log("ChaseState: Player in Attackable Range");

                IState attack = stateMachine.states.Find(s => s.GetType() == typeof(AttackState));
                stateMachine.SwitchState(attack);
            }
        }

    }

    void IState.OnStateExit()
    {
        //(this as IState).IsActive = false; //helpful in debugging
        Debug.Log("ChaseState: Exit");
    }
}
