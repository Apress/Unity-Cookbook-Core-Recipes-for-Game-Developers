using UnityEngine;

[CreateAssetMenu(fileName = "NewParkourAction", menuName = "Parkour/ParkourActionData", order = 1)]
public class ParkourBehavior : ScriptableObject
{
    [SerializeField] private string animationName;//Ensure the animation name spelling in the SO exactly matches the name in the Animator 
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private string obstacleTag; //ACC

    [Header("Target Matching")]
    [SerializeField] protected AvatarTarget avatarTargetBodyPart;
    [SerializeField] private float startTargetMatching; //Percentage into the animation when target matching should commence
    [SerializeField] private float endTargetMatching; //Percentage into the animation when the avatarTargetBodyPart should reach a specific position
    [SerializeField] private Vector3 targetWeightMask = new Vector3(0f, 1f, 0f);

    //Properties
    public bool Mirror { get; set; } //ACC
    public string AnimationName => animationName;
    public AvatarTarget AvatarTargetBodyPart => avatarTargetBodyPart;
    public float StartTargetMatching => startTargetMatching;
    public float EndTargetMatching => endTargetMatching;
    public Vector3 TargetWeightMask => targetWeightMask;

    public virtual bool ParkourActionPossible(HitInfo hitInfo, Transform player)
    {
        if(!string.IsNullOrEmpty(obstacleTag) && !hitInfo.hitData.transform.CompareTag(obstacleTag)) //ACC 
            return false;

        if (hitInfo.obstacleHeight < minHeight || hitInfo.obstacleHeight > maxHeight)
        {
            return false;
        }
        return true;
    }
}
