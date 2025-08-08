using UnityEngine;

[CreateAssetMenu(fileName = "ChunkConfig", menuName = "Configs/ChunkConfig")]
public class ChunkConfig : ScriptableObject
{
    public float chunkSize = 10f; // Kích thước Chunk_A (10x10)
    public ChunkData[] chunkData; // Danh sách ChunkData (TallPlant, ShortPlant, v.v.)
}

[System.Serializable]
public class ChunkData
{
    public GameObject prefabHighDetail; // Prefab chi tiết cao
    public GameObject prefabLowDetail;  // Prefab chi tiết thấp (LOD)
    public float pointLoadDensity;      // Mật độ PointLoad (số object trên 100 đơn vị diện tích)
}