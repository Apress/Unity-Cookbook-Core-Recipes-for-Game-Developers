using UnityEngine;

public class MeleeWeaponDamage : MonoBehaviour
{
    [Tooltip("The amount of damage to deal to the Hit Object")]
    [SerializeField] private int damageAmount = 10;

    private Collider swordCollider;
    private bool hasDealtDamage;
    private string topMostParentTag;

    private void Awake()
    {
        swordCollider = gameObject.GetComponent<Collider>();
        if (swordCollider == null)
        {
            Debug.LogError($"[MeleeWeaponDamage - {gameObject.name}] : No Collider found on the sword object.");
            return;
        }
        else
        {
            topMostParentTag = swordCollider.GetComponentInParent<Transform>().root.tag;
            DisableCollider(); // Ensure collider is initially disabled
        }
    }

    // Function to enable the collider at the start of a new attack
    public void EnableCollider()
    {
        swordCollider.enabled = true; // Enable sword collider
        hasDealtDamage = false; // Reset damage flag
    }

    // Function to disable the collider at the end of the attack
    public void DisableCollider()
    {
        swordCollider.enabled = false; // Disable sword collider
    }

    // Function to handle collision detection when the collider is enabled
    private void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage)
            return;

        // Ensure that the warrior/player wielding the sword has not hit itself and one warrior has not hit another warrior

        //Player has hit itself.
        if (topMostParentTag == "Player" && other.CompareTag("Player")) 
            return; 
        //NPC has hit itself or another NPC
        else if (topMostParentTag == "NPC" && other.CompareTag("NPC")) 
            return;

        if (other.TryGetComponent<Health>(out Health health))
        {
            health.TakeHealth(damageAmount);
            Debug.Log($"Melee Weapon: ({gameObject.name}) interacted with: {other.name} - Its Health is now: {health.CurrentHealth}");
            hasDealtDamage = true; //Set damage flag
        }
        else
        {
            Debug.LogWarning($"[MeleeWeaponDamage - {gameObject.name}] : No Health component found.");
        }
    }

    private void OnDisable()
    {
        DisableCollider(); // Reset collider state
        hasDealtDamage = false; // Reset damage flag
    }
}


