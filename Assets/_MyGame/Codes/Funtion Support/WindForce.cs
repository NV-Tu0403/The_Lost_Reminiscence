using UnityEngine;
using System.Collections.Generic;

public class WindForce : MonoBehaviour
{
    [Header("Wind Direction Toggles")]
    [SerializeField] private bool enableForward = true;  // Hướng gió phía trước (+Z)
    [SerializeField] private bool enableBackward = false; // Hướng gió phía sau (-Z)
    [SerializeField] private bool enableRight = false;   // Hướng gió phải (+X)
    [SerializeField] private bool enableLeft = false;    // Hướng gió trái (-X)
    [SerializeField] private bool enableUp = false;      // Hướng gió lên (+Y)
    [SerializeField] private bool enableDown = false;    // Hướng gió xuống (-Y)

    [Header("Wind Force Settings")]
    [SerializeField] private bool useCollider = false;   // Bật/tắt chế độ phóng prefab
    [SerializeField] private float minForce = 5f;        // Cường độ gió tối thiểu
    [SerializeField] private float maxForce = 15f;       // Cường độ gió tối đa
    [SerializeField] private float changeFrequency = 1f; // Tần suất thay đổi lực gió (giây)
    [SerializeField] private float windRadius = 10f;     // Bán kính ảnh hưởng của gió
    [SerializeField] private LayerMask affectedLayers;   // Các layer bị ảnh hưởng bởi gió (khi useCollider = false)

    [Header("Prefab Spawn Settings")]
    [SerializeField] private GameObject windObject;      // Prefab windObject trong scene
    [SerializeField] private float spawnInterval = 0.5f; // Tần suất phóng prefab (giây)
    [SerializeField] private float spawnOffset = 1f;     // Khoảng cách từ vị trí nguồn khi phóng
    [SerializeField] private float lifeTime = 3f;        // Thời gian tồn tại của windObject (giây)

    private float timer = 0f;
    private float spawnTimer = 0f;
    private float currentForceMagnitude;
    private List<GameObject> windObjectPool;             // Pool chứa các windObject
    private List<float> activeObjectTimers;              // Thời gian tồn tại của các đối tượng đang hoạt động

    private void Start()
    {
        // Khởi tạo lực gió ban đầu
        UpdateWindForce();

        // Khởi tạo pool
        windObjectPool = new List<GameObject>();
        activeObjectTimers = new List<float>();
        InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        if (windObject == null)
        {
            Debug.LogError("windObject is not assigned in the Inspector!");
            return;
        }

        // Thêm windObject gốc vào pool và tắt nó
        windObject.SetActive(false);
        windObjectPool.Add(windObject);

        // Tạo thêm 2 bản sao
        for (int i = 0; i < 2; i++)
        {
            GameObject newObject = Instantiate(windObject, transform.position, Quaternion.identity);
            newObject.SetActive(false);
            windObjectPool.Add(newObject);
        }

        Debug.Log($"Initialized windObject pool with {windObjectPool.Count} objects.");
    }

