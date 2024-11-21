using UnityEngine;
using System;

/***
Attach this Health script to your NPCs or player character.
Ensure the ragdollPrefab has the Destroyable script attached.
Set the destroyDelay in the Health script to define how long the ragdoll and NPC should remain before being destroyed.
***/

public class Health : MonoBehaviour, IHealth
{
    public event Action OnHealthDepleted;    // C# Event invoked from TakeHealth() and listened for in Actions script.
    public event Action OnDead;             // C# Event invoked from Die() and listened for in Actions script.
    public event Action OnPlayerDeath;     // C# Event invoked from Die() and listened for in BehaviorFSM script.

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject ragdollPrefab;
    //[SerializeField] private Animator animator;
    [SerializeField] private bool useRagdollEffect = false;
    [SerializeField] private float destroyDelay = 5.0f;     //Time after which the ragdoll or NPC is destroyed

    public int currentHealth;
    private bool isDead = false;

    public int CurrentHealth { get { return currentHealth; } private set { currentHealth = value; } }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeHealth(int amount)
    {
        if (!isDead)
        {
            currentHealth -= amount;
            Debug.Log($"Health is now : {currentHealth}");

            if (currentHealth <= 0)
                Die();
            else
                OnHealthDepleted?.Invoke(); //Event being listened for in Actions script
        }        
    }

    public void Die()
    {
        if (isDead)
            return;
        
        isDead = true;

        if (!gameObject.CompareTag("Player"))
        {
            if (useRagdollEffect && ragdollPrefab != null)
            {
                GameObject ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
                TimedDestroyable destroyable = ragdoll.GetComponent<TimedDestroyable>();
                if (destroyable != null)
                    destroyable.DestroyAfterDelay(destroyDelay);

                Destroy(gameObject); // Destroy the NPC object this script is attached to immediately
            }
            //else if (animator != null)
            else
            {
                //animator.SetTrigger("Die");

                OnDead?.Invoke();   //Event being listened for in Actions and Death scripts

                Destroy(gameObject, destroyDelay); // Destroy the NPC object this script is attached to after a predefined time ideally after sometime of the animation having played.
            }
        }
        else
        {
            // Handle player death logic here - Show Death screen etc.
            // Ensure NPC goes back to Patrolling. 
            Debug.Log("Player is Dead");
            OnPlayerDeath?.Invoke(); //Event being listened for in BehaviorFSM. Ensures that the NPC that finally killed player stops attacking and continues patrolling
        }
    }
    /***
    private void Update()
    {
        // Example triggering firing of weapon, e.g., when the player fires the weapon by clicking the left Mouse Button
        if (Input.GetButtonDown("Fire1"))
        {
            if (gameObject.CompareTag("NPC"))
                TakeHealth(10); //Demonstrates the Damage and Death animations when Player shoots at NPC.
        }
    }
    ***/
}
