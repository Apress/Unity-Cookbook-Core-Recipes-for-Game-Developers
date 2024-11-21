using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BroadcastMsg : MonoBehaviour
{
    public void OnAttack()
    {
        Debug.Log("Player Attacking Animation played.");
    }
}
