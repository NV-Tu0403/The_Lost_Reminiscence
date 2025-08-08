using System.Linq;
using Tu_Develop.Import.Scripts;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerCheckPoint : MonoBehaviour, ISaveable
{
    public static PlayerCheckPoint Instance { get; private set; }
    public string FileName => "PlayerCheckPoint.json";
    [SerializeField] private Transform playerTransform;
    public Transform PlayerTransform => playerTransform;
    [SerializeField] private Transform characterCameraTransform;
    [SerializeField] private Camera characterCamera; // nếu cần lấy FOV
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
                return;
            }
        }
    }

    private void LateUpdate()
    {

        if (characterCameraTransform == null)
        {
            characterCameraTransform = Core.Instance?.characterCameraObj?.transform;
        }
        if (characterCamera == null)
        {
            characterCamera = Core.Instance?.characterCameraObj?.GetComponent<Camera>();
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
        if (characterCameraTransform == null || characterCamera == null)
        {
            Debug.LogWarning("[PlayerCheckPoint] CharacterCamera is null — save will miss camera data.");
        }

        if (!ShouldSave())
        {
            Debug.LogWarning("[PlayerCheckPoint] Save skipped due to invalid state.");
            return string.Empty;
        }

        var data = new PlayerCheckPointData
        {
            mapName = CurrentMap,
            position = new SerializableVector3(playerTransform.position),
            playerRotation = new SerializableQuaternion(playerTransform.rotation),


            // camera 
            cameraPosition = new SerializableVector3(characterCameraTransform?.position ?? Vector3.zero),
            cameraRotation = new SerializableQuaternion(characterCameraTransform?.rotation ?? Quaternion.identity),
            cameraFOV = characterCamera?.fieldOfView ?? 60f
        };

        Quaternion rot = playerTransform.rotation;
        Debug.Log($"[PlayerCheckPoint] Saving - Map: {CurrentMap}, Position: {playerTransform.position},\n" +
                  $"\nRotation (Euler): {rot.eulerAngles}");
        return JsonUtility.ToJson(data, true);
    }

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[PlayerCheckPoint] Empty JSON data, skipping load.");
            return;
        }

        //var data = JsonUtility.FromJson<PlayerCheckPointData>(json);
        //if (ExcludedScenes.Contains(data.mapName))
        //{
        //    data.mapName = "Unknown";
        //}

        _lastLoadedData = JsonUtility.FromJson<PlayerCheckPointData>(json);
        CurrentMap = _lastLoadedData.mapName;
    }

    public void AssignCameraFromCore()
    {
        //var cameraObj = Core.Instance?.characterCameraObj;
        //var cameraComp = cameraObj?.GetComponent<Camera>();

        var cameraObj = characterCameraTransform;
        var cameraComp = characterCamera;

        if (cameraObj != null && cameraComp != null)
        {
            SetCharacterCamera(cameraObj.transform, cameraComp);
        }
        else
        {
            Debug.LogWarning("[PlayerCheckPoint] Failed to assign character camera from Core.");
        }
    }

    public void ApplyLoadedPosition()
    {
        if (_lastLoadedData == null || playerTransform == null)
        {
            Debug.LogWarning("[PlayerCheckPoint] Cannot apply position, missing data or player.");
            return;
        }

        Vector3 loadedPos = _lastLoadedData.position.ToVector3();
        Quaternion loadedRot = _lastLoadedData.playerRotation.ToQuaternion();

        // Nếu có NavMeshAgent
        if (playerTransform.TryGetComponent(out NavMeshAgent agent))
        {
            agent.enabled = false;
            playerTransform.position = loadedPos;
            agent.enabled = true; 
            agent.Warp(loadedPos);
            //agent.Warp(loadedPos);
        }
        // Nếu có Rigidbody
        else if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = loadedPos;
            rb.rotation = loadedRot;
        }
        else
        {
            playerTransform.position = loadedPos;
            playerTransform.position = loadedPos;
            playerTransform.rotation = loadedRot;
        }

        if (characterCameraTransform != null && characterCamera != null)
        {
            if (characterCamera.TryGetComponent(out CharacterCamera camLogic))
            {
                camLogic.ApplySavedTransform(
                    _lastLoadedData.cameraPosition.ToVector3(),
                    _lastLoadedData.cameraRotation.ToQuaternion(),
                    _lastLoadedData.cameraFOV
                );
            }
            else
            {
                characterCameraTransform.position = _lastLoadedData.cameraPosition.ToVector3();
                characterCameraTransform.rotation = _lastLoadedData.cameraRotation.ToQuaternion();
                characterCamera.fieldOfView = _lastLoadedData.cameraFOV;
            }
        }

        _lastLoadedData = null;

        Quaternion rot = playerTransform.rotation;
        Debug.Log($"[PlayerCheckPoint] Saving - Map: {CurrentMap}, Position: {playerTransform.position}," +
                  $"\nRotation (Euler): {rot.eulerAngles}");
    }

    public void SetCharacterCamera(Transform camTransform, Camera cam)
    {
        characterCameraTransform = camTransform;
        characterCamera = cam;
    }

    public void ResetPlayerPositionWord()
    {
        // Tìm Player nếu chưa có
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

        // Kiểm tra NavMeshAgent trước
        if (playerTransform.TryGetComponent(out NavMeshAgent navAgent))
        {
            // Tạm thời vô hiệu hóa NavMeshAgent để set vị trí
            navAgent.enabled = false;
            playerTransform.position = targetPos;
            navAgent.enabled = true;
            navAgent.Warp(targetPos);
        }
        // Nếu có Rigidbody
        else if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.MovePosition(targetPos);
        }
        // Nếu không có NavMeshAgent hoặc Rigidbody
        else
        {
            playerTransform.position = targetPos;
        }

        //// Lưu dữ liệu checkpoint
        //_lastLoadedData = new PlayerCheckPointData
        //{
        //    mapName = CurrentMap,
        //    position = new SerializableVector3(playerTransform.position)
        //};

        _lastLoadedData = null;
        _isDirty = true;
        Debug.Log($"[PlayerCheckPoint] Reset position to: {playerTransform.position}");
    }

    public void SetPlayerTransform(Transform transform)
    {
        playerTransform = transform;

        if (playerTransform == null)
        {
            Debug.LogWarning("[PlayerCheckPoint] PlayerTransform is null after assignment.");
            return;
        }

        Vector3 targetPos = transform.position;

        // Kiểm tra NavMeshAgent trước
        if (playerTransform.TryGetComponent(out NavMeshAgent navAgent))
        {
            // Tạm thời vô hiệu hóa NavMeshAgent để set vị trí
            navAgent.enabled = false;
            playerTransform.position = targetPos;
            navAgent.enabled = true; // Kích hoạt lại NavMeshAgent
            navAgent.Warp(targetPos); // Dùng Warp để đảm bảo NavMeshAgent cập nhật vị trí
        }
        // Nếu có Rigidbody
        else if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.MovePosition(targetPos);
        }
        // Nếu không có NavMeshAgent hoặc Rigidbody
        else
        {
            playerTransform.position = targetPos;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ExcludedScenes.Contains(scene.name))
        {
            CurrentMap = scene.name;
            _isDirty = true;
            // Debug.Log($"[PlayerCheckPoint] Set CurrentMap to: {CurrentMap}");
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
    public SerializableQuaternion playerRotation;

    public SerializableVector3 cameraPosition;
    public SerializableQuaternion cameraRotation;
    public float cameraFOV;
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

[System.Serializable]
public struct SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion(Quaternion q)
    {
        x = q.x; y = q.y; z = q.z; w = q.w;
    }

    public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
}
