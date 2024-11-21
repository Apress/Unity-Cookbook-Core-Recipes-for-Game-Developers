using UnityEngine;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_RetreatState : MonoBehaviour, IState
{
    //Distance to which NPC should retreat to after an Attack on Player 
    private float retreatDistance;
    private WarriorStateMachine stateMachine;

    private static readonly int WalkBackwardHash = Animator.StringToHash("WalkBackward");
    private static readonly int IdleHash = Animator.StringToHash("Idle");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[RetreatState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }      

        //After an Attack on Player NPC to retreat only to its circling Distance.
        retreatDistance = GetComponent<W_ChaseState>().StopDist;
    }

    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            //Vector3 retreatDirection = (transform.position - stateMachine.Player.position).normalized;
            stateMachine.Agent.isStopped = false;
        }
        else
            return;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Retreat state
        meleeWeapon.GetComponent<Collider>().enabled = false;

        stateMachine.Anim.CrossFade(WalkBackwardHash, 0.1f);

        Debug.Log("RetreatState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        RetreatFromPlayer(retreatDistance, deltaTime);

        // Check if the NPC has reached its destination to transition back to circling
        if (stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance && !stateMachine.Agent.pathPending)
        {
            Debug.Log($"[RetreatState: {gameObject.name}] Retreated back to its original Circling Distance");

            
            IState circling = stateMachine.FindState<W_CirclingState>();
            if (circling != null)
            {
                Debug.Log($"[RetreatState - {gameObject.name}] : Switching to Circling State having completed Retreat");
                Utils.Wait(1.0f, this, () => stateMachine.SwitchState(circling));
            }
            
        }
    }

    private void RetreatFromPlayer(float retreatDistance, float deltaTime)
    {
        // Calculate the direction from the NPC to the player
        Vector3 directionToPlayer = (stateMachine.Player.position - transform.position).normalized;
        directionToPlayer.y = 0; //Ensure movement is only on the horizontal plane
        float distanceFromPlayer = Vector3.Distance(stateMachine.Player.position, transform.position);

        if (distanceFromPlayer < retreatDistance) //you could provide a buffer here.
        {
            // Calculate the retreat direction and the movement vector
            Vector3 retreatDirection = -directionToPlayer;
            Vector3 moveVector = retreatDirection * stateMachine.Agent.speed * deltaTime;

            // Move the agent
            stateMachine.Agent.Move(moveVector);

            // Rotate to face the player
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, stateMachine.Agent.angularSpeed * deltaTime);

        }
        else
        {
            // Stop movement if the NPC has reached the retreat distance
            stateMachine.Agent.Move(Vector3.zero);

            // Rotate to face the player
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, stateMachine.Agent.angularSpeed * deltaTime);
        }
    }

    void IState.OnStateExit()
    {        
        Debug.Log("RetreatState: Exit");
        stateMachine.Anim.CrossFade(IdleHash, 0.1f);
    }
}
