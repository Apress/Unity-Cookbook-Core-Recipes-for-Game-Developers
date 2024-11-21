using UnityEngine;
//This script is attached to the PlayerArmature game object.

public struct HitInfo
{
    public bool hitObstacle; //determines if an obstacle was encountered
    public RaycastHit hitData; //provides information about the hit obstacle 
    public float obstacleHeight; //provides the height of the hit obstacle
    public Quaternion targetRotToFaceObstacle; //Players rotation to face obstacle to climb upon
}

[RequireComponent(typeof(LineRenderer))]
public class ObstacleSensor : MonoBehaviour
{
    [Tooltip("The approx knee height of the Player representing the height from which the ray is cast forward.")]
    [SerializeField] private float kneeHeight = 0.35f;
    [Tooltip("The length of the ray in the forward direction.")]
    [SerializeField] float rayLength = 0.75f;

    private Vector3 rayOrigin; //Define the ray's origin

    private LayerMask obstacleLayerMask;

    private HitInfo hitInfo = new HitInfo();

    private LineRenderer lineRenderer;

    private void Start()
    {
        obstacleLayerMask = LayerMask.GetMask("Obstacle");

        //Get the LineRenderer component, it is guaranteed to be present by RequireComponent attribute
        lineRenderer = GetComponent<LineRenderer>();
    }

    public HitInfo ObstacleDetected()
    {
        rayOrigin = transform.position + Vector3.up * kneeHeight;

        hitInfo.hitObstacle = Physics.Raycast(rayOrigin, transform.forward, out hitInfo.hitData, rayLength, obstacleLayerMask);

        //hitInfo.hitObstacle = Physics.SphereCast(rayOrigin, 0.2f, transform.forward, out hitInfo.hitData, rayLength, obstacleLayerMask);


        if (hitInfo.hitObstacle)
        {
            //Debug.DrawRay(rayOrigin, transform.forward * rayLength, Color.green);
            Utils.DrawRay(rayOrigin, transform.forward, rayLength, Color.green, lineRenderer);

            // Calculate the height of the object hit
            Collider hitCollider = hitInfo.hitData.collider;
            hitInfo.obstacleHeight = hitCollider.bounds.size.y;
            hitInfo.targetRotToFaceObstacle = Quaternion.LookRotation(-hitInfo.hitData.normal);

            Debug.Log($"Obstacle detected with height: {hitInfo.obstacleHeight}");
        }
        else
        {
            //Debug.DrawRay(rayOrigin, transform.forward * rayLength, Color.red);
            Utils.DrawRay(rayOrigin, transform.forward, rayLength, Color.red, lineRenderer);
        }

        return hitInfo;
    }
}
