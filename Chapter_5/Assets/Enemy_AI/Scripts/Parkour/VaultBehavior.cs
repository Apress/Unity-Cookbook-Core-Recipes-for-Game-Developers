using UnityEngine;

[CreateAssetMenu(fileName = "NewVaultAction", menuName = "Parkour/ParkourVaultActionData", order = 2)]
public class VaultBehavior : ParkourBehavior
{
    public override bool ParkourActionPossible(HitInfo hitInfo, Transform player)
    {
        if(!base.ParkourActionPossible(hitInfo, player))    return false;

        Vector3 hitPointLocalSpace = hitInfo.hitData.transform.InverseTransformPoint(hitInfo.hitData.point);

        if ((hitPointLocalSpace.z < 0 && hitPointLocalSpace.x < 0) || (hitPointLocalSpace.z > 0 && hitPointLocalSpace.x > 0))
        {
            Mirror = true; //mirror the animation using the right hand for the animation.
            avatarTargetBodyPart =AvatarTarget.RightHand;
        }
        else
        {
            Mirror = false; //Don't mirror the animation - use default left hand set in the Vault_SO scriptable obect. 
            avatarTargetBodyPart = AvatarTarget.LeftHand;
        }
        return true;
    }
}

