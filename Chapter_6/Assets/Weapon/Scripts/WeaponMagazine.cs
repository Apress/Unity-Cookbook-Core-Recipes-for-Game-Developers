using System.Collections;
using UnityEngine;

public class WeaponMagazine : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Max amount of Ammo the Weapons Magazine can have")]
    private int maxAmmoInMag = 25;

    [SerializeField]
    [Tooltip("How long it takes for the weapon to reload. While reloading Weapon can't be fired. (should match animation time)")]
    private float reloadTime = 1f;
    private int ammoCount;
    private bool isMagUsed = false;

    public bool IsReloading { get; private set; }

    private void Awake()
    {
        ammoCount = 0; // Initialize ammo count to Zero as Weapons start  of with no Magazines loaded.
    }

    public bool HasAmmoInMag() => ammoCount > 0;

    public void UseAmmo() => ammoCount--;

    public void Reload()
    {
        if (!IsReloading)
            StartCoroutine(ReloadAsync());
    }

    public void Unload()
    {
        IsReloading = false;
        ammoCount = 0;
        isMagUsed = true;
        Debug.Log("Magazine Detatched from Weapon");
    }
    private IEnumerator ReloadAsync()
    {
        if (!isMagUsed)
        {
            IsReloading = true;
            yield return new WaitForSeconds(reloadTime);
            ammoCount = maxAmmoInMag; //Refill ammo after waiting.
            IsReloading = false;
            Debug.Log("Magazine Loaded into Weapon");
        }
    }
}
