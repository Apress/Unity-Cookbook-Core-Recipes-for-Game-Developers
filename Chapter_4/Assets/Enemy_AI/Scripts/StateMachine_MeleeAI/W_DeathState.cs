using UnityEngine;

//States have access to MonoBheaviour methods too.
//All States must be added as components to the NPC game object.
public class W_DeathState : MonoBehaviour, IState
{
    private WarriorStateMachine stateMachine;
    private float destroyDelay = 3f;
    private bool isNpcDead;

    private const float crossFadeTime = 0.1f;
    private readonly int deathHash = Animator.StringToHash("Death");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if(stateMachine == null)
        {
            Debug.LogError($"[DeathState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }

        Debug.Log("In W_DeathState");
    }

    void IState.OnStateEnter()
    {
        if (!stateMachine.Agent.enabled)
            return;

        stateMachine.Agent.isStopped = true;
        stateMachine.Agent.ResetPath();
        stateMachine.Agent.enabled = false;
        isNpcDead = true;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Death state.
        meleeWeapon.GetComponent<Collider>().enabled = false;

        stateMachine.Anim.CrossFadeInFixedTime(deathHash, crossFadeTime);

        Debug.Log("DeathState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (isNpcDead)
        {            
            IState idle = stateMachine.FindState<W_IdleState>();
            if (idle != null)
            {
                Debug.Log("DeathState: Switching to Idle State");
                stateMachine.SwitchState(idle);
            }
        }        
    }
        
    void IState.OnStateExit()
    {
        Debug.Log("DeathState: Exit");
        Destroy(gameObject, destroyDelay);
    }  
}
