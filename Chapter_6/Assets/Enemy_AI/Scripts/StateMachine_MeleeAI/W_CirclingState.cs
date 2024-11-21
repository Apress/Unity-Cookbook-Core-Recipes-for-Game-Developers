using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_CirclingState : MonoBehaviour, IState
{
    [Tooltip("A range of seconds for which the NPC Circles around the Player")]
    [SerializeField] private Vector2 circlingTime = new Vector2(5.0f, 7.0f);
    [Tooltip("NPC's angular circling speed around Player")]
    [SerializeField] private float circlingSpeed = 30f;

    private WarriorStateMachine stateMachine;
    private float circlingTimer;
    private int circlingDir;//Circling direction  of NPC - Left represented by -1 & Right represented by +1
    private float circlingDist;//Distance from Player at which NPC circles around it.
    private float playerDetectionSphereRadius;
    private float stopDistBuffer = 1.0f;

    private static readonly int StrafeLeftHash = Animator.StringToHash("StrafeLeft");
    private static readonly int StrafeRightHash = Animator.StringToHash("StrafeRight");
    private static readonly int IdleHash = Animator.StringToHash("Idle");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[CirclingState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }

        GameObject pds = GameObject.FindGameObjectWithTag("PlayerDetectionSphere");
        if ( pds == null)
        {
            Debug.LogError($"[CirclingState - {gameObject.name}] : PlayerDetectionSphere Game Object not found.");
            return;
        }
        else
            playerDetectionSphereRadius = pds.GetComponent<SphereCollider>().radius;

        //Distance at which NPC circles around Player after having chased it down.
        circlingDist = GetComponent<W_ChaseState>().StopDist;
       
    }
    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            stateMachine.Agent.isStopped = false;
            stateMachine.Agent.ResetPath();
        }
        else
            return;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Idle state
        meleeWeapon.GetComponent<Collider>().enabled = false;

        circlingTimer = Random.Range(circlingTime.x, circlingTime.y);
        stateMachine.CirclingTime = circlingTimer;
        circlingDir = Random.Range(0, 2) == 0 ? -1 : 1;

        if (circlingDir == -1)
        {
            stateMachine.Anim.CrossFade(StrafeLeftHash, 0.1f);
        }
        else
        {
            stateMachine.Anim.CrossFade(StrafeRightHash, 0.1f);
        }

        Debug.Log("CirclingState: Enter");
    }
    void IState.OnStateUpdate(float deltaTime)
    {   
        if(stateMachine.IsPlayerDead)
        {
            Debug.Log("CirclingState: Player is Dead - Switching to Wandering state");
            IState wander = stateMachine.FindState<W_WanderState>();
            if (wander != null)
                stateMachine.SwitchState(wander);
        }

        Debug.Log($"[CirclingState - {gameObject.name}] : CirclingTimer: {circlingTimer} - Dist NPC To Player: {Vector3.Distance(stateMachine.Player.position, transform.position)} - Circling Dist: {circlingDist+ stopDistBuffer}");

        if (circlingTimer > 0 && Vector3.Distance(stateMachine.Player.position, transform.position) <= (circlingDist + stopDistBuffer)) 
        {
            Debug.Log($"[CirclingState - {gameObject.name}] : Circling Around Player");

            circlingTimer -= deltaTime;

            transform.RotateAround(stateMachine.Player.position, Vector3.up, circlingSpeed * circlingDir * deltaTime);
            stateMachine.RotateToFacePlayer();
        }       
        else if (circlingTimer <= 0 || Vector3.Distance(stateMachine.Player.position, transform.position) > (circlingDist + stopDistBuffer))
        {
            Debug.Log($"[CirclingState - {gameObject.name}] : Switch to Attacking State");

            if (NPCManager.Instance.GetAttackingNPC() == stateMachine)
            {
                IState attack = stateMachine.FindState<W_AttackState>();
                if (attack != null)
                {
                    Debug.Log($"[CirclingState - {gameObject.name}] : Switching to Attack State having completed Circling");
                    Utils.Wait(1.0f, this, () => stateMachine.SwitchState(attack));
                }
            }
            else
            {
                // Reset timers to start circling again
                circlingTimer = Random.Range(circlingTime.x, circlingTime.y);
            }
        } 
        
        if(Vector3.Distance(stateMachine.Player.position, transform.position) > (circlingDist + stopDistBuffer) &&
                Vector3.Distance(stateMachine.Player.position, transform.position) <= playerDetectionSphereRadius)
        {
            Debug.Log($"[CirclingState - {gameObject.name}] : Stop Dist : {circlingDist + stopDistBuffer} - Sphere Radius: {playerDetectionSphereRadius} - Enemy To Player Distance: {Vector3.Distance(stateMachine.Player.position, transform.position)}");

            //Can still Chase Player as its within NPC collider radius.
            IState chase = stateMachine.FindState<W_ChaseState>();
            if (chase != null)
            {
                Debug.Log($"[CirclingState - {gameObject.name}] : Chasing Escaping Player who is still within NPC Collider Radius - Switching to Chase State");
                Utils.Wait(1.0f, this, () => stateMachine.SwitchState(chase));
            }
        }        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            //If NPC stopping distance (radius) is not large enough to allow  it to  circle around the Obstacle then
            //when it collides with the Obstacle, set circlingTime to zero to stop circling and enter Attacking state. 
            //Ensure CapsuleCollider radius is at least 0.75
            Debug.Log($"CirclingState - {gameObject.name}] : Collided with Obstacle");
            circlingTimer = 0;
        }
    }

    void IState.OnStateExit()
    {        
        Debug.Log("CirclingState: Exit");
        stateMachine.Agent.ResetPath();
        stateMachine.Anim.CrossFade(IdleHash, 0.1f);
    }
}
