using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class WarriorStateMachine : StateMachine
{
    //Events
    public static event Action<Vector3> OnPlayerSpotted;

    [Tooltip("Warriors Field Of View for initiating chase")]
    [SerializeField] private float visibleChaseAngle = 180f;

    public List<IState> states = new List<IState>();
    public Transform Player { get; private set; }//Reference to the Player transform
    public NavMeshAgent Agent { get; private set; } // Reference to the warrior Agent 
    public Animator Anim { get; private set; } //Reference to the Animator
    public float CirclingTime { get; set; } //Circling time of each NPC - used within NPCManager
    public bool HasSpottedPlayer { get; set; } //Determines if Player has been spotted by NPC i.e. Player entered into NPCs trigger zone.
    public bool IsPlayerDead { get; set; } //Keeps track of whether Player is dead or alive.

    private IState startingState;
    private float rotSpeed = 2f;

    void OnEnable()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnNpcDeath += NpcDead;
            health.OnHealthDepleted += TakeDamage;
        }
    }
    void OnDisable()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnNpcDeath -= NpcDead;
            health.OnHealthDepleted -= TakeDamage;
        }
        
        if(Player != null)
            Player.GetComponent<Health>().OnPlayerDeath -= PlayerDead;

    }
    private void Awake()
    {
        states = GetComponents<IState>().ToList();
        if (states == null || states.Count == 0)
        {
            Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : No NPC States found.");
            return;
        }

        Player = GameObject.FindWithTag("Player").transform;
        if (Player == null)
        {
            Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : Player not found.");
            return;
        }
        else
            Player.GetComponent<Health>().OnPlayerDeath += PlayerDead;


        Anim = GetComponent<Animator>();//ACC
        if(Anim == null)
        {
            Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : Animator not found.");
            return;
        }

        Agent = GetComponent<NavMeshAgent>();
        if (Agent == null)
        {
            Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : No NavMesh Agent found.");
            return;
        }
    }

    void Start()
    {
        //Selects Warriow Idle state as default starting state 
        startingState = states.Find(s => s.GetType() == typeof(W_IdleState));
        if (!IsPlayerDead)
            SwitchState(startingState);
    }
    
    /*** Commonly used Methods that can be accessed from any State ***/
    public void InitiateAttackOnPlayer(WarriorStateMachine npc)
    {
        Debug.Log($"[WarriorStateMachine - {gameObject.name}] : Initiating NPC - {npc.name} to chase down Player");
        IState chase = FindState<W_ChaseState>();
        if (chase != null)
        {
           SwitchState(chase);
        }
    }
    
    public bool IsPlayerVisible()
    {
        Vector3 npcToPlayerDir = Player.transform.position - transform.position;
        float angle = Vector3.Angle(npcToPlayerDir, transform.forward);

        if (angle < visibleChaseAngle/2)
            return true;
        else
            return false;
    }

    //Alert nearby NPCs when an NPC detects the Player
    public void AlertNearbyNPCs()//Invoked from PlayerDetector class
    {
        //Event being listened for within the NPCManager class
        OnPlayerSpotted?.Invoke(transform.position);
    }

    //Have NPC Rotate to face player.
    public void RotateToFacePlayer()
    {
        Vector3 npcToPlayerDir = Player.transform.position - this.transform.position;
        npcToPlayerDir.y = 0;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,Quaternion.LookRotation(npcToPlayerDir),Time.deltaTime * rotSpeed);
    }

    private void NpcDead()
    {
        IState death = FindState<W_DeathState>();
        if (death != null)
            SwitchState(death);
        else
        {
            Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : W_DeathState component not attached to game object.");
            return;
        }
    }

    private void PlayerDead()
    {
        IsPlayerDead = true;//Being polled in Warriors CirclingState.
        Debug.Log("WarriorStateMachine: Player is Dead");      
    }
    private void TakeDamage(string objTag)
    {
        if (objTag == "NPC")
        {
            Health health = GetComponent<Health>();
            if (health != null && health.CurrentHealth < 50)
            {
                Debug.Log("NPC Health < 50% - Taking Cover");
                IState takeCover = FindState<W_CoverState>();
                if (takeCover != null)
                {
                    SwitchState(takeCover);
                    return;
                }
                else
                {
                    Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : W_CoverState component not attached to game object.");
                    return;
                }
            }

            Debug.Log("NPC Has been Hit");
            IState hit = FindState<W_HitState>();
            if (hit != null)
                SwitchState(hit);
            else
            {
                Debug.LogError($"[WarriorStateMachine - {gameObject.name}] : W_HitState component not attached to game object.");
                return;
            }
        }
        else if (objTag == "Player")
        {
            // Deal with Player Health damage response.
        }
    }

    //A way to refactor logic for finding states into a helper method
    //Made public to be invokable from different States.
    public IState FindState<T>() where T : IState
    {
        return states.Find(s => s.GetType() == typeof(T));
    }

    public IState FindState(IState stateInstance)
    {
        return states.Find(s => s.GetType() == stateInstance.GetType());
    }
}
