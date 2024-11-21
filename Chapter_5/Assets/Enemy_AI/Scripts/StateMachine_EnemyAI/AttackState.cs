using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class AttackState : MonoBehaviour, IState
{
    public event Action OnNpcAttack;//Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    [Tooltip("Speed at which NavMeshAgent moves while Attacking")]
    [SerializeField] private float npcSpeed = 0f;

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[AttackState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[AttackState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {        
        agent.speed = npcSpeed;
        agent.isStopped = true;
        OnNpcAttack?.Invoke(); //Being listened for in Actions script.

        //(this as IState).IsActive = true; //Attack not default starting State, so Not initialized in Awake()
        Debug.Log("AttackState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        stateMachine.RotateToFacePlayer();
        OnNpcAttack?.Invoke();//provides visual of NPC firing weapon.

        if (!stateMachine.IsPlayerAttackable())
        {
            if (stateMachine.IsPlayerVisible())
            {
                IState chase = stateMachine.states.Find(s => s.GetType() == typeof(ChaseState));
                stateMachine.SwitchState(chase);
            }
            else//Player not visible got to either Patrol or Wander State with 50% chance for either state.
            {
                if (Random.Range(0, 100) < 50)
                {
                    IState patrol = stateMachine.states.Find(s => s.GetType() == typeof(PatrolState));
                    stateMachine.SwitchState(patrol);
                    Debug.Log("AttackState: Switching to Patrol State");
                }
                else
                {
                    IState wander = stateMachine.states.Find(s => s.GetType() == typeof(WanderState));
                    stateMachine.SwitchState(wander);
                    Debug.Log("AttackState: Switching to Wander State");
                }
            }
        }

        Debug.Log("AttackState: Update");
    }

    void IState.OnStateExit()
    {
        //(this as IState).IsActive = false; //helpful in debugging

        Debug.Log("AttackState: Exit");
    }
}
