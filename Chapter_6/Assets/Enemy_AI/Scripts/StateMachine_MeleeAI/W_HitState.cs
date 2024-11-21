using UnityEngine;
/***
Whilst in any State (i.e. Idle, Attack,Wander, Chase) an NPC can be Hit 
and will need to enter the HitState, playing a Hit animation.
*****
States have access to MonoBheaviour methods too.
All States must be added as components to the NPC game object.
***/
public class W_HitState : MonoBehaviour, IState
{
    private WarriorStateMachine stateMachine;
    private IState previousState;
    private static readonly int HitHash = Animator.StringToHash("HitImpact");
    private static readonly int IdleHash = Animator.StringToHash("Idle");

    void Awake()
    {
        stateMachine = GetComponent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[HitState - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }       
    }

    void IState.OnStateEnter()
    {
        if (!stateMachine.Agent.enabled)
            return;

        GameObject meleeWeapon = GameObject.FindGameObjectWithTag("MeleeWeapon");
        //Deactivate the Swords collider in Hit state
        meleeWeapon.GetComponent<Collider>().enabled = false;

        stateMachine.Anim.CrossFade(HitHash, 0.1f);

        Debug.Log("HitState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
            //switch to the immediate previous state before the HitState, which would most probably be Attack or Retreat 
            //as NPC needs to be close to the player to get hit, however you can set previousState to Idle if you just want
            //to hit him and see the HitImpact animation play.

            //Check if the previous state is not of type HitState as NPC could be hit several times in succession. 
            if (stateMachine.PreviousState.GetType() != typeof(W_HitState))
            {
                previousState = stateMachine.PreviousState;
            }

            IState prevState = stateMachine.FindState(previousState);
            if (prevState != null)
            {
                Debug.Log($"HitState: Switching to immediate Previous State: {prevState.GetType()}. Pausing briefly to see Hit Animation Play");

                Utils.Wait(1f, this, () => stateMachine.SwitchState(prevState));
            }

        Debug.Log("HitState: Update");
    }

    void IState.OnStateExit()
    {
        Debug.Log("HitState: Exit");
        stateMachine.Anim.CrossFade(IdleHash, 0.1f);
    }
}
