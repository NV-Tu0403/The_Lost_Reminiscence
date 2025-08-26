using UnityEngine;
using System.Collections.Generic; // Cần thiết để sử dụng List
using System.Linq; // Cần thiết để sử dụng Linq

public class PictureRandomizer : MonoBehaviour
{
    [Tooltip("Danh sách tất cả các vị trí có thể đặt tranh. Kéo các GameObject đánh dấu vị trí vào đây.")]
    public List<Transform> spawnPoints;

    [Tooltip("Danh sách các Prefab của Mảnh Tranh (cả thật và giả) sẽ được đặt ngẫu nhiên vào các vị trí trên.")]
    public List<GameObject> picturePrefabs;

    void Start()
    {
        RandomizePictures();
    }

    void RandomizePictures()
    {
        if (spawnPoints == null || picturePrefabs == null || spawnPoints.Count == 0 || picturePrefabs.Count == 0)
        {
            Debug.LogError("Chưa thiết lập Spawn Points hoặc Picture Prefabs cho PictureRandomizer!");
            return;
        }

        if (picturePrefabs.Count > spawnPoints.Count)
        {
            Debug.LogError("Số lượng tranh nhiều hơn số lượng vị trí! Sẽ có tranh không được tạo ra.");
        }

        // Tạo một bản sao của danh sách vị trí để xáo trộn
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        // Xáo trộn danh sách vị trí bằng thuật toán Fisher-Yates (sử dụng Linq cho đơn giản)
        System.Random rng = new System.Random();
        availableSpawnPoints = availableSpawnPoints.OrderBy(a => rng.Next()).ToList();
        
        // Đặt từng prefab tranh vào một vị trí đã được xáo trộn
        for (int i = 0; i < picturePrefabs.Count; i++)
        {
            // Nếu không còn vị trí trống, dừng lại
            if (i >= availableSpawnPoints.Count) break;

            Transform spawnPoint = availableSpawnPoints[i];
            GameObject picturePrefab = picturePrefabs[i];

            // Tạo một bản sao của prefab tại vị trí và hướng của điểm spawn
            Instantiate(picturePrefab, spawnPoint.position, spawnPoint.rotation, transform); // Đặt làm con của Randomizer để dễ quản lý
        }
    }
}