using System.Collections;
using UnityEngine;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_AttackState : MonoBehaviour, IState
{
    [Tooltip("A child object of the NPC that indicates its Eye Level")]
    [SerializeField] private GameObject npcEyes; //Object that indicates eye level of NPC
    [Tooltip("NPC can attack Player only when within this distance")]
    [SerializeField] private float attackDistance = 1f;//Distance from Player at which NPC throws a sword attack.
    private WarriorStateMachine stateMachine;
    private bool isAttacking;

    private readonly int attackHash = Animator.StringToHash("SwordSlash");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private readonly int idleHash = Animator.StringToHash("Idle");

    private const float crossFadeTime = 0.1f;

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[AttackState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }
    }
    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            stateMachine.Agent.isStopped = false;
        }
        else
            return;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Activate the Swords collider in Attack state
        meleeWeapon.GetComponent<Collider>().enabled = true;

        stateMachine.RotateToFacePlayer();

        //Let the NPCManager Singleton know an Attack is underway.
        NPCManager.Instance.SetAttackingNPC(stateMachine);

        stateMachine.Anim.CrossFadeInFixedTime(walkHash, crossFadeTime);//walk to attack Player.

        Debug.Log("AttackState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (isAttacking)
            return;
        
        MoveTowardsPlayer();
                
        if(Vector3.Distance(stateMachine.Player.position, transform.position) <= (attackDistance + 0.25f))
        {
            if (!IsObstacleInPath())
            {
                stateMachine.Anim.CrossFadeInFixedTime(attackHash, crossFadeTime);
                StartCoroutine(AttackPlayer());
            }
            else
            {
                //You can't attack a Player if he is behind an Obstacle
                NPCManager.Instance.ClearAttackingNPC(stateMachine);//Give other NPCs chance to Attack

                //Retreat back  from the Player.
                IState retreat = stateMachine.FindState<W_RetreatState>();
                if (retreat != null)
                {
                    Debug.Log($"[AttackState - {gameObject.name}] : Switching to Retreat State having completed an Attack on Player");
                    Utils.Wait(1.0f, this, () => stateMachine.SwitchState(retreat));
                }
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (stateMachine == null) return;

        Vector3 directionToPlayer = (stateMachine.Player.position - transform.position).normalized;
        directionToPlayer.y = 0; //Ensure movement is only on the horizontal plane
        float distanceToPlayer = Vector3.Distance(stateMachine.Player.position, transform.position);

        if (distanceToPlayer > attackDistance)
        {
            // Calculate the desired movement vector
            Vector3 moveVector = directionToPlayer * stateMachine.Agent.speed * Time.deltaTime;
            stateMachine.Agent.Move(moveVector);

            // Rotate to face the player
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation,stateMachine.Agent.angularSpeed * Time.deltaTime);
        }
        else
        {
            // Stop movement if within stopping distance
            stateMachine.Agent.Move(Vector3.zero);

            // Rotate to face the player
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, stateMachine.Agent.angularSpeed * Time.deltaTime);
        }
    }

    void IState.OnStateExit()
    {
        Debug.Log("AttackState: Exit");
        
        // Clear the attacking NPC if this NPC was attacking
        if (NPCManager.Instance.IsAnyNPCAttacking() && NPCManager.Instance.GetAttackingNPC() == stateMachine)
        {
            NPCManager.Instance.ClearAttackingNPC(stateMachine);
        }
        stateMachine.Agent.ResetPath();
        isAttacking = false;
    }
    private bool IsObstacleInPath()
    {
        int layerMask = LayerMask.GetMask("Obstacle");
        // Use a simple Raycast to detect obstacles
        RaycastHit hit;
        if(Physics.Raycast(npcEyes.transform.position,transform.forward,out hit,attackDistance,layerMask)) 
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log($"[CirclingState - {gameObject.name}] : Obstacle Encountered : {hit.collider.gameObject.name}");
                return true;
            }
        }
        return false;
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        // Wait until the 'RightHandAttack' animation is nearly completed
        yield return new WaitUntil(() => IsAnimationNearlyComplete("SwordSlash", 0.99f));
        stateMachine.Anim.CrossFadeInFixedTime(idleHash, crossFadeTime);//stop walking.
        
        //Retreat back  from the Player.
        IState retreat = stateMachine.FindState<W_RetreatState>();
        if (retreat != null)
        {
            Debug.Log($"[AttackState - {gameObject.name}] : Switching to Retreat State having completed an Attack on Player");
            Utils.Wait(1.0f, this, () => stateMachine.SwitchState(retreat));
        }
    }

    private bool IsAnimationNearlyComplete(string stateName, float threshold)
    {
        AnimatorStateInfo stateInfo = stateMachine.Anim.GetCurrentAnimatorStateInfo(1); // Using layer 1 
        return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= threshold;
    }
}
