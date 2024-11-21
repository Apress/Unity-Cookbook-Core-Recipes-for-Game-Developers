using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***
A static utility class is often used for functions that do not require maintaining or tracking an instance state.
These are typically methods that provide a service or perform an operation based on the input parameters alone.
If your utility methods are independent and don't need to maintain state, a static class is simpler and more convenient.
Advantages:
*Static methods can be called from anywhere without needing to instantiate the class.
*Easy to call for quick or common operations without managing an object lifecycle.
*Ideal for stateless utility functions that don't rely on instance variables.
***/

public static class Utils
{
    public static Coroutine Wait(float seconds, MonoBehaviour owner, System.Action callback)
    {
        return owner.StartCoroutine(WaitCoroutine(seconds, callback));
    }

    private static IEnumerator WaitCoroutine(float seconds, System.Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    /*** Example Usage
    To use the above Wait() method, call Utils.Wait() from any MonoBehaviour script, passing the number of seconds
    to wait, the MonoBehaviour itself (this), and an optional callback action to execute after the wait.
    The callback parameter is optional, and if provided, it will be executed after the waiting period is over. 
    In the provided Wait method, owner.StartCoroutine is called to begin the coroutine WaitCoroutine. 
    Here, owner is the script instance from which the Wait method is called. If this MonoBehaviour instance is 
    destroyed, Unity automatically stops all coroutines running on it, which helps in managing the coroutine's lifecycle effectively.
    public class ExampleUsage : MonoBehaviour
    {
        void Start()
        {
            Utils.Wait(2.0f, this, () => Debug.Log("Waited for 2 seconds!"));
        }
    }
    In the ExampleUsage class above, the Wait() method is called with a 2-second delay, and after the delay, 
    it logs a message to the console.
    ***/

    //Used to generate Random numbers or pick random elements.
    public static int RandomInt(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    //Reset a Transform to its default state.
    public static void ResetTransform(Transform trans)
    {
        trans.position = Vector3.zero;
        trans.rotation = Quaternion.identity;
        trans.localScale = Vector3.one;
    }

    //Check if a layer is in a given layer mask.
    public static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    //Find all objects of a certain type that are also on a specific layer.
    public static List<T> FindObjectsOfTypeOnLayer<T>(int layer) where T : Component
    {
        T[] allOfType = GameObject.FindObjectsOfType<T>();
        List<T> onLayer = new List<T>();
        foreach (T item in allOfType)
        {
            if (item.gameObject.layer == layer)
            {
                onLayer.Add(item);
            }
        }
        return onLayer;
    }

    //Allow for smooth transitions between two values using easing.
    public static float LerpWithEasing(float start, float end, float t, float easing = 0.5f)
    {
        float normalizedTime = Mathf.Clamp01(t);
        return Mathf.Lerp(start, end, Mathf.Pow(normalizedTime, easing));
    }
}