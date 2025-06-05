using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCheckPoint : MonoBehaviour, ISaveable
{
    public string FileName => "PlayerCheckPoint.json";

    [SerializeField] private Transform playerTransform;
    public string CurrentMap { get; private set; } = "Unknown";
    private string _lastSceneName;

    private void Awake()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("[PlayerCheckPoint] Player Transform not found!");
            }
        }
    }

    private void Update()
    {
        //Debug.Log($"[PlayerCheckPoint] Current Map: {CurrentMap}, Player Position: {playerTransform.position}");
        string currentScene = SceneManager.GetActiveScene().name;
        if (_lastSceneName != currentScene)
        {
            CurrentMap = currentScene;
            _lastSceneName = currentScene;
        }
    }

    /// <summary>
    /// Đặt tên bản đồ hiện tại mà người chơi đang ở bằng tên scene hiện tại.
    /// </summary>
    public void SetCurrentMapToCurrentScene()
    {
        CurrentMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    public string SaveToJson()
    {
        var data = new PlayerCheckPointData
        {
            mapName = CurrentMap,
            position = new SerializableVector3(playerTransform.position)
        };

        return JsonUtility.ToJson(data, true);
    }

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<PlayerCheckPointData>(json);
        CurrentMap = data.mapName;

        // Di chuyển player tới vị trí cũ
        playerTransform.position = data.position.ToVector3();

        Debug.Log($"[PlayerPositionManager] Loaded Player position: {data.position.ToVector3()}, Map: {CurrentMap}");
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
