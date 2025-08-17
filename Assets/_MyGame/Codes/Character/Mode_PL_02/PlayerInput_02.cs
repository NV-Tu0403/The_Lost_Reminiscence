using DuckLe;
using UnityEngine;

public class PlayerInput_02 : MonoBehaviour
{
    private Core_02 _core;
    private string mess = null;

    [Header("reference")]
    public CharacterCamera _characterCamera;
    public Camera _camera;
    public PlayerController_02 _playerController;

    public bool isInputLocked = false;

    public Vector3 dir;

    private float _lastSpacePressTime;
    private int _spacePressCount;

    private int currentIndex = 0;
    private float aimCooldown = 0.5f;           // Thời gian cooldown giữa 2 lần bật aim
    private float nextAvailableAimTime = 0f;    // Thời điểm tiếp theo được phép aim

    public float lastAnimationTime;             // Thời gian lần cuối animation được kích hoạt
    public bool isOnCooldown;                   // Trạng thái cooldown

    private void Awake()
    {
        if (_core == null) _core = Core_02.Instance;
    }

    private void Start()
    {
        _playerController = GetComponent<PlayerController_02>();
        InitializeCamera();
    }

    private void FixedUpdate()
    {
        _camera = _characterCamera.mainCamera;
        dir = GetMoveInput();
        CharacterActionType actionType = GetSpecialActionsInput();

        if (!isInputLocked) // <=> true
        {
            _playerController.PerformMoveInput(actionType, dir);
        }
    }

    void Update()
    {


        GetAttackInput();
        GetObjInListSlot();
        GetUseResourceInput();
        InteractInput();

        if (Input.GetKeyDown(KeyCode.K))
        {
            _playerController.ActiveAttack = !_playerController.ActiveAttack;
            _playerController.RightHandObject.SetActive(_playerController.ActiveAttack);
        }
    }

    private void InitializeCamera()
    {
        GameObject cameraPrefab = Resources.Load<GameObject>("Prefab Loaded/CharacterCamera");
        if (cameraPrefab == null)
        {
            Debug.LogError("Prefab Loaded/CharacterCamera không tìm thấy!");
            return;
        }

        GameObject cameraInstance = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        _characterCamera = cameraInstance.GetComponent<CharacterCamera>();
        if (_characterCamera == null)
        {
            Debug.LogError("CharacterCamera component không tìm thấy trên prefab!");
            return;
        }
        //Debug.Log(mess);
        _characterCamera.SetTarget(transform);
    }

    /// <summary>
    /// trả về hướng di chuyển của nhân vật dựa trên input từ bàn phím.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMoveInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
        {
            return Vector3.zero;
        }

        Vector3 dir = Vector3.zero;
        if (_characterCamera?.mainCamera != null)
        {
            Vector3 camForward = _characterCamera.mainCamera.transform.forward;
            Vector3 camRight = _characterCamera.mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            dir = (camForward * v + camRight * h).normalized;
        }
        else
        {
            dir = new Vector3(h, 0, v).normalized;
            Debug.LogWarning("Không tìm thấy camera trong CharacterCamera, dùng input thô!");
        }

        //Debug.Log($"dir la: {dir}");
        return dir;
    }

    /// <summary>
    /// trả về loại di chuyển đặc biệt dựa trên các phím tắt.
    /// </summary>
    /// <returns></returns>
    public CharacterActionType GetSpecialActionsInput()
    {
        CharacterActionType moveType = CharacterActionType.Walk; // mặc định
        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);
        bool isSpacePressed = Input.GetKeyDown(KeyCode.Space);
        float doubleTapTimeWindow = 0.5f; // thời gian double tap dash (0.3-0.4s là hợp lý)

        // Xử lý double tap Space cho Dash
        if (isSpacePressed)
        {
            // Nếu lần nhấn thứ hai trong khoảng thời gian cho phép
            if (Time.time - _lastSpacePressTime <= doubleTapTimeWindow && _spacePressCount == 1)
            {
                _spacePressCount = 0; // reset
                mess = "Dash triggered (double Space)";
                //Debug.LogWarning(mess);
                return CharacterActionType.Dash;
            }
            else
            {
                // Lần nhấn đầu tiên
                _spacePressCount = 1;
                _lastSpacePressTime = Time.time;
                mess = "Space key pressed (Jump)";
                return CharacterActionType.Jump; // ưu tiên jump khi nhấn lần đầu
            }
        }

        // Reset nếu quá thời gian double tap
        if (Time.time - _lastSpacePressTime > doubleTapTimeWindow)
        {
            _spacePressCount = 0;
        }

        // Ưu tiên Sprint nếu giữ Shift
        if (isShiftHeld)
        {
            moveType = CharacterActionType.Sprint;
        }

        return moveType;
    }

    private void GetAttackInput()
    {
        if (Input.GetMouseButtonDown(0)) _playerController.PerformAttackInput(CharacterActionType.Attack, dir);

        if (Input.GetKeyDown(KeyCode.Q)) _playerController.PerformAttackInput(CharacterActionType.ThrowItem, dir);

        //if (Time.time >= nextAvailableAimTime) // Kiểm tra cooldown
        //{
        if (Input.GetMouseButtonDown(1))
        {
            _playerController.throwTimer.UpdateTimer(true);
            if (_characterCamera != null)
            {
                _playerController.Aim(true);
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _playerController.Aim(false);
            _playerController.config.throwForce = _playerController.CalculateThrowForce();
            _playerController.PerformAttackInput(CharacterActionType.ThrowWeapon, dir);
            // Đặt lại cooldown
            nextAvailableAimTime = Time.time + aimCooldown;
        }
        //}
    }

    private void GetUseResourceInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _playerController.ChangeResource(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _playerController.ChangeResource(3);
        }
    }

    public void InteractInput()
    {
        if (Input.GetKeyDown(KeyCode.E)) // nhặt
        {
            _playerController.PerformInteractInput(CharacterActionType.PickUp, _playerController.CurrentSourcesLookAt);
        }
        // if (Input.GetKeyDown(KeyCode.F)) // kích hoạt vật thể
        // {
        //     Debug.Log("Chưa viết xong chức năng Kích hoạt vật thể!");
        // }
    }

    public void GetObjInListSlot()
    {
        float scroll = Input.mouseScrollDelta.y;
        int totalSlots = _playerController.ListSlot.transform.childCount;

        if (totalSlots == 0)
        {
            //Debug.Log("Không có Item nào trong ListSlot!");
            return;
        }
        //if (isInputLocked) // Kiểm tra nếu input bị khóa
        //{
        //    Debug.Log("Input is locked, không thay đổi slot.");
        //    return;
        //}
        if (totalSlots <= 0)
        {
            //Debug.Log("Không có Item nào để thay đổi.");
            return;
        }
        if (currentIndex < 0 || currentIndex >= totalSlots)
        {
            currentIndex = 0; // Đặt lại về chỉ số hợp lệ
        }
        if (scroll == 0)
        {
            return; // Không làm gì nếu không có scroll
        }

        if (scroll > 0)
        {
            currentIndex = (currentIndex + 1) % totalSlots;
            _playerController.ChangeSlotIndex(currentIndex);
        }
        else if (scroll < 0)
        {
            currentIndex = (currentIndex - 1 + totalSlots) % totalSlots;
            _playerController.ChangeSlotIndex(currentIndex);
        }
    }

    #region  Methods
    public Vector3 ReturnPointInput()
    {
        var pointLookAt = _playerController.ReturnPoinHit();
        Debug.Log("[PlayerInput] PointLookAt: " + pointLookAt);
        return pointLookAt;
    }

    #endregion
}
