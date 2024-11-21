using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_CoverState : MonoBehaviour, IState
{
    private GameObject[] coverSpots;

    [Tooltip("Speed at which NavMeshAgent moves while seeking Cover")]
    [SerializeField] private float npcSpeed = 2.5f;

    private WarriorStateMachine stateMachine;
    private Transform player; // Reference to the Player transform
    private Vector3 target; //target is the selected hiding spot
    private float offset = 1.5f;
    private GameObject lastHidingSpot;//Avoid repeatedly finding the same hiding spot.

    private const float crossFadeTime = 0.1f;
    private readonly int movementHash = Animator.StringToHash("Run");
    private readonly int idleHash = Animator.StringToHash("Idle");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[CoverState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }

        player = GameObject.FindWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError($"[CoverState - {gameObject.name}] : Couldn't find player with tag Player");
            return;
        }

        coverSpots = GameObject.FindGameObjectsWithTag("Obstacle");
    }

    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            stateMachine.Agent.speed = npcSpeed;
            stateMachine.Agent.isStopped = false;

            //provide 50% chance of NPC moving to either closest or farthest hiding spot for Cover 
            if (Random.Range(0, 100) < 50)
                target = FindClosestCover();
            else
                target = FindFarthestCover();

            GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
            //Deactivate the Swords collider in Cover state
            meleeWeapon.GetComponent<Collider>().enabled = false;

            stateMachine.Agent.ResetPath();
            stateMachine.Agent.SetDestination(target);
        }
        else
            return;

        stateMachine.Anim.CrossFadeInFixedTime(movementHash, crossFadeTime);

        Debug.Log("CoverState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        Health health = GetComponent<Health>();

        if (health == null)
            return;

        // Check if the agent has reached its current destination
        if (stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance || !stateMachine.Agent.hasPath)
        {
            //NPC reached hiding spot so let him stop.
            stateMachine.Agent.isStopped = true;
            stateMachine.Anim.CrossFadeInFixedTime(idleHash, crossFadeTime);//Play Idle animation
        }
        else if(stateMachine.Agent.remainingDistance > stateMachine.Agent.stoppingDistance && health.CurrentHealth < 50 )
        {
            stateMachine.Agent.isStopped = false;
            stateMachine.Anim.CrossFadeInFixedTime(movementHash, crossFadeTime);//Continue run animation
        }

        if (stateMachine.IsPlayerVisible() && health.CurrentHealth >= 50)
        {
            IState chase = stateMachine.FindState<W_ChaseState>();
            if (chase != null)
                stateMachine.SwitchState(chase);
        }
        else if(health.CurrentHealth >= 50 && !stateMachine.IsPlayerVisible())
        {
                IState wander = stateMachine.FindState<W_WanderState>();
                if (wander != null)
                {
                    Debug.Log("CoverState: Switching to Wander State");
                    stateMachine.SwitchState(wander);
                }            
        }

        Debug.Log("CoverState: Update");
    }

    void IState.OnStateExit()
    {
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
            //Exclude the last hiding spot from consideration
            if (coverSpot == lastHidingSpot)
            {
                //Debug.Log($"FindClosestCover(): Most recent hiding spot:  {lastHidingSpot.name}");
                continue;
            }

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

            if (NavMesh.SamplePosition(potentialHidingSpot, out NavMeshHit hit, obstacleOffset, NavMesh.AllAreas))
            {
                bestHidingSpot = hit.position;
                lastHidingSpot = closestHidingSpot; //ACC - Update last hiding spot
            }
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
            // Exclude the last hiding spot from consideration
            if (coverSpot == lastHidingSpot)
            {
                //Debug.Log($"FindFarthestCover(): Most recent hiding spot: {lastHidingSpot.name}");
                continue;
            }

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
            
            if (NavMesh.SamplePosition(potentialHidingSpot, out NavMeshHit hit, obstacleOffset, NavMesh.AllAreas))
            {
                bestHidingSpot = hit.position;
                lastHidingSpot = farthestHidingSpot; //ACC - Update last hiding spot
            }
            else
            {
                bestHidingSpot = Vector3.zero;
                Debug.LogError($"CoverState: NPC cannot be positioned behind Hiding Spot: '{farthestHidingSpot.name}'");
            }
            return bestHidingSpot;
        }
    }
}
