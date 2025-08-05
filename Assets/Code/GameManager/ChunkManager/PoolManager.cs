using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    /// <summary>
    /// Dictionary để quản lý các pool của GameObject.
    /// </summary>
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Preload các GameObject vào pool.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    /// <param name="key"></param>
    public void Preload(GameObject prefab, int count, string key)
    {
        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab);
            obj.SetActive(false);
            pools[key].Enqueue(obj);
        }
    }

    /// <summary>
    /// Lấy một GameObject từ pool theo key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public GameObject Get(string key)
    {
        if (!pools.ContainsKey(key) || pools[key].Count == 0)
        {
            Debug.LogWarning($"Pool {key} empty or not found!");
            return null;
        }
        var obj = pools[key].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Trả lại một GameObject vào pool theo key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    public void Return(string key, GameObject obj)
    {
        obj.SetActive(false);
        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();
        pools[key].Enqueue(obj);
    }
}