    private void FixedUpdate()
    {
        // Cập nhật lực gió theo thời gian
        timer += Time.fixedDeltaTime;
        if (timer >= changeFrequency)
        {
            UpdateWindForce();
            timer = 0f;
        }

        // Cập nhật timer cho phóng prefab
        if (useCollider)
        {
            spawnTimer += Time.fixedDeltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnPrefabs();
                spawnTimer = 0f;
            }

            // Cập nhật và kiểm tra LifeTime của các đối tượng đang hoạt động
            UpdateActiveObjects();
        }
        else
        {
            ApplyWindForce();
        }
    }

    private void UpdateWindForce()
    {
        // Tạo lực ngẫu nhiên trong khoảng minForce và maxForce
        currentForceMagnitude = Random.Range(minForce, maxForce);
    }

    private void ApplyWindForce()
    {
        // Danh sách các hướng gió được bật
        Vector3[] windDirections = new Vector3[]
        {
            enableForward ? Vector3.forward : Vector3.zero,
            enableBackward ? Vector3.back : Vector3.zero,
            enableRight ? Vector3.right : Vector3.zero,
            enableLeft ? Vector3.left : Vector3.zero,
            enableUp ? Vector3.up : Vector3.zero,
            enableDown ? Vector3.down : Vector3.zero
        };

        bool anyDirectionEnabled = false;
        foreach (Vector3 direction in windDirections)
        {
            if (direction != Vector3.zero)
            {
                anyDirectionEnabled = true;
                ApplyForceInDirection(direction);
            }
        }

        if (!anyDirectionEnabled)
        {
            Debug.LogWarning("No wind directions enabled.");
            return;
        }
    }

    private void ApplyForceInDirection(Vector3 direction)
    {
        // Tìm tất cả các Rigidbody trong bán kính
        Collider[] colliders = Physics.OverlapSphere(transform.position, windRadius, affectedLayers);
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Áp dụng lực gió theo hướng cụ thể
                rb.AddForce(direction.normalized * currentForceMagnitude, ForceMode.Force);
                //Debug.Log($"Applied wind force in direction {direction} with magnitude {currentForceMagnitude} to {col.gameObject.name}");
            }
        }
    }

    private void SpawnPrefabs()
    {
        // Danh sách các hướng gió được bật
        Vector3[] windDirections = new Vector3[]
        {
            enableForward ? Vector3.forward : Vector3.zero,
            enableBackward ? Vector3.back : Vector3.zero,
            enableRight ? Vector3.right : Vector3.zero,
            enableLeft ? Vector3.left : Vector3.zero,
            enableUp ? Vector3.up : Vector3.zero,
            enableDown ? Vector3.down : Vector3.zero
        };

        bool anyDirectionEnabled = false;
        foreach (Vector3 direction in windDirections)
        {
            if (direction != Vector3.zero)
            {
                anyDirectionEnabled = true;
                SpawnPrefabInDirection(direction);
            }
        }

        if (!anyDirectionEnabled)
        {
            Debug.LogWarning("No wind directions enabled for spawning prefabs.");
            return;
        }
    }

    private void SpawnPrefabInDirection(Vector3 direction)
    {
        // Lấy một windObject từ pool
        GameObject windObj = GetPooledObject();
        if (windObj == null)
        {
            //Debug.LogWarning("No available windObject in pool.");
            return;
        }

        // Đặt vị trí và bật windObject
        windObj.transform.position = transform.position + direction.normalized * spawnOffset;
        windObj.SetActive(true);

        // Áp dụng lực cho windObject
        Rigidbody rb = windObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Reset vận tốc
            rb.AddForce(direction.normalized * currentForceMagnitude, ForceMode.Impulse);
            //Debug.Log($"Spawned {windObj.name} in direction {direction} with force {currentForceMagnitude}");
        }
        else
        {
            Debug.LogWarning($"windObject {windObj.name} does not have a Rigidbody component.");
        }

        // Thêm vào danh sách theo dõi LifeTime
        activeObjectTimers.Add(lifeTime);
    }

    private GameObject GetPooledObject()
    {
        foreach (GameObject obj in windObjectPool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return null; // Không còn đối tượng nào khả dụng trong pool
    }

    private void UpdateActiveObjects()
    {
        for (int i = activeObjectTimers.Count - 1; i >= 0; i--)
        {
            activeObjectTimers[i] -= Time.fixedDeltaTime;
            if (activeObjectTimers[i] <= 0f)
            {
                // Tắt đối tượng tương ứng
                GameObject obj = windObjectPool.Find(o => o.activeInHierarchy && activeObjectTimers[i] <= 0f);
                if (obj != null)
                {
                    obj.SetActive(false);
                    //Debug.Log($"Deactivated {obj.name} after lifetime.");
                }
                activeObjectTimers.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Vẽ vùng ảnh hưởng của gió
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, windRadius);

        // Vẽ các hướng gió
        Gizmos.color = Color.yellow;
        if (enableForward) Gizmos.DrawRay(transform.position, Vector3.forward * windRadius);
        if (enableBackward) Gizmos.DrawRay(transform.position, Vector3.back * windRadius);
        if (enableRight) Gizmos.DrawRay(transform.position, Vector3.right * windRadius);
        if (enableLeft) Gizmos.DrawRay(transform.position, Vector3.left * windRadius);
        if (enableUp) Gizmos.DrawRay(transform.position, Vector3.up * windRadius);
        if (enableDown) Gizmos.DrawRay(transform.position, Vector3.down * windRadius);
    }
}