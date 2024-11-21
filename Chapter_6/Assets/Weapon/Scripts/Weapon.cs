using System;
using UnityEngine;

[RequireComponent(typeof(WeaponAudio))]
public class Weapon : MonoBehaviour
{
    public event Action OnWeaponFired, OnWeaponDryFired, OnMagazineAttached, OnMagazineDetatched;

    [SerializeField] private float shootInterval = 2.0f; //Time between shots. If set to Zero weapon can rapid fire.

    //Magazine will be auto reloaded if it's a NPC weapon, the moment its ammo is over.
    //For an NPC weapon set autoReload to true.
    //For Palyer weapons autoReload should be false as the player needs to perform a reload action to load a magazine into the weapon.
    [SerializeField] private bool autoReload;

    private WeaponMagazine magAmmo;
    private float lastShotTime;

    private void Start()
    {
        //Registering  of the OnNpcAttack event needs  to be done in the Start() method here as the NPC Weapon is not preassigned to 
        //the NPC but is assigned to an NPC at run time, so this event registration cannot be done within OnEnable(). 
        //Ensure  that the weapon is a NPC's weapon i.e. tagged as 'NPC_Weapon'as the below event registration and magAmmo reference
        //population should not occur for a Player Weapons.
        if (gameObject.CompareTag("NPC_Weapon"))
        {
            GetComponentInParent<AttackState>().OnNpcAttack += TryFireWeapon;
            magAmmo = GetComponent<WeaponMagazine>();//For all Weapons the Magazine script is a component on the Weapon object.
        }
    }

    private void OnDestroy()
    {
        AttackState attackState = GetComponentInParent<AttackState>();
        if (attackState != null)
            attackState.OnNpcAttack -= TryFireWeapon;
    }

    private void FireWeapon()
    {
        magAmmo.UseAmmo(); //Reduce the ammo in the Magazine.
        lastShotTime = Time.time;
        //Notify subscribers 'WeaponRaycast','WeaponAudio, WeaponCasing,etc that weapon was fired.
        OnWeaponFired?.Invoke();
        Debug.Log("NPC Weapon Fired at Player");
    }

    private void TryFireWeapon()
    {
        if (magAmmo == null || magAmmo.IsReloading)
            return;

        if (magAmmo.HasAmmoInMag())
        {
            if (Time.time - lastShotTime >= shootInterval)
                FireWeapon();
        }
        else if (autoReload)
        {
            MagazineAttached();
        }
        else
        {
            OnWeaponDryFired?.Invoke();
            Debug.Log("NPC / Player Weapon Magazine is Empty - Need to reload Mag");
        }
    }

    //Used with the Players weapons only as only the Players weapon magazines can be physically attached 
    public void MagazineAttached()
    {
        if (magAmmo == null)
        {
            Debug.LogError("magAmmo was Null");
            return;
        }
        OnMagazineAttached?.Invoke(); //Notify subscribers 'WeaponAudio', script  that a mag was attached to weapon 
        magAmmo.Reload();
        Debug.Log("Weapon is Reloading");
    }

    //Used with the Players weapons only as only the Players weapon magazines can be physically detatched.
    public void MagazineDetatched()
    {
        OnMagazineDetatched?.Invoke(); //Notify subscribers 'WeaponAudio', script that a mag was detatched from weapon 
        magAmmo.Unload(); //Ensures the Magazine cannot be reused.
        magAmmo = null;
    }

}
