using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class WeaponRaycast : MonoBehaviour
{
    [SerializeField]
    [Tooltip("A transform that determines where the raycast starts")]
    private Transform muzzlePoint;

    [SerializeField]
    [Tooltip("Max distance the shot can hit a Target")]
    private float fireDist = 100f;

    [SerializeField]
    [Tooltip("The amount of damage caused by a Head Shot")]
    private int headshotDamage = 100;

    [SerializeField]
    [Tooltip("The amount of damage caused by a Body Shot")]
    private int bodyshotDamage = 20;

    //Ensure you assign shootableLayer in the Unity editor to match the layers of your shootable objects.
    //This code assumes that all shootable objects are assigned specific shootable layers you define.
    //Examples of your shootableLayer could be 'Head', 'Body', 'Wall', 'Floor', 'Crate', 'Container', 'Door' etc
    [SerializeField]
    [Tooltip("Layer mask to specify which layers the raycast should hit")]
    private LayerMask shootableLayer;

    private void OnValidate()
    {
        if (muzzlePoint == null)
            Debug.LogWarning($"muzzlePoint : {gameObject.name} : You have not setup its value in the Inspector");
    }

    private void OnEnable()
    {
        GetComponent<Weapon>().OnWeaponFired += CastRay;
    }
    private void OnDisable()
    {
        GetComponent<Weapon>().OnWeaponFired -= CastRay;
    }
    private void CastRay() //Method is called only when Weapon is fired.
    {
        Ray ray = new Ray(muzzlePoint.position, muzzlePoint.forward);

        #if UNITY_EDITOR
            Debug.DrawRay(muzzlePoint.position, muzzlePoint.forward * fireDist, Color.red, 2f);
        #endif


        if (Physics.Raycast(ray, out RaycastHit hit, fireDist, shootableLayer))
        {
            //Start searching for the Health component within the gameObject that the collider is attached to and
            //continue searching upwards through its parent, grandparent, and so on, until it either finds a Health component
            //or reaches the root of the hierarchy. If a Health component is found at any level in this hierarchy, it will be returned;
            //if not, null is returned.
            Health targetHealth = hit.collider.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Head"))
                {
                    targetHealth.TakeHealth(headshotDamage); // Headshot
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Body"))
                {
                    targetHealth.TakeHealth(bodyshotDamage); // Body shot
                }
            }

            #if UNITY_EDITOR
                        Debug.DrawRay(hit.point, hit.normal, Color.green, 5f);
            #endif
        }
    }
}
