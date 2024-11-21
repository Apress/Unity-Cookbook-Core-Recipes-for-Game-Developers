using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Collections;

//This script is attached to the PlayerArmature game object.
public class ParkourManager : MonoBehaviour
{
    //Below events being listened for in ThirdPersonController script.
    public event Action OnEnterParkourMode, OnExitParkourMode;

    [Tooltip("Drag and Drop all Parkour Behavior Scriptable Objects into this List")]
    [SerializeField] private List<ParkourBehavior> parkourBehaviors;

    private ObstacleSensor obstacleSensor;
    private Keyboard keyboard;
    private Animator anim;
    private bool inParkourAction;
    private float rotSpeed = 500f;
    private Quaternion targetRotation;//Rotates Player to face Obstacle to be climbed


    private void Awake()
    {
        anim = GetComponent<Animator>();
        obstacleSensor = GetComponent<ObstacleSensor>();

        keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.LogError("Keyboard Not found");
            return;
        }
    }

    private void Update()
    {
        if (keyboard.cKey.wasPressedThisFrame && !inParkourAction)
        {
            HitInfo hitInfo = obstacleSensor.ObstacleDetected();
            
            if (hitInfo.hitObstacle)
            {
                Debug.Log($"Obstacle hit was: {hitInfo.hitData.transform.name} - Its Height is: {hitInfo.obstacleHeight}");

                foreach (ParkourBehavior action in parkourBehaviors)
                {
                    if (action.ParkourActionPossible(hitInfo, transform))
                    {
                        OnEnterParkourMode?.Invoke(); //Event being listened for in ThirdPersonController script.
                        anim.applyRootMotion = true; 
                        StartCoroutine(ParkourAction(action,hitInfo));
                        break;
                    }
                }
            }
        }
    }

    private void PerformTargetMatching(ParkourBehavior action, HitInfo hitInfo)
    {
        if (anim.isMatchingTarget) return; //if Target Matching already in progress.

        anim.MatchTarget(new Vector3(0f,hitInfo.obstacleHeight,0f), transform.rotation, action.AvatarTargetBodyPart, new MatchTargetWeightMask(action.TargetWeightMask, 0), action.StartTargetMatching, action.EndTargetMatching);
        Debug.Log("Performed Target Matching");
    }

    IEnumerator ParkourAction(ParkourBehavior action, HitInfo hitInfo)
    {
        inParkourAction = true;

        anim.SetBool("mirrorVault", action.Mirror); //ACC

        anim.CrossFade(Animator.StringToHash(action.AnimationName), 0.2f);
        yield return null; //Wait for next frame, so animation is playing

        // Wait until the concerned animation is nearly completed
        yield return new WaitUntil(() => IsAnimationNearlyComplete(0.99f,action,hitInfo));
        anim.applyRootMotion = false; //Deactivate root motion.
        inParkourAction = false;
        OnExitParkourMode?.Invoke(); //Event being listened for in ThirdPersonController script
    }
        
    private bool IsAnimationNearlyComplete(float threshold, ParkourBehavior action, HitInfo hitInfo)
    {
        targetRotation = Quaternion.LookRotation(-hitInfo.hitData.normal);//Rotation to face obstacle

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0); //Using Base layer 0 
        //Rotate Player to face obstacle
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        PerformTargetMatching(action, hitInfo);//Target Matching being performed while waiting on animation to complete
        return stateInfo.IsName(action.AnimationName) && stateInfo.normalizedTime >= threshold;
    }
}
