using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCStateMachine : StateMachine
{
    [Tooltip("NPC's distance from Player to begin Chase.")]
    [SerializeField] private float visibleChaseDistance = 15f;
    [Tooltip("NPC's visibility angle for initiating chase")]
    [SerializeField] private float visibleChaseAngle = 70f;
    [Tooltip("NPC's visibility angle for initiating chase")]
    [SerializeField] private float attackDistance = 3f;
    [Tooltip("NPC's stopping distance from Player whilst Chasing it")]

    public List<IState> states = new List<IState>();
    private IState startingState;
    private Transform player;//Reference to the Player transform
    private float rotSpeed = 1.5f;
    private bool  isPlayerDead;

    //Properties    
    public Transform Player => player;

    void OnEnable()
    {
        GetComponent<Health>().OnNpcDeath += NpcDead;
        GetComponent<Health>().OnHealthDepleted += TakeDamage;

    }

    void OnDisable()
    {
        GetComponent<Health>().OnNpcDeath -= NpcDead;
        GetComponent<Health>().OnHealthDepleted -= TakeDamage;

    }

    private void Awake()
    {
        states = GetComponents<IState>().ToList();
        if (states.Count == 0)
        {
            Debug.LogError($"[NPCStateMachine - {gameObject.name}] : No NPC States found.");
            return;
        }

        player = GameObject.FindWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError($"[NPCStateMachine - {gameObject.name}] : Player not found.");
            return;
        }
    }

    void Start()
    {
        //Selects Idle state as default starting state 
        startingState = states.Find(s => s.GetType() == typeof(IdleState));
        if (!isPlayerDead)
            SwitchState(startingState);
    }

    /*** Commonly used Methods that can be accessed from any State ***/
    public bool IsPlayerVisible()
    {
        Vector3 npcToPlayerDir = player.transform.position - this.transform.position;
        float angle = Vector3.Angle(npcToPlayerDir, this.transform.forward);

        if(npcToPlayerDir.magnitude < visibleChaseDistance && angle < visibleChaseAngle)
            return true;
        else
            return false;
    }

    public bool IsPlayerAttackable()
    {
        Vector3 npcToPlayerDir = player.transform.position - this.transform.position;

        if (npcToPlayerDir.magnitude < attackDistance)
            return true;
        else
            return false;
    }

    //Have NPC Rotate to face player.
    public void RotateToFacePlayer()
    {
        Vector3 npcToPlayerDir = player.transform.position - this.transform.position;
        npcToPlayerDir.y = 0;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,Quaternion.LookRotation(npcToPlayerDir),Time.deltaTime * rotSpeed);
    }

    private void NpcDead()
    {
        IState death = states.Find(s => s.GetType() == typeof(DeathState));
        SwitchState(death);
    }

    private void TakeDamage(string objTag)
    {
        if (objTag == "NPC")
        {
            if ((GetComponent<Health>().CurrentHealth < 50))
            {
                Debug.Log("NPC Health < 50% - Taking Cover");
                IState takeCover = states.Find(s => s.GetType() == typeof(CoverState));
                SwitchState(takeCover);
                return;
            }

            Debug.Log("NPC Has been Hit");
            IState hit = states.Find(s => s.GetType() == typeof(HitState));
            SwitchState(hit);
        }
        else if (objTag == "Player") 
        {
            //Deal with Player Health damage response.
        }
    }
}
