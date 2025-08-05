using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    public ChunkConfig config;
    public Transform player;
    public float cullingRange = 10f; // Giảm cho chunkSize = 2
    private ChunkDataLoader dataLoader;
    private Vector2Int currentChunkIndex;
    private GameObject[,] chunkGrid = new GameObject[3, 3]; // Grid 3x3
    private List<GameObject> activeObjects = new List<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player not assigned!");
            return;
        }
        if (config == null)
        {
            Debug.LogError("ChunkConfig not assigned!");
            return;
        }
        dataLoader = GetComponent<ChunkDataLoader>();
        if (dataLoader == null)
        {
            Debug.LogError("ChunkDataLoader not assigned!");
            return;
        }
        dataLoader.Initialize();
        Debug.Log("ChunkDataLoader Initialized");
        InitializePool();
        Debug.Log("Pool Initialized");
        UpdateChunks();
        Debug.Log("Initial Chunks Updated");
    }

    private void InitializePool()
    {
        foreach (var data in config.chunkData)
        {
            if (data.prefabHighDetail == null)
            {
                Debug.LogError($"Missing prefabHighDetail in ChunkData: {data}");
                continue;
            }
            float area = config.chunkSize * config.chunkSize;
            //int count = Mathf.Min(Mathf.FloorToInt(data.pointLoadDensity * area * 9 * 1.2f), 100); // Điều chỉnh theo diện tích
            int count = Mathf.Min(Mathf.FloorToInt(data.pointLoadDensity * 100 * 9 * 1.2f));
            Debug.Log($"Preloading {count} objects for {data.prefabHighDetail.name}");
            PoolManager.Instance.Preload(data.prefabHighDetail, count, data.prefabHighDetail.name);
        }
    }

    private void Update()
    {
        if (player == null) return;
        Vector2Int newChunkIndex = GetChunkIndex(player.position);
        if (newChunkIndex != currentChunkIndex)
        {
            currentChunkIndex = newChunkIndex;
            Debug.Log($"Player moved to chunk: {currentChunkIndex}");
            UpdateChunks();
        }

        // Culling
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = activeObjects[i];
            if (obj == null)
            {
                activeObjects.RemoveAt(i);
                continue;
            }
            float distance = Vector3.Distance(obj.transform.position, player.position);
            bool inRange = distance < cullingRange;
            obj.SetActive(inRange);
            if (!inRange)
            {
                Debug.Log($"Deactivating object {obj.name} at distance {distance}");
            }
        }
    }

    private Vector2Int GetChunkIndex(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / config.chunkSize),
            Mathf.FloorToInt(position.z / config.chunkSize)
        );
    }

    private void UpdateChunks()
    {
        // Clear old objects
        foreach (var obj in activeObjects)
        {
            if (obj != null)
            {
                PoolManager.Instance.Return(obj.name.Replace("(Clone)", ""), obj);
            }
        }
        activeObjects.Clear();
        Debug.Log("Cleared active objects");

        // Update grid
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                int chunkX = currentChunkIndex.x + x - 1;
                int chunkZ = currentChunkIndex.y + z - 1;
                SpawnChunk(chunkX, chunkZ);
            }
        }
        Debug.Log($"Spawned {activeObjects.Count} objects in 9 chunks");
    }

    private void SpawnChunk(int chunkX, int chunkZ)
    {
        Vector3 chunkPos = new Vector3(chunkX * config.chunkSize, 0, chunkZ * config.chunkSize);
        for (int i = 0; i < config.chunkData.Length; i++)
        {
            var points = dataLoader.GeneratePointLoads(config.chunkData[i], new Vector2Int(chunkX, chunkZ));
            if (points == null || points.Count == 0)
            {
                Debug.LogWarning($"No PointLoads for ChunkData index {i} in chunk ({chunkX}, {chunkZ})");
                continue;
            }
            foreach (var point in points)
            {
                Vector3 worldPos = chunkPos + point;
                float distance = Vector3.Distance(worldPos, player.position);
                string key = config.chunkData[i].prefabHighDetail.name;
                var obj = PoolManager.Instance.Get(key);
                if (obj != null)
                {
                    obj.transform.position = worldPos;
                    obj.SetActive(distance < cullingRange);
                    activeObjects.Add(obj);
                    Debug.Log($"Spawned {obj.name} at {worldPos}, distance: {distance}, active: {obj.activeSelf}");
                }
                else
                {
                    Debug.LogWarning($"Failed to get object from pool: {key}");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (player == null || config == null) return;

        //// Vẽ vùng culling (hình cầu xanh)
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(player.position, cullingRange);

        // Vẽ lưới 3x3 chunk (xanh dương)
        Gizmos.color = Color.blue;
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                int chunkX = currentChunkIndex.x + x - 1;
                int chunkZ = currentChunkIndex.y + z - 1;
                Vector3 chunkPos = new Vector3(chunkX * config.chunkSize, 0, chunkZ * config.chunkSize);
                Gizmos.DrawWireCube(chunkPos + new Vector3(config.chunkSize / 2, 0, config.chunkSize / 2),
                    new Vector3(config.chunkSize, 0.1f, config.chunkSize));
            }
        }

        // Vẽ PointLoad (điểm đỏ) và active objects (điểm vàng)
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                int chunkX = currentChunkIndex.x + x - 1;
                int chunkZ = currentChunkIndex.y + z - 1;
                Vector3 chunkPos = new Vector3(chunkX * config.chunkSize, 0, chunkZ * config.chunkSize);
                for (int i = 0; i < config.chunkData.Length; i++)
                {
                    var points = dataLoader.GeneratePointLoads(config.chunkData[i], new Vector2Int(chunkX, chunkZ));
                    if (points != null)
                    {
                        Gizmos.color = Color.red;
                        foreach (var point in points)
                        {
                            Gizmos.DrawSphere(chunkPos + point, 0.1f);
                        }
                    }
                }
            }
        }

        // Vẽ active objects (điểm vàng)
        Gizmos.color = Color.yellow;
        foreach (var obj in activeObjects)
        {
            if (obj != null && obj.activeSelf)
            {
                Gizmos.DrawSphere(obj.transform.position, 0.15f);
            }
        }
    }
}