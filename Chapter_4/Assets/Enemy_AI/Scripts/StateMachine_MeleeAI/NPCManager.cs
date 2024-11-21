using System.Collections.Generic;
using UnityEngine;
//Attached to the NPCManager game object in the hierarhcy that contains Patrol Points
public class NPCManager : MonoBehaviour
{
    [Tooltip("Range within which NPCs will be alerted")]
    [SerializeField] private float alertRange = 15f;

    [Tooltip("Time in seconds that needs to elapse  before NPC attacks Player")]
    [SerializeField] private Vector2 attackTimeRange = new Vector2(2.0f, 4.0f);

    //Represents NPCs for whom the Player has entered within the bounds of their trigger collider.
    public List<WarriorStateMachine> npcsInRange = new List<WarriorStateMachine>();
    
    //Represents all the NPCs avaialable in the current level
    public List<WarriorStateMachine> npcsInLevel = new List<WarriorStateMachine>();
    private static NPCManager instance;
    private WarriorStateMachine currentAttackingNPC;
    private float attackTimer = 2.0f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        //Populate the npcsInLevel list with all NPCs available within the level.
        PopulateNpcsInLevel();
    }
    private void OnEnable()
    {
        WarriorStateMachine.OnPlayerSpotted += HandlePlayerSpotted;
    }
    private void OnDisable()
    {
        WarriorStateMachine.OnPlayerSpotted -= HandlePlayerSpotted;
    }

    public static NPCManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("NPCManager");
                instance = go.AddComponent<NPCManager>();
            }
            return instance;
        }
    }

    private void Update()
    {
        if (npcsInRange.Count > 0)
        {
            if (!IsAnyNPCAttacking())
            {
                if (attackTimer > 0)//wait attackTimer seconds before having an NPC attack  Player.
                {
                    attackTimer -= Time.deltaTime;
                }
                else //select an NPC to attack player 
                {
                    //Randomly select NPC to perform an Attack. 
                    currentAttackingNPC = npcsInRange[Random.Range(0, npcsInRange.Count)];

                    attackTimer = Random.Range(attackTimeRange.x, attackTimeRange.y);
                    if(currentAttackingNPC != null)
                        Debug.Log($"[NPCManager] {currentAttackingNPC.name} -  has been selected to attack");
                }
            }
        }
        else
            return;
    }

    private void HandlePlayerSpotted(Vector3 alertPosition)
    {
        foreach (WarriorStateMachine npc in npcsInLevel)
        {
            if (Vector3.Distance(npc.transform.position, alertPosition) <= alertRange && !npc.HasSpottedPlayer)
            {
                npc.InitiateAttackOnPlayer(npc); //Method to get NPC to chase and attack player
            }
        }
    }
    private void PopulateNpcsInLevel()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject npc in npcs)
        {
            WarriorStateMachine warrior = npc.GetComponent<WarriorStateMachine>();
            if (warrior != null)
            {
                npcsInLevel.Add(warrior);
            }
        }
    }


    //Method below invoked from PlayerDetector class.
    public void RegisterInRangeNpc(WarriorStateMachine npc)
    {
        if(!npcsInRange.Contains(npc))
        {
            npcsInRange.Add(npc);
            npc.HasSpottedPlayer = true;
        }
    }

    //Method below invoked from PlayerDetector class.
    public void UnregisterOutOfRangeNpc(WarriorStateMachine npc)
    {
        npcsInRange.Remove(npc);
        npc.HasSpottedPlayer = false;
    }

    public bool IsAnyNPCAttacking()
   {
       return currentAttackingNPC != null;
   }

   public void SetAttackingNPC(WarriorStateMachine npc)
   {
       currentAttackingNPC = npc;
   }
  
   public WarriorStateMachine GetAttackingNPC()
   {
       return currentAttackingNPC;
   }

    public void ClearAttackingNPC(WarriorStateMachine npc)
    {
        if (currentAttackingNPC == npc)
        {
            currentAttackingNPC = null;
        }
    }

}

