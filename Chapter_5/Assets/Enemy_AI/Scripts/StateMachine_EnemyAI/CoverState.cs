using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class CoverState : MonoBehaviour, IState
{
    public event Action OnNpcTakeCover; //Being listened for in Actions script.
    public event Action OnNPCSquat; //Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    private GameObject[] coverSpots;

    [Tooltip("Speed at which NavMeshAgent moves while seeking Cover")]
    [SerializeField] private float npcSpeed = 2.5f;

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent
    private Transform player; // Reference to the Player transform
    private Vector3 target; //target is the selected hiding spot
    private float offset = 1.5f;

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[CoverState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[CoverState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
        player = GameObject.FindWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Couldn't find Player with tag 'Player'");
            return;
        }

        coverSpots = GameObject.FindGameObjectsWithTag("Obstacle");
    }

    void IState.OnStateEnter()
    {
        if (agent.enabled)
        {
            agent.speed = npcSpeed;
            agent.isStopped = false;

            //provide 50% chance of NPC moving to either closest or farthest hiding spot for Cover 
            if (Random.Range(0, 100) < 50)
                target = FindClosestCover();
            else
                target = FindFarthestCover();

            agent.SetDestination(target);
        }
        
        OnNpcTakeCover?.Invoke(); //Being listened for in Actions script.

        //(this as IState).IsActive = true; //Patrol not default starting State, so Not initialized in Awake()
        Debug.Log("CoverState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {

        // Check if the agent has reached its current destination
        if (agent.enabled && agent.remainingDistance <= agent.stoppingDistance)
        {
            //NPC reached hiding spot so let him squat.
            agent.isStopped = true;
            OnNPCSquat?.Invoke();
        }

        if (stateMachine.IsPlayerVisible() && GetComponent<Health>().CurrentHealth >= 50)
        {
            IState chase = stateMachine.states.Find(s => s.GetType() == typeof(ChaseState));
            stateMachine.SwitchState(chase);
        }
        else if(GetComponent<Health>().CurrentHealth >= 50 && !stateMachine.IsPlayerVisible())
        {
            //provide 50% chance of entering into either Wander or Patrol states
            if (Random.Range(0, 100) < 50)
            {
                IState patrol = stateMachine.states.Find(s => s.GetType() == typeof(PatrolState));
                Debug.Log("CoverState: Switching to Patrol State");
                stateMachine.SwitchState(patrol);
            }
            else
            {
                IState wander = stateMachine.states.Find(s => s.GetType() == typeof(WanderState));
                Debug.Log("CoverState: Switching to Wander State");
                stateMachine.SwitchState(wander);
            }
        }
    }

    void IState.OnStateExit()
    {
        //(this as IState).IsActive = false; //helpful in debugging

        Debug.Log("CoverState: Exit");
    }

    private Vector3 FindClosestCover()
    {
        // Check if there are any cover spots available
        if (coverSpots.Length == 0)
        {
            Debug.LogError("No Obstacles found for NPC to hide behind.");
            return transform.position;  // Return current NPC position if no covers are found
        }

        float minDistance = Mathf.Infinity;
        Vector3 bestHidingSpot = transform.position;
        GameObject closestHidingSpot = null;

        foreach (GameObject coverSpot in coverSpots)
        {
            // Calculate the distance from the current cover spot to the player
            float distanceToPlayer = Vector3.Distance(player.position, coverSpot.transform.position);

            if (distanceToPlayer < minDistance)
            {
                minDistance = distanceToPlayer;
                closestHidingSpot = coverSpot;
            }
        }

        // Calculate the direction from the closest cover spot obtained to player 
        Vector3 coverToPlayer = player.position - closestHidingSpot.transform.position;

        //Get the Collider component to determine size
        Collider obstacleCollider = closestHidingSpot.GetComponent<Collider>();
        if (obstacleCollider == null)
        {
            Debug.LogError($"CoverState: Obstacle: '{closestHidingSpot.name}' not fitted with a Collider");
            return Vector3.zero;
        }
        else
        {
            // Compute an appropriate offset based on the obstacle's collider size
            float obstacleOffset = Mathf.Max(obstacleCollider.bounds.size.x, obstacleCollider.bounds.size.z) / 2.0f + offset;

            // Find a potential hiding spot behind the cover spot with the adjusted offset
            Vector3 potentialHidingSpot = closestHidingSpot.transform.position - coverToPlayer.normalized * obstacleOffset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialHidingSpot, out hit, obstacleOffset, NavMesh.AllAreas))
                bestHidingSpot = hit.position;
            else
            {
                bestHidingSpot = Vector3.zero;
                Debug.LogError($"CoverState: NPC cannot be positioned behind Hiding Spot: '{closestHidingSpot.name}'");
            }
            return bestHidingSpot;
        }
    }

    private Vector3 FindFarthestCover()
    {
        // Check if there are any cover spots available
        if (coverSpots.Length == 0)
        {
            Debug.LogError("No Obstacles found for NPC to hide behind.");
            return transform.position;  // Return current NPC position if no covers are found
        }

        float maxDistance = Mathf.NegativeInfinity;
        Vector3 bestHidingSpot = transform.position;
        GameObject farthestHidingSpot = null;

        foreach (GameObject coverSpot in coverSpots)
        {
            // Calculate the distance from the current cover spot to the player
            float distanceToPlayer = Vector3.Distance(player.position, coverSpot.transform.position);

            if (distanceToPlayer > maxDistance)
            {
                maxDistance = distanceToPlayer;
                farthestHidingSpot = coverSpot;
            }
        }

        // Calculate the direction from the farthest cover spot obtained to player 
        Vector3 coverToPlayer = player.position - farthestHidingSpot.transform.position;

        //Get the Collider component to determine size
        Collider obstacleCollider = farthestHidingSpot.GetComponent<Collider>();
        if (obstacleCollider == null)
        {
            Debug.LogError($"CoverState: Obstacle: '{farthestHidingSpot.name}' not fitted with a Collider");
            return Vector3.zero;
        }
        else
        {
            // Compute an appropriate offset based on the obstacle's collider size
            float obstacleOffset = Mathf.Max(obstacleCollider.bounds.size.x, obstacleCollider.bounds.size.z) / 2.0f + offset;

            // Find a potential hiding spot behind the cover spot with the adjusted offset
            Vector3 potentialHidingSpot = farthestHidingSpot.transform.position - coverToPlayer.normalized * obstacleOffset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialHidingSpot, out hit, obstacleOffset, NavMesh.AllAreas))
                bestHidingSpot = hit.position;
            else
            {
                bestHidingSpot = Vector3.zero;
                Debug.LogError($"CoverState: NPC cannot be positioned behind Hiding Spot: '{farthestHidingSpot.name}'");
            }
            return bestHidingSpot;
        }
    }
}
