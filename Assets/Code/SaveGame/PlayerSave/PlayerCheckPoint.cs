using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerCheckPoint : MonoBehaviour, ISaveable
{
    public static PlayerCheckPoint Instance { get; private set; }
    public string FileName => "PlayerCheckPoint.json";
    [SerializeField] private Transform playerTransform;
    public Transform PlayerTransform => playerTransform;
    public string CurrentMap { get; private set; } = "Unknown";
    private PlayerCheckPointData _lastLoadedData;
    private bool _isDirty;
    private static readonly string[] ExcludedScenes = { "Menu" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("[PlayerCheckPoint] Player Transform not found!");
            }
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    public bool ShouldSave()
    {
        //if (playerTransform == null)
        //{
        //    Debug.LogWarning("[PlayerCheckPoint] Cannot save, playerTransform is null.");
        //    return false;
        //}

        if (ExcludedScenes.Contains(CurrentMap))
        {
            Debug.LogWarning($"[PlayerCheckPoint] Scene '{CurrentMap}' is excluded from saving.");
            return false;
        }

        return true;
    }
    public bool IsDirty => _isDirty;

    public void BeforeSave()
    {
        if (playerTransform != null)
        {
            CurrentMap = SceneManager.GetActiveScene().name;
            _isDirty = true;
        }
    }

    public void AfterLoad()
    {
        _isDirty = false;
    }

    public string SaveToJson()
    {
        if (!ShouldSave())
        {
            Debug.LogWarning("[PlayerCheckPoint] Save skipped due to invalid state.");
            return string.Empty;
        }

        var data = new PlayerCheckPointData
        {
            mapName = CurrentMap,
            position = new SerializableVector3(playerTransform.position)
        };

        Debug.Log($"[PlayerCheckPoint] Saving - Map: {CurrentMap}, Position: {playerTransform.position}");
        return JsonUtility.ToJson(data, true);
    }

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[PlayerCheckPoint] Empty JSON data, skipping load.");
            return;
        }

        var data = JsonUtility.FromJson<PlayerCheckPointData>(json);
        if (ExcludedScenes.Contains(data.mapName))
        {
            data.mapName = "Unknown";
        }

        _lastLoadedData = data;
        CurrentMap = data.mapName;
    }

    public void ApplyLoadedPosition()
    {
        if (_lastLoadedData == null || playerTransform == null)
        {
            Debug.LogWarning("[PlayerCheckPoint] Cannot apply position, missing data or player.");
            return;
        }

        Vector3 loadedPos = _lastLoadedData.position.ToVector3();

        // Nếu có Rigidbody
        if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.position = loadedPos;
        }
        // Nếu có NavMeshAgent
        else if (playerTransform.TryGetComponent(out NavMeshAgent agent))
        {
            agent.Warp(loadedPos);
        }
        else
        {
            playerTransform.position = loadedPos;
        }

        _lastLoadedData = null;
        Debug.Log($"[PlayerCheckPoint] Applied position: {playerTransform.position}");
    }

    public void ResetPlayerPositionWord()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogWarning("[PlayerCheckPoint] Cannot reset position — PlayerTransform is null.");
                return;
            }
        }

        Vector3 targetPos = new Vector3(0, 3, 0);

        // Nếu có Rigidbody
        if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.MovePosition(targetPos);
        }
        else
        {
            playerTransform.position = targetPos;
        }

        _lastLoadedData = new PlayerCheckPointData
        {
            mapName = CurrentMap,
            position = new SerializableVector3(playerTransform.position)
        };

        _lastLoadedData = null;
        _isDirty = true;
        Debug.Log($"[PlayerCheckPoint] Reset position to: {playerTransform.position}");
    }

    public void SetPlayerTransform(Transform transform)
    {
        playerTransform = transform;
        Debug.Log("[PlayerCheckPoint] PlayerTransform set via OnNewGame().");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ExcludedScenes.Contains(scene.name))
        {
            CurrentMap = scene.name;
            _isDirty = true;
            Debug.Log($"[PlayerCheckPoint] Set CurrentMap to: {CurrentMap}");
        }
    }
}

/// <summary>
/// Cấu trúc để lưu trữ dữ liệu vị trí của người chơi.
/// </summary>
[System.Serializable]
public class PlayerCheckPointData
{
    public string mapName;
    public SerializableVector3 position;
    // Thêm các trường khác nếu cần thiết, ví dụ: rotation, health, v.v.
}

/// <summary>
/// cấu trúc để lưu trữ vị trí của người chơi trong không gian 3D.
/// </summary>
[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}
