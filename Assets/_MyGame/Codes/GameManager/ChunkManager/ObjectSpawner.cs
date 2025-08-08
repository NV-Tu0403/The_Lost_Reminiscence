using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct ObjectGroup
    {
        public GameObject[] objects; // Mảng các GameObject
        public float density; // Mật độ (số object trên đơn vị diện tích)
    }

    public float chunkSize = 10f; // Kích thước bề mặt (chunkSize x chunkSize)
    public ObjectGroup[] objectGroups; // Mảng các nhóm object
    public bool create = false; // Kiểm soát việc tạo objects

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private float surfaceArea; // Diện tích bề mặt

    void Start()
    {
        // Tính diện tích bề mặt
        surfaceArea = chunkSize * chunkSize;
    }

    void Update()
    {
        if (create)
        {
            CreateObjects();
            create = false; // Đặt lại để tránh tạo liên tục
        }
    }

    void CreateObjects()
    {
        // Xóa các object đã tạo trước đó
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }
        spawnedObjects.Clear();

        // Lấy vị trí tâm từ GameObject chứa script
        Vector3 surfaceCenter = transform.position;

        // Tính toán và đặt các object cho mỗi nhóm
        foreach (ObjectGroup group in objectGroups)
        {
            // Tính số lượng object cần tạo dựa trên mật độ và diện tích
            int objectCount = Mathf.FloorToInt(surfaceArea * group.density);

            for (int i = 0; i < objectCount; i++)
            {
                // Chọn ngẫu nhiên một object từ mảng
                GameObject prefab = group.objects[Random.Range(0, group.objects.Length)];

                // Tạo vị trí ngẫu nhiên trong khu vực bề mặt
                Vector3 randomPos = new Vector3(
                    surfaceCenter.x + Random.Range(-chunkSize / 2f, chunkSize / 2f),
                    surfaceCenter.y, // Giữ nguyên y của tâm
                    surfaceCenter.z + Random.Range(-chunkSize / 2f, chunkSize / 2f)
                );

                // Tạo object
                GameObject newObj = Instantiate(prefab, randomPos, Quaternion.identity);
                spawnedObjects.Add(newObj);
            }
        }
    }

    // Xem trước trong Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(chunkSize, 0.1f, chunkSize));
    }
}