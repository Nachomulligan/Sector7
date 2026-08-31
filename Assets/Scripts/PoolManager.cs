using System.Collections.Generic;
using UnityEngine;
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            instanceToPrefab[instance] = prefab;
            pools[prefab].Enqueue(instance);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject instance;

        if (queue.Count > 0)
        {
            instance = queue.Dequeue();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(prefab, position, rotation);
            instanceToPrefab[instance] = prefab;
        }

        if (instance.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnSpawn();
        }

        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnDespawn();
        }

        instance.SetActive(false);

        if (instanceToPrefab.TryGetValue(instance, out GameObject prefab))
        {
            pools[prefab].Enqueue(instance);
        }
        else
        {
            Destroy(instance);
        }
    }
}
