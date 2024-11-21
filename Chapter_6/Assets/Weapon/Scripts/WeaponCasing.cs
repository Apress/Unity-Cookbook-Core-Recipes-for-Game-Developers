using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

//Ideally attach this script only to the Players weapon to keep performance optimized.
//Using on NPC Weapons wont have much of a visual effect and will deteorate performance.
public class WeaponCasing : MonoBehaviour
{

    [SerializeField][Tooltip("The Shell Casing belonging to this Weapon")] private GameObject shellCasingPrefab;
    //Where to have the bullet casing eject from 
    [SerializeField][Tooltip("Location from where the Bullet Shell Casing should be Ejected")] private Transform ejectPointTransform;
    // Serialized field for ejection direction
    [SerializeField]
    [Tooltip("Direction in which the shell casing will be ejected. Adjust in local space relative to the eject point transform.")]
    private Vector3 ejectionDirection = Vector3.right;
    //Eject force to be applied to the shell casing
    [SerializeField][Tooltip("Ejection Force for Shell Casing")][Range(1.0f, 10.0f)] private float ejectForce = 2.5f;
    //How many seconds should the shell casing last before being removed.
    [SerializeField][Tooltip("How many seconds should the shell casing last in the World")] private float seconds = 1.5f;

    private Weapon weapon;

    private ObjectPool<GameObject> shellCasingPool;


    private void OnValidate()
    {
        if (shellCasingPrefab == null)
            Debug.LogError($"[Weapon Casing - {gameObject.name}] : The property 'Shell Casing Prefab' cannot be left empty. It needs to be assigned a Prefab");


        if (ejectPointTransform == null)
            Debug.LogError($"[Weapon Casing - {gameObject.name}] : The property 'Eject Point Transform' cannot be left empty. It needs to be assigned a Tranform that represents the Position from where the Bullet Casing should be Ejected");

        if (seconds <= 0)
            Debug.LogError($"[Weapon Casing - {gameObject.name}] : The property 'Seconds' cannot be a Zero or Negative value.");
    }

    private void Awake()
    {
        weapon = GetComponent<Weapon>();

        // Initialize the object pool
        shellCasingPool = new ObjectPool<GameObject>(
            createFunc: CreateShellCasing,
            actionOnGet: OnGetShellCasing,
            actionOnRelease: OnReleaseShellCasing,
            actionOnDestroy: OnDestroyShellCasing,
            collectionCheck: false,
            defaultCapacity: 100,
            maxSize: 500);
    }

    private void OnEnable()
    {
        weapon.OnWeaponFired += EjectCasing;
    }

    private void OnDisable()
    {
        weapon.OnWeaponFired -= EjectCasing;
    }

    private GameObject CreateShellCasing()
    {
        return Instantiate(shellCasingPrefab);
    }

    private void OnGetShellCasing(GameObject shellCasing)
    {
        shellCasing.SetActive(true);
    }

    private void OnReleaseShellCasing(GameObject shellCasing)
    {
        shellCasing.SetActive(false);
    }

    private void OnDestroyShellCasing(GameObject shellCasing)
    {
        Destroy(shellCasing);
    }
    void EjectCasing()
    {
        GameObject shellCasing = shellCasingPool.Get();
        Rigidbody rb = shellCasing.GetComponent<Rigidbody>();
        shellCasing.transform.position = ejectPointTransform.position;
        shellCasing.transform.rotation = ejectPointTransform.rotation;

        // Apply force in the specified ejection direction, transformed to world space
        Vector3 worldEjectionDirection = ejectPointTransform.TransformDirection(ejectionDirection.normalized);
        if (rb != null)
            rb.AddForce(worldEjectionDirection * ejectForce, ForceMode.VelocityChange);
        else
        {
            Debug.LogError("Shell Casing does not have a Rigidbody component attached.");
            shellCasingPool.Release(shellCasing); // Release back to pool to prevent leakage.
            return;
        }

        // With object pooling in place, you would activate and then deactivate after a delay, instead of destroying.
        StartCoroutine(ReturnToPoolAfterDelay(shellCasing, seconds));
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject shellCasing, float delay)
    {
        yield return new WaitForSeconds(delay);
        shellCasingPool.Release(shellCasing);
    }
}
