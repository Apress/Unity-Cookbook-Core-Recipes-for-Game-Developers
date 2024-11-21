using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class WanderState : MonoBehaviour, IState
{
    public event Action OnNpcWander; //Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    [Tooltip("Speed at which NavMeshAgent moves")]
    [SerializeField] private float npcSpeed = 1.5f;

    [Tooltip("Area around NPC where it can Wander to")]
    [SerializeField] private float navigationRadius = 7.0f;

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent
    private Vector3 target; //target is the random Wander location for NPC Agent
    private bool isTargetSet;

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[WanderState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[WanderState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {
        if (agent.enabled)
        {
            agent.speed = npcSpeed;
            agent.isStopped = false;
            isTargetSet = false; // Reset target selection on state enter
        }

        OnNpcWander?.Invoke(); //Being listened for in Actions script.

        //(this as IState).IsActive = true; //Patrol not default starting State, so Not initialized in Awake()
        Debug.Log("WanderState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (agent.enabled)
        {
            // Check if the agent has reached its wander target or if no target is set
            if (!isTargetSet || agent.remainingDistance < agent.stoppingDistance)
            {
                // Calculate a new random target 
                Vector3 randomDirection = Random.insideUnitSphere * navigationRadius;
                randomDirection += agent.transform.position;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, navigationRadius, NavMesh.AllAreas))
                {
                    target = hit.position;
                    agent.SetDestination(target);
                    isTargetSet = true; // Mark target as set
                }
            }
        }

        if (stateMachine.IsPlayerVisible())
        {
            IState chase = stateMachine.states.Find(s => s.GetType() == typeof(ChaseState));
            stateMachine.SwitchState(chase);
        }

    }

    void IState.OnStateExit()
    {
        //(this as IState).IsActive = false; //helpful in debugging

        Debug.Log("WanderState: Exit");
    }

}
