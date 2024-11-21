using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class PatrolState : MonoBehaviour, IState
{
    public event Action OnNpcPatrol; //Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    [Tooltip("Patrol points assigned to this NPC")]
    [SerializeField] private Transform[] wayPoints;

    [Tooltip("Speed at which NavMeshAgent moves")]
    [SerializeField] private float npcSpeed = 1.5f;

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent
    private Vector3 target; //target is the Patrol point
    private int lastWaypointIndex = -1;// Initialize to an impossible index

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[PatrolState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[PatrolState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {
        if (agent.enabled)
        {
            agent.speed = npcSpeed;
            agent.isStopped = false;
            target = FindNextPoint();
            agent.SetDestination(target);
        }
        
        OnNpcPatrol?.Invoke(); //Being listened for in Actions script.

        //(this as IState).IsActive = true; //Patrol not default starting State, so Not initialized in Awake()
        Debug.Log("PatrolState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (agent.enabled)
        {
            // Check if the agent has reached its current destination
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    target = FindNextPoint();
                    agent.SetDestination(target);
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

        Debug.Log("PatrolState: Exit");
    }

    private Vector3 FindNextPoint()// Find the next random patrol point
    {
        if (wayPoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned to Patrol behavior.");
            return transform.position; // Return current position if no waypoints.
        }
        int rndIndex;
        do
        {
            rndIndex = Random.Range(0, wayPoints.Length);
        } while (wayPoints.Length > 1 && rndIndex == lastWaypointIndex);//Ensure it's not the same if there's more than one point

        lastWaypointIndex = rndIndex;//Store the index of the chosen waypoint
        return wayPoints[rndIndex].position;
    }

}
