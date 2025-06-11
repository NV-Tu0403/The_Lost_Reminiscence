using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCheckPoint : MonoBehaviour, ISaveable
{
    public static PlayerCheckPoint Instance { get; private set; }

    public string FileName => "PlayerCheckPoint.json";

    [SerializeField] private Transform playerTransform;
    public Transform PlayerTransform => playerTransform; 
    public string CurrentMap { get; private set; } = "Unknown";
    private string _lastSceneName;
    private PlayerCheckPointData _lastLoadedData;

    private static readonly string[] ExcludedScenes = { "Menu" }; // Danh sách scene bỏ qua

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("[PlayerCheckPoint] Another instance of PlayerCheckPoint already exists. Destroying this instance.");
        }

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("[PlayerCheckPoint] Player Transform not found in initial scene!");
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public string SaveToJson()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[PlayerCheckPoint] Player Transform is null during save!");
            return JsonUtility.ToJson(new PlayerCheckPointData { mapName = CurrentMap, position = new SerializableVector3(Vector3.zero) }, true);
        }

        if (ExcludedScenes.Contains(CurrentMap))
        {
            Debug.LogWarning($"[PlayerCheckPoint] Attempted to save excluded scene: {CurrentMap}. Using default map.");
            CurrentMap = "Unknown"; // Hoặc một scene gameplay mặc định
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
            Debug.LogWarning($"[PlayerCheckPoint] Loaded excluded scene: {data.mapName}. Using default map.");
            data.mapName = "Unknown";
        }

        // CHỈ LƯU DỮ LIỆU, KHÔNG ÁP DỤNG NGAY LẬP TỨC
        _lastLoadedData = data;
        CurrentMap = data.mapName;
        Debug.Log($"[PlayerCheckPoint] Data loaded - Map: {CurrentMap}, Position: {_lastLoadedData.position.ToVector3()}");
    }

    /// <summary>
    /// Áp dụng vị trí đã được load trước đó cho player.
    /// Phương thức này sẽ được gọi từ bên ngoài sau khi scene đã sẵn sàng.
    /// </summary>
    public void ApplyLoadedPosition()
    {
        if (_lastLoadedData == null)
        {
            Debug.LogWarning("[PlayerCheckPoint] No loaded data to apply.");
            return;
        }

        if (playerTransform == null)
        {
            // Cố gắng tìm lại playerTransform nếu nó bị null (ví dụ: player được spawn cùng scene)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("[PlayerCheckPoint] Player Transform is null, cannot apply position!");
                return;
            }
        }

        playerTransform.position = _lastLoadedData.position.ToVector3() + new Vector3(0, 5, 0); // Offset của bạn
        Debug.Log($"[PlayerCheckPoint] Applied loaded position: {playerTransform.position}");
    }

    /// <summary>
    /// Sự kiện được gọi khi một scene mới được tải.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetCurrentMapToCurrentScene();
    }

    /// <summary>
    /// Cập nhật CurrentMap thành tên của scene hiện tại nếu nó khác với tên trước đó.
    /// </summary>
    public void SetCurrentMapToCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene != null && !string.IsNullOrEmpty(currentScene.name) && currentScene.name != "Menu")
        {
            CurrentMap = currentScene.name;
            Debug.Log($"[PlayerCheckPoint] Set CurrentMap to: {CurrentMap}");
        }
        else
        {
            Debug.LogWarning($"[PlayerCheckPoint] Invalid scene, keeping CurrentMap as: {CurrentMap}");
        }
    }

    public PlayerCheckPointData GetLastLoadedData() => _lastLoadedData;

    /// <summary>
    /// Đặt lại vị trí của người chơi về (0, 0, 0).
    /// chờ đến khi vị trí được cập nhật trong frame tiếp theo.
    /// Lưu ý: Phương thức này chỉ nên được gọi khi playerTransform đã được khởi tạo.
    /// </summary>
    /// <returns></returns>
    public void ResetPlayerPositionWord()
    {
        if (playerTransform != null)
        {
            playerTransform.position = new Vector3(0, 3, 0);
            _lastLoadedData = new PlayerCheckPointData
            {
                mapName = CurrentMap,
                position = new SerializableVector3(playerTransform.position)
            };

            Debug.Log($"[PlayerCheckPoint] Player position reset to: {playerTransform.position}");
        }
        else
        {
            Debug.LogError("[PlayerCheckPoint] Player Transform is null, cannot reset position!");
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
