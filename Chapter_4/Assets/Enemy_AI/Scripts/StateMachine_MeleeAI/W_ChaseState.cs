using UnityEngine;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_ChaseState : MonoBehaviour, IState
{
    [Tooltip("Stopping Distance from Player at which NPC begins Circling Player after Chasing it down")]
    [SerializeField] private float stopDist = 4.75f;

    //Properties
    public float StopDist => stopDist;

    private WarriorStateMachine stateMachine;
    private IState previousState;
    private float playerDetectionSphereRadius;
    private float stopDistBuffer = 1.0f;

    private static readonly int ChaseHash = Animator.StringToHash("Run");
    private static readonly int IdleHash = Animator.StringToHash("Idle");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[ChaseState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }

        GameObject pds = GameObject.FindGameObjectWithTag("PlayerDetectionSphere");
        if (pds == null)
        {
            Debug.LogError($"[CirclingState - {gameObject.name}] : PlayerDetectionSphere Game Object not found.");
            return;
        }
        else
            playerDetectionSphereRadius = pds.GetComponent<SphereCollider>().radius;
    }

    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            stateMachine.Agent.ResetPath();
            stateMachine.Agent.stoppingDistance = stopDist; 
            stateMachine.Agent.isStopped = false;
        }
        else
            return;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Chase state
        meleeWeapon.GetComponent<Collider>().enabled = false;

        stateMachine.Anim.CrossFade(ChaseHash, 0.1f);

        Debug.Log("ChaseState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        
        stateMachine.RotateToFacePlayer();
        stateMachine.Agent.SetDestination(stateMachine.Player.position);//Could Use MoveTowardsPlayer() available in W_AttackState

          // Preserve previous state if not chase state
        if (stateMachine.PreviousState.GetType() != typeof(W_ChaseState))
        {
            previousState = stateMachine.PreviousState;
        }

        // If agent has reached stopping distance from player, have agent circle Player
        if (Vector3.Distance(stateMachine.Player.position, transform.position) <= StopDist + stopDistBuffer)
        {
            stateMachine.Anim.CrossFade(IdleHash, 0.1f);
            stateMachine.RotateToFacePlayer();
            Debug.Log($"[ChaseState - {gameObject.name}] :Agent has reached stopping distance from Player");

            IState circling = stateMachine.FindState<W_CirclingState>();
            if (circling != null)
            {
                Debug.Log($"[ChaseState - {gameObject.name}] : Switching to Circling State");
                Utils.Wait(1.0f, this, () => stateMachine.SwitchState(circling));
            }
        }
        /***
        else if (Vector3.Distance(stateMachine.Player.position, transform.position) > playerDetectionSphereRadius)
        {
            stateMachine.Anim.SetFloat("fwdSpeed", 0);

            //Player is outside the bounds of the NPC Collider, then switch to Idle State
            IState idle = stateMachine.FindState<W_IdleState>();
            if (idle != null)
            {
                Debug.Log($"[ChaseState - {gameObject.name}] : Player Escaped NPC Collider - Switching to Idle State");
                stateMachine.SwitchState(idle);
            }
        }
        ***/

    }
    void IState.OnStateExit()
    {
        Debug.Log("ChaseState: Exit");
        stateMachine.Agent.isStopped = false;
    }
}
