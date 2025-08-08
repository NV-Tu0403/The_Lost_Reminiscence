using UnityEngine;
using System.Collections.Generic;

public class ChunkDataLoader : MonoBehaviour
{
    public ChunkConfig config;
    private List<Vector3>[] pointLoads; // Cache PointLoad cho mỗi ChunkData

    /// <summary>
    /// Khởi tạo dữ liệu ChunkData và PointLoad.
    /// Phương thức này sẽ được gọi khi scene bắt đầu hoặc khi cần tải lại dữ liệu.
    /// </summary>
    public void Initialize()
    {
        // Không cache PointLoad, sẽ tạo động trong ChunkManager
        Debug.Log("ChunkDataLoader Initialized");
    }

    public List<Vector3> GeneratePointLoads(ChunkData data, Vector2Int chunkIndex)
    {
        List<Vector3> points = new List<Vector3>();
        float area = config.chunkSize * config.chunkSize;
        int count = Mathf.FloorToInt(data.pointLoadDensity * area); // Tính số PointLoad theo diện tích
        count = Mathf.Max(1, count); // Đảm bảo ít nhất 1 điểm
        Debug.Log($"Generating {count} PointLoads for {data.prefabHighDetail.name} in chunk {chunkIndex}");

        // Sử dụng seed dựa trên chunkIndex để đảm bảo tính nhất quán
        Random.InitState(chunkIndex.x * 1000 + chunkIndex.y);
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(0f, config.chunkSize);
            float z = Random.Range(0f, config.chunkSize);
            points.Add(new Vector3(x, 0, z));
        }
        return points;
    }

    /// <summary>
    /// Lấy danh sách PointLoad cho một ChunkData theo chỉ số.
    /// </summary>
    /// <param name="dataIndex"></param>
    /// <returns></returns>
    public List<Vector3> GetPointLoads(int dataIndex)
    {
        return pointLoads[dataIndex];
    }
}