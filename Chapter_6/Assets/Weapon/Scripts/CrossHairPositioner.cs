using UnityEngine;

//This script is attached to the 'CrossHairCanvasObject'.
//The CrossHairCanvasObject' must be a child of the Camera object  that is attached to the Weapon.

public class CrossHairPositioner : MonoBehaviour
{
    [SerializeField] private Transform weaponCamera;
    //How far out can you see with this weapon cross hair. Changes with different weapon types.
    [SerializeField] private float sightDistance = 50f;
    private Vector3 defaultCrossHairPosn;
    private Vector3 defaultCrossHairScale;
    private RaycastHit hit;
    private Ray ray;
    private int layer;
    void Start()
    {
        //Target objects only on the Head, Body and Shootable layers i.e. NPCs and Explodable Crates/Barrels 
        layer = (1 << 8) | (1 << 9) | (1 << 10);

        defaultCrossHairPosn = transform.localPosition;
        defaultCrossHairScale = transform.localScale;

        // Debug.Log($"Layer Value is : {layer}");
    }

    void Update()
    {
        ray = new Ray(weaponCamera.transform.position, weaponCamera.transform.rotation * Vector3.forward);
        //Raycasting for objects on Head,Body,Shootable Layers only. The ray will be cast upto sightDistance meteres 
        //in the forward direction, if true is returned a shootable object on the Shootable layer has been located.
        if (Physics.Raycast(ray, out hit, sightDistance, layer))
        {

            if (hit.distance > defaultCrossHairPosn.z)
            {
                //move the cross hair to the hit position, less 30% so the cross hair is a bit in front of its actual hit point
                //and does not disappear into the shootable object.
                //transform.localPosition = new Vector3(0f, 0f, (hit.distance * 0.95f));
                transform.localPosition = new Vector3(0f, 0f, (hit.distance * 0.7f));

                //Note the local scale of the canvas to which this script is attached is the same in X,Y & Z.
                //At start its default scale is 0.0003 and this value will increase / decrease based on the distance.
                float scaledValue = hit.distance * defaultCrossHairScale.x;
                transform.localScale = new Vector3(scaledValue, scaledValue, defaultCrossHairScale.z);

                //Debug.Log($"Located Enemy Object : {hit.transform.tag} at a distance of : {hit.distance} meters");
            }
            else //the hit.distance is either equal to or less than the cross hairs z distance from the shootable object being targeted.
            {
                transform.localPosition = defaultCrossHairPosn; //move the cross hair back to its default location.
                transform.localScale = defaultCrossHairScale; // rescale cross hair to its default size.
            }

        }
        else //no shootable object has been sighted
        {
            // Debug.Log("No Shootable Target found");
            transform.localPosition = defaultCrossHairPosn; //move the cross hair back to its default location.
            transform.localScale = defaultCrossHairScale; // rescale cross hair to its default size.
        }

    }

}
