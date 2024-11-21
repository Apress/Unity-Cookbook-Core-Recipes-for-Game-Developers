using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class PoolItem
{
    [Tooltip("Prefab of the Object You want Pooled")] public GameObject prefab;
    [Tooltip("Default Pool Size for above Prefab")] public int poolSize; //The defualt size of this items pool.
    [Tooltip("Max Pool Size for above Prefab")] public int poolMaxSize; //The Max size this items pool can grow to.
}

public class CentralizedObjectPooler : MonoBehaviour
{
    public static CentralizedObjectPooler Instance;

    [SerializeField][Tooltip("Total Number of Items in Game that need to be Pooled")] private List<PoolItem> itemsToPool;

    private Dictionary<GameObject, ObjectPool<GameObject>> pools;

    private void Awake()
    {
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

        foreach (var item in itemsToPool)
        {
            var pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var instance = Instantiate(item.prefab);
                    instance.transform.SetParent(this.transform, false); // Make the CentralizedObjectPooler the parent
                    return instance;
                },
                actionOnGet: (obj) =>
                {
                    obj.SetActive(true);
                    obj.transform.SetParent(null); // Optionally reset the parent to none when getting the object
                },
                actionOnRelease: (obj) =>
                {
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform, false); // Make the CentralizedObjectPooler the parent again
                },
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: item.poolSize,
                maxSize: item.poolMaxSize 
            );

            pools.Add(item.prefab, pool);
        }
    }


    public GameObject GetObject(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            return pool.Get();
        }

        Debug.LogError($"No pool found for prefab: {prefab.name}");
        return null;
    }

    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            Debug.LogError($"No pool found for prefab: {prefab.name}");
        }
    }
}