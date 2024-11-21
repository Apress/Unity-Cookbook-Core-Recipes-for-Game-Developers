using UnityEngine;
using UnityEngine.InputSystem;
using System;

//Attach this Health script to your NPCs-Soldier game object and the PlayerArmature game object.

public class Health : MonoBehaviour, IHealth
{
    public event Action<string> OnHealthDepleted;//C# Event invoked from TakeHealth() and listened for in NPCStateMachine script.
    public event Action OnPlayerDeath;// C# Event invoked from Die() and listened for in WarriorStateMachine script.
    public event Action OnNpcDeath;//Being listened for in WarriorStateMachine script.

    [SerializeField] private int maxHealth = 100;

    public int currentHealth; //set variable back to private after testing
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
                OnHealthDepleted?.Invoke(gameObject.tag); //Event being listened for in WarriorStateMachine script
        }        
    }
    public void Die()
    {
        if (isDead)
            return;
        
        isDead = true;

        if (!gameObject.CompareTag("Player"))
        {
                OnNpcDeath?.Invoke();//Being listened for in WarriorStateMachine script
        }
        else
        {
            // Handle player death logic here - Show Death screen etc.
            // Ensure NPC goes back to Wandering. 
            //Event being listened for in WarriorStateMachine. Ensures that the NPCs stop attacking and continue Wandering
            OnPlayerDeath?.Invoke(); 
        }
    }
    
    private void Update() //For Testing purposes only.
    {
        Mouse myMouse = Mouse.current ;

        // Example of triggering firing of weapon, e.g., when the player fires the weapon by clicking the left Mouse Button
        if (myMouse != null)
        {
            if (myMouse.leftButton.wasPressedThisFrame)
            {
                if (gameObject.CompareTag("NPC"))
                    TakeHealth(10); //Simulates Player shooting NPC demostrating the Damage and Death animations on NPC.
            }
        }
    }
    
}
