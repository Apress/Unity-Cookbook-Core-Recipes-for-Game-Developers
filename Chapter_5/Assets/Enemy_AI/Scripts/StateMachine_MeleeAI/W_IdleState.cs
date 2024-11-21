using UnityEngine;
using Random = UnityEngine.Random;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_IdleState : MonoBehaviour, IState
{
    [Tooltip("A range of seconds for which the NPC remains Idle")]
    [SerializeField] private Vector2 idlingTime = new Vector2(4f,6f);

    private WarriorStateMachine stateMachine;
    private float idleTimer = 0;

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[IdleState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }        
    }

    void IState.OnStateEnter()
    {
        if (stateMachine.Agent.enabled)
        {
            stateMachine.Agent.isStopped = true;
            stateMachine.Agent.ResetPath();
        }
        else
            return;

        idleTimer = Random.Range(idlingTime.x, idlingTime.y); 

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Idle state
        meleeWeapon.GetComponent<Collider>().enabled = false;

        Debug.Log("IdleState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {        
        stateMachine.RotateToFacePlayer();

        idleTimer -= deltaTime;

        if (idleTimer <= 0)
        {
            Debug.Log("IdleState: Switching to Wandering");
            RandomWander(); 
        }
    }

    void IState.OnStateExit()
    {        
        Debug.Log("IdleState: Exit");

    }

    private void RandomWander()
    {
        IState wander = stateMachine.FindState<W_WanderState>();
        if (wander != null)
        {
            Debug.Log("IdleState: Switching to Wander State");
            stateMachine.SwitchState(wander);
        }
    }
}
