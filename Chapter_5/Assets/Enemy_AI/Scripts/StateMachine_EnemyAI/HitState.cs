using UnityEngine;
using UnityEngine.AI;
using System;

/***
Whilst in any State (i.e. Patrol, Idle, Attack, Chase) NPC can be Hit (i.e. shot at)
and will need to enter the HitState, playing a Hit animation.
*****
States have access to MonoBheaviour methods too.
All States must be added as components to the NPC game object.
***/
public class HitState : MonoBehaviour, IState
{
    public event Action PlayNpcHitAnim;//Being listened for in Actions script.

    //private bool isActive;
    //bool IState.IsActive { get => isActive; set => isActive = value;}

    private NPCStateMachine stateMachine;
    private NavMeshAgent agent; // Reference to the soldier NPC Agent
    private bool isNpcHit;
    private IState previousState;

    void Awake()
    {
        stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[HitState - {gameObject.name}] : NPCStateMachine not found.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[HitState - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void IState.OnStateEnter()
    {        
        agent.speed = 0f;
        agent.isStopped = true;
        isNpcHit = true;
        PlayNpcHitAnim?.Invoke();//Plays NPC Hit Animation.
        //(this as IState).IsActive = true; //Chase not default starting State, so Not initialized in Awake()
        Debug.Log("HitState: Enter");
    }

    void IState.OnStateUpdate(float deltaTime)
    {
        if (isNpcHit)//switch to the immediate previous state before the HitState
        {
            //Check if the previous state is not of type HitState as NPC could be hit several times in succession. 
            if (stateMachine.PreviousState.GetType() != typeof(HitState))
            {
                previousState = stateMachine.PreviousState;
            }

            IState prevState = stateMachine.states.Find(s => s.GetType() == previousState.GetType());
            stateMachine.SwitchState(prevState);
        }

        Debug.Log("HitState: Update");
    }
        
    void IState.OnStateExit()
    {
        //(this as IState).IsActive = false; //helpful in debugging

        Debug.Log("HitState: Exit");
        isNpcHit = false;
    }
}
