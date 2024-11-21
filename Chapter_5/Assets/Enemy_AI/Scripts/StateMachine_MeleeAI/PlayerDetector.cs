using UnityEngine;
using System.Collections;
public class PlayerDetector : MonoBehaviour
{
    private WarriorStateMachine stateMachine;
    private Coroutine visibilityCoroutine;

    void Awake()
    {
        stateMachine = GetComponentInParent<WarriorStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError($"[PlayerDetector - {gameObject.name}] : WarriorStateMachine not found.");
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[PlayerDetector - {gameObject.name}] : Player Entered NPCs Trigger");

            // Start the coroutine to continuously check for player visibility
            if (visibilityCoroutine == null)
            {
                visibilityCoroutine = StartCoroutine(CheckPlayerVisibility());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[PlayerDetector - {gameObject.name}] : Player Exited NPCs Trigger");

            NPCManager.Instance.UnregisterOutOfRangeNpc(stateMachine);

            // Stop the coroutine when the player exits the trigger
            if (visibilityCoroutine != null)
            {
                StopCoroutine(visibilityCoroutine);
                visibilityCoroutine = null;
            }

        
        }
    }

    private IEnumerator CheckPlayerVisibility()
    {
        while (true)
        {
            if (stateMachine.IsPlayerVisible())
            {
                IState chase = stateMachine.FindState<W_ChaseState>();
                if (chase != null)
                {
                    NPCManager.Instance.RegisterInRangeNpc(stateMachine);
                    Debug.Log($"[PlayerDetector - {gameObject.name}] : Player Visible - Switching to Chase State");
                    stateMachine.AlertNearbyNPCs();//Alert NPCs within range to join attack on Player
                    stateMachine.SwitchState(chase);
                }
                yield break; //Exit the coroutine once the player becomes visible and state is switched
            }
            else
            {
                Debug.Log($"[PlayerDetector - {gameObject.name}] : Player Not in Visible Range");
            }
            yield return new WaitForSeconds(0.1f); //Check every 0.1 seconds
        }
    }
}
