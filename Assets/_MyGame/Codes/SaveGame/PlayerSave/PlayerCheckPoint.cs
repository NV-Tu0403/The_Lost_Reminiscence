using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerCheckPoint : MonoBehaviour, ISaveable
{
    public static PlayerCheckPoint Instance { get; private set; }
    public string FileName => "PlayerCheckPoint.json";
    [SerializeField] private Transform playerTransform;
    public Transform PlayerTransform => playerTransform;
    private Transform characterCameraTransform;
    [SerializeField] private Camera characterCamera;
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
        //_lastLoadedData = null;
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

    public void SetCharacterCamera(Transform camTransform, Camera cam)
    {
        characterCameraTransform = camTransform;
        characterCamera = cam;
    }

    public bool ApplyLoadedPositionStrict()
    {
        if (_lastLoadedData == null || playerTransform == null)
        {
            Debug.LogWarning("[PlayerCheckPoint] Cannot apply position, missing data or player.");
            return false;
        }

        Vector3 loadedPos = _lastLoadedData.position.ToVector3();
        Quaternion loadedRot = _lastLoadedData.playerRotation.ToQuaternion();
        const float epsilonPos = 0.0001f;  // độ chính xác vị trí
        const float epsilonRot = 0.01f;    // độ chính xác góc (dot quaternion ~ 1.0)

        bool success = false;

        if (playerTransform.TryGetComponent(out NavMeshAgent agent))
        {
            // Tắt auto-rotation để không bị agent ghi đè rotation mong muốn
            bool originalUpdateRotation = agent.updateRotation;
            agent.updateRotation = false;

            // Warp (đặt cứng theo navmesh)
            bool warped = agent.Warp(loadedPos);
            playerTransform.rotation = loadedRot;
            Physics.SyncTransforms();

            success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);

            // (fallback): disable/enable rồi set cứng, sau đó Warp để đồng bộ nội bộ của agent
            if (!success)
            {
                agent.enabled = false;
                playerTransform.SetPositionAndRotation(loadedPos, loadedRot);
                Physics.SyncTransforms();
                agent.enabled = true;
                agent.Warp(loadedPos); // đồng bộ nextPosition/đồ thị
                Physics.SyncTransforms();
                success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);
            }

            agent.updateRotation = originalUpdateRotation;

            if (!success)
            {
                Debug.LogError($"[PlayerCheckPoint] Failed to place with NavMeshAgent. pos={playerTransform.position} rot={playerTransform.rotation.eulerAngles}");
                return false;
            }

            Debug.Log($"[PlayerCheckPoint] Applied (NavMeshAgent) at {playerTransform.position}, rot={playerTransform.rotation.eulerAngles}");
        }

        else if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            // Lưu trạng thái và làm “cứng” vật lý để teleport an toàn
            bool originalKinematic = rb.isKinematic;
            Vector3 originalVelocity = rb.linearVelocity;
            Vector3 originalAngular = rb.angularVelocity;

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Đặt trực tiếp lên Rigidbody
            rb.position = loadedPos;
            rb.rotation = loadedRot;
            Physics.SyncTransforms();

            success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);

            // (fallback ): tắt active, set transform, bật lại
            if (!success)
            {
                bool originalActive = playerTransform.gameObject.activeSelf;
                playerTransform.gameObject.SetActive(false);
                playerTransform.SetPositionAndRotation(loadedPos, loadedRot);
                Physics.SyncTransforms();
                playerTransform.gameObject.SetActive(originalActive);
                Physics.SyncTransforms();

                success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);
            }

            // Khôi phục trạng thái Rigidbody
            rb.isKinematic = originalKinematic;

            rb.Sleep();

            if (!success)
            {
                Debug.LogError($"[PlayerCheckPoint] Failed to place with Rigidbody. pos={playerTransform.position} rot={playerTransform.rotation.eulerAngles}");
                return false;
            }

            Debug.Log($"[PlayerCheckPoint] Applied (Rigidbody) at {playerTransform.position}, rot={playerTransform.rotation.eulerAngles}");
        }
        // CharacterController 
        else if (playerTransform.TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;
            playerTransform.SetPositionAndRotation(loadedPos, loadedRot);
            Physics.SyncTransforms();
            cc.enabled = true;

            success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);

            if (!success)
            {
                Debug.LogError($"[PlayerCheckPoint] Failed to place with CharacterController. pos={playerTransform.position} rot={playerTransform.rotation.eulerAngles}");
                return false;
            }

            Debug.Log($"[PlayerCheckPoint] Applied (CharacterController) at {playerTransform.position}, rot={playerTransform.rotation.eulerAngles}");
        }
        // Transform thuần 
        else
        {
            playerTransform.SetPositionAndRotation(loadedPos, loadedRot);
            Physics.SyncTransforms();

            success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);
            if (!success)
            {
                // Fallback 
                bool originalActive = playerTransform.gameObject.activeSelf;
                playerTransform.gameObject.SetActive(false);
                playerTransform.SetPositionAndRotation(loadedPos, loadedRot);
                Physics.SyncTransforms();
                playerTransform.gameObject.SetActive(originalActive);
                Physics.SyncTransforms();

                success = IsAtTarget(playerTransform, loadedPos, loadedRot, epsilonPos, epsilonRot);
            }

            if (!success)
            {
                Debug.LogError($"[PlayerCheckPoint] Failed to place (Transform). pos={playerTransform.position} rot={playerTransform.rotation.eulerAngles}");
                return false;
            }

            Debug.Log($"[PlayerCheckPoint] Applied (Transform) at {playerTransform.position}, rot={playerTransform.rotation.eulerAngles}");
        }

        //  Camera 
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

        _lastLoadedData = null; // chỉ xóa khi đã chắc chắn thành công
        return true;

        // helper cục bộ
        static bool IsAtTarget(Transform t, Vector3 pos, Quaternion rot, float epsPos, float epsRot)
        {
            bool okPos = (t.position - pos).sqrMagnitude <= epsPos * epsPos;
            float dot = Mathf.Abs(Quaternion.Dot(t.rotation, rot));
            bool okRot = (1f - dot) <= epsRot; // dot ~1 nghĩa là rất gần nhau
            return okPos && okRot;
        }
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

        Vector3 targetPos = new Vector3(0, 5, 0);
        if (playerTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.MovePosition(targetPos + new Vector3(0, 5, 0));
        }
        else if (playerTransform.TryGetComponent(out NavMeshAgent navAgent))
        {
            // Tạm thời vô hiệu hóa NavMeshAgent để set vị trí
            navAgent.enabled = false;
            playerTransform.position = targetPos;
            navAgent.enabled = true;
            navAgent.Warp(targetPos);
        }
        else
        {
            playerTransform.position = targetPos;
        }

        // Lưu dữ liệu checkpoint
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
            rb.MovePosition(targetPos + new Vector3(0, 5, 0));
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
