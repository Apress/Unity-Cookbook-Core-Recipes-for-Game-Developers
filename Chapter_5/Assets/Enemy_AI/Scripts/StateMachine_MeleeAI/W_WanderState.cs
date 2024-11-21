using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_WanderState : MonoBehaviour, IState
{

    [Tooltip("Area around NPC where it can Wander to")]
    [SerializeField] private float navigationRadius = 7.0f;

    private WarriorStateMachine stateMachine;
    private Vector3 target; //target is the random Wander location for NPC Agent
    private bool isTargetSet;

    private const float crossFadeTime = 0.1f;
    private readonly int walkHash = Animator.StringToHash("Walk");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[WanderState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            stateMachine.Agent.isStopped = false;
            isTargetSet = false; // Reset target selection on state enter
            SetNewWanderTarget(); // Set the initial wander target
        }
        else
            return;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Wander state
        meleeWeapon.GetComponent<Collider>().enabled = false;

        stateMachine.Anim.CrossFadeInFixedTime(walkHash, crossFadeTime);
        Debug.Log("WanderState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
      
        // Check if the agent has reached its wander target
        if (isTargetSet && stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance)
        {
            // Reset target selection
            isTargetSet = false;
            SetNewWanderTarget();
        }        
    }
    void IState.OnStateExit()
    {
        Debug.Log("WanderState: Exit");
    }

    private void SetNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * navigationRadius;
        randomDirection += stateMachine.Agent.transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, navigationRadius, NavMesh.AllAreas))
        {
            target = hit.position;
            stateMachine.Agent.SetDestination(target);
            isTargetSet = true; // Mark target as set
            Debug.Log("New wander target set: " + target);
        }
    }
}
