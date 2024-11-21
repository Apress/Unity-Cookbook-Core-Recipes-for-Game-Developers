using UnityEngine;

public class WarriorAnimationEvents : MonoBehaviour
{
    private MeleeWeaponDamage swordDamage;

    private void Awake()
    {
        // Assuming the sword is a direct child of the warrior
        swordDamage = GetComponentInChildren<MeleeWeaponDamage>();
        if (swordDamage == null)
        {
            Debug.LogError("MeleeWeaponDamage component not found on sword child object.");
        }
    }

    // Proxy method to enable the sword's collider
    public void EnableSwordCollider()
    {
        if (swordDamage != null)
        {
            swordDamage.EnableCollider();
        }
    }

    // Proxy method to disable the sword's collider
    public void DisableSwordCollider()
    {
        if (swordDamage != null)
        {
            swordDamage.DisableCollider();
        }
    }
}

