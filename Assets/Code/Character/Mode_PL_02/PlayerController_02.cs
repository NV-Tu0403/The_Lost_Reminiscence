using Duckle;
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController_02 : PlayerEventListenerBase
{
    #region biến cần thiết

    [Header("Debug")]
    private string mess = null;
    [SerializeField] private float checkInterval = 0.1f;
    private float lastCheckTime;
    private Vector3 groundCheckPosition; // Vị trí kiểm tra mặt đất

    [Header("Core base")]
    private Core_02 _core_02;
    private Animator _animator;
    public PlayerConfig config;

    [Header("Reference")]
    public Rigidbody _rigidbody { get; private set; }
    public NavMeshAgent _navMeshAgent { get; private set; }

    public PlayerInput_02 _playerInput;
    public Timer throwTimer = new Timer();

    public CharacterStateType CurrentPlayerState;           // Trạng thái hiện tại của người chơi
    public IUsable CurrentUsable { get; set; }

    [Header("Body")]
    public GameObject FacePlayer;                           // mặt người chơi (tạm thời)
    [SerializeField] private List<Transform> ListBody;      //   Danh sách các slot vũ khí
    public GameObject ListSlot;                             // Danh sách Slot (tạm thời)


    [Header("Slot Settings")]
    [SerializeField] private LayerMask invironmentLayer;    // Lớp để kiểm tra môi trường
    [SerializeField] private LayerMask lookAtLayerMask;     // Lớp để kiểm tra đối tượng khi nhìn vào
    public GameObject CurrentSourcesLookAt { get; set; }    // Đối tượng mà người chơi đang nhìn vào
    [SerializeField] public GameObject RightHandObject;     // đối tượng đang được sử dụng (đang nằm trong tay phải)


    [Header("Currrent character setting")]
    private Vector3 lastRotationDirection = Vector3.forward;
    [SerializeField] private bool useNavMesh = false;
    [SerializeField] private bool isGrounded = true;

    [Header("Nav character Settings")]
    [SerializeField] private float N_speed = 1.3f;
    [SerializeField] private float airDrag = 1f;
    [SerializeField] private float extraGravity = 30f;
    
    /// <summary>
    /// Lộc thêm 2 biến để giảm tốc độ di chuyển player
    /// </summary>
    private float originalSpeed;                // Lưu tốc độ gốc
    private bool isSpeedReduced = false;        // Kiểm tra xem tốc độ có đang bị giảm không
    
    [SerializeField] private float JumpMomentumBoost = 2f;
    [SerializeField] private float dashForce = 15f;         // lực dash
    [SerializeField] private float dashDuration = 0.3f;     // thời gian dash
    [SerializeField] private float dashMomentumBoost = 1.3f;// nhân vận tốc ngang

    [Header("Character action Settings")]
    //[SerializeField] private string movementMode = "Rigidbody";
    public int previousIndex = -1;                          // Lưu chỉ mục trước đó của item được trang bị
    public Coroutine deactivateCoroutine;                   // Coroutine để hủy kích hoạt item sau khi ném
    [SerializeField] private GameObject markerPrefab;       // Prefab dùng để đánh dấu (test)
    [SerializeField] private Material laserMaterial;        // Material cho laser line (test)
    [SerializeField] private float laserWidth = 0.02f;      // Độ rộng của laser line (test)
    private GameObject currentMarker;
    private LineRenderer laserLine;
    private float savedDistance = 0f;
    private float savedHeight = 0f;
    private bool isCameraSettingsSaved = false;

    private float currentDirectionX = 0f;
    private float currentDirectionZ = 0f;
    [SerializeField] private float smoothSpeed = 10f; // Tốc độ làm mượt (có thể chỉnh trong Inspector)
    public Vector3 CurrentLookAtHitPoint { get; private set; } // tọa độ điểm va chạm của tia nhìn

    [Header("Attack Settings")]
    public bool ActiveAttack = false;
    public bool isAttacking;
    public bool wait = false;
    public float waitTimeAt = 2f; // Thời gian chờ 2 giây
    private float timer = 0f;
    private float lastAttackTime;

    [SerializeField] private float[] attackDurations;
    [SerializeField] private float groundCheckRadius = 1f; // Radius of the sphere for ground check
    [SerializeField] private float groundCheckDistance = 0.4f; // Distance to check below the player

    #endregion

    protected override void Awake()
    {
        if (_core_02 == null) _core_02 = Core_02.Instance;
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
        if (_navMeshAgent == null) _navMeshAgent = GetComponent<NavMeshAgent>();

        base.Awake();
        if (_core_02 == null)
        {
            Debug.LogError("Core_02 instance is null in PlayerController_02.");
            _core_02 = Core_02.Instance;
        }
        _core_02._stateMachine.SetState(new IdleState_1(_core_02._stateMachine, _playerEvent));

        // Cấu hình NavMeshAgent
        if (_navMeshAgent != null)
        {
            _navMeshAgent.updatePosition = useNavMesh; // Chỉ cập nhật vị trí nếu dùng NavMesh
            _navMeshAgent.updateRotation = false; // Tắt xoay tự động để tự xử lý
        }
    }

    private void Start()
    {
        if (_playerInput == null) _playerInput = GetComponent<PlayerInput_02>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_navMeshAgent == null)
        {
            _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            _navMeshAgent.updatePosition = useNavMesh;
            _navMeshAgent.updateRotation = false;
        }

        _animator.SetInteger("State", 0); // Idle
        _animator.SetFloat("DirectionX", 0f);
        _animator.SetFloat("DirectionZ", 0f);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            useNavMesh = !useNavMesh;
            if (useNavMesh)
            {
                _navMeshAgent.Warp(transform.position); // Đồng bộ vị trí NavMeshAgent với Rigidbody
                _navMeshAgent.updatePosition = true;
            }
            else
            {
                _navMeshAgent.updatePosition = false;
            }
            Debug.Log($"Switched to {(useNavMesh ? "NavMesh" : "Rigidbody")} movement.");
        }

        UsingResource();
        CheckItemByLooking();

        if (!aim)
        {
            // đợi 1 giây và setbool throw = false
            if (_animator != null && _animator.GetBool("Throw") == true)
            {
                _animator.SetBool("Throw", false);
            }
        }
        WaitCheck();
    }

    private void LateUpdate()
    {
        if (_navMeshAgent != null && isGrounded)
        {
            EnableNavMeshAgent();
        }

    }

    private void FixedUpdate()
    {
        // AirControl: giảm quán tính ngang khi ở trên không
        if (!isGrounded && CurrentPlayerState == CharacterStateType.Jump)
        {
            Vector3 vel = _rigidbody.linearVelocity;
            vel.x *= airDrag;
            vel.z *= airDrag;
            _rigidbody.linearVelocity = vel;

            // tăng gravity
            _rigidbody.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    #region base Event/state

    public override void RegisterEvent(PlayerEvent e)
    {
        e.OnChangePlayerState += UpdateCurrentPlayerState;
    }

    public override void UnregisterEvent(PlayerEvent e)
    {
        e.OnChangePlayerState -= UpdateCurrentPlayerState;
    }

    private void UpdateCurrentPlayerState(CharacterStateType stateType)
    {
        CurrentPlayerState = stateType;
    }
    #endregion

    #region get input

    public void PerformMoveInput(CharacterActionType actionType, Vector3 direction)
    {
        if (_core_02 == null)
        {
            Debug.LogError("Core_02 is null in PerformMoveInput.");
            return;
        }
        if (_core_02._stateMachine == null)
        {
            Debug.LogError("StateMachine is null in PerformMoveInput.");
            return;
        }
        if (config == null)
        {
            Debug.LogError("PlayerConfig is not assigned in PlayerController_02!");
            return;
        }

        // Chuyển state
        _core_02._stateMachine.HandleAction(actionType);

        if (_animator != null)
        {
            SetAnimationParameters(actionType, direction);
        }

        float moveSpeed = config.walkSpeed; // mặc định là walk
        // Chọn tốc độ dựa trên type action
        switch (actionType)
        {
            case CharacterActionType.Run:
                moveSpeed = config.runSpeed;
                break;
            case CharacterActionType.Sprint:
                moveSpeed = config.sprintSpeed;
                break;
            case CharacterActionType.Jump:
                Jump(config.jumpImpulse);
                return;
            case CharacterActionType.Dash:
                Dash();
                return;
        }

        if (/*!isGrounded && */CurrentPlayerState != CharacterStateType.Jump && CurrentPlayerState != CharacterStateType.Dash)
            HelpCheckGround();
        //if (!isGrounded) HelpCheckGround();
        if (isGrounded && !isAttacking)
        {
            // Chọn phương thức di chuyển
            if (useNavMesh && _navMeshAgent != null)
            {
                MoveNavMesh(direction, moveSpeed, actionType);
            }
            else
            {
                Move(direction * moveSpeed, actionType);
            }
        }
    }

    public void PerformAttackInput(CharacterActionType actionType, Vector3 direction)
    {
        _core_02._stateMachine.HandleAction(actionType);
        //Debug.LogWarning($"PerformAttackInput: {actionType} with direction {direction}");

        switch (actionType)
        {
            case CharacterActionType.Attack:
                if (ActiveAttack)
                {
                    //Attack(direction, config.attackRange, config.attackDuration, config.attackDamage, config.attackCooldown);
                    int attackIndex = GetAttackIndexFromDirection(direction);
                    if (!isAttacking)
                    {
                        TryAttack(attackIndex);
                    }
                }
                break;
            case CharacterActionType.ThrowItem:
                Thrown(2f); // tạm thời
                break;
            case CharacterActionType.ThrowWeapon:
                Thrown(config.throwForce);
                break;
        }
    }

    public void PerformInteractInput(CharacterActionType actionType, GameObject currentSources)
    {
        _core_02._stateMachine.HandleAction(actionType);

        switch (actionType)
        {
            case CharacterActionType.PickUp:
                PickUp();
                break;
            case CharacterActionType.Drop:

                break;
            case CharacterActionType.Active:

                break;
        }
    }

    #endregion

    #region Action Resource
    public void EquipUsable(IUsable usable)
    {
        if (usable == null)
        {
            Debug.LogError("Attempted to equip null usable");
            return;
        }
        CurrentUsable = usable;
        Debug.Log($"Equipped usable: {CurrentUsable.Name}");
    }

    /// <summary>
    /// Sử dụng tài nguyên từ ListBody[1] (tay phải).
    /// </summary>
    public void UsingResource()
    {
        if (ListBody == null)
        {
            Debug.LogError("ListBody is null! Please initialize it in the Inspector or code.");
            return;
        }

        // kiểm tra ListBody[1] có tồn tại và có con không
        if (ListBody.Count > 2 && ListBody[1] != null && ListBody[1].childCount > 0)
        {
            Transform childTransform = ListBody[1].transform.GetChild(0);
            if (childTransform != null)
            {
                GameObject childObject = childTransform.gameObject;
                RightHandObject = childObject;
                Component component = null;

                Component[] components = RightHandObject.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c.GetType().Name == "Weapon")
                    {
                        component = c;
                        break;
                    }
                    else if (c.GetType().Name == "Loot")
                    {
                        component = c;
                        break;
                    }
                }

                if (component is IUsable usable)
                {
                    CurrentUsable = usable;
                    EquipUsable(CurrentUsable);
                    Debug.Log($"Equipped usable: {CurrentUsable.Name}, Classify: {CurrentUsable.Classify}, Effect Value: {CurrentUsable.GetEffectValue()}");
                }
                else
                {
                    Debug.LogWarning($"Weapons component not found on {RightHandObject.name}!");
                }
            }
            else
            {
                Debug.LogWarning("No GameObject found in the children of ListBody[1]!");
            }
        }
        else
        {
            //Debug.LogWarning("ListBody[1] is null or has no children!");
        }
    }
    #endregion

    #region Move

    /// <summary>
    /// Move cơ bản sử dụng Transform, không dùng Rigidbody hay NavMeshAgent.
    /// </summary>
    private void MoveBasic(Vector3 direction, CharacterActionType moveType)
    {
        float rotationSpeed = config.rotationSpeed;
        float deltaTime = Time.deltaTime;

        if (direction.sqrMagnitude <= 0.001f && moveType != CharacterActionType.Jump)
        {
            _core_02._stateMachine.SetState(new IdleState_1(_core_02._stateMachine, _playerEvent));
            return;
        }

        // Di chuyển nhân vật
        Vector3 moveVector = direction * deltaTime;
        transform.position += moveVector;

        // Xử lý xoay nhân vật
        if (_playerInput == null)
        {
            Debug.LogError("_playerInput is null in MoveBasic.");
            return;
        }

        if (_playerInput._characterCamera == null)
        {
            Debug.LogWarning("_characterCamera is null in MoveBasic. Using default rotation.");
            if (direction.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = direction;
            }
        }
        else if (!_playerInput._characterCamera.isAiming)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = direction;
            }
        }
        else if (_playerInput._characterCamera.mainCamera != null)
        {
            Vector3 camForward = _playerInput._characterCamera.mainCamera.transform.forward;
            camForward.y = 0;
            if (camForward.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = camForward.normalized;
            }
        }

        // Xoay nhân vật
        if (lastRotationDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }
    }

    /// <summary>
    /// Move bằng Rigidbody.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="moveType"></param>
    private void Move(Vector3 direction, CharacterActionType moveType)
    {
        float acceleration = config.acceleration;
        float rotationSpeed = config.rotationSpeed;

        if (this._rigidbody == null)
        {
            Debug.LogError("Rigidbody is null in MoveAction.");
            return;
        }

        if (direction.sqrMagnitude <= 0.001f && moveType != CharacterActionType.Jump)
        {
            _core_02._stateMachine.SetState(new IdleState_1(_core_02._stateMachine, _playerEvent));
            return;
        }

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 currentVelocity = this._rigidbody.linearVelocity;

        Vector3 targetVelocity = direction;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 velocityChange = targetVelocity - horizontalVelocity;

        velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration * deltaTime);
        this._rigidbody.linearVelocity += new Vector3(velocityChange.x, 0f, velocityChange.z);

        Quaternion currentRotation = this._rigidbody.rotation;

        if (_playerInput == null)
        {
            Debug.LogError("_playerInput is null in MoveAction.");
            return;
        }

        if (_playerInput._characterCamera == null)
        {
            Debug.LogWarning("_characterCamera is null in MoveAction. Using default rotation.");
            if (direction.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = direction;
            }

            if (lastRotationDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
                this._rigidbody.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * deltaTime));
            }
            return;
        }

        if (!_playerInput._characterCamera.isAiming)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = direction;
            }

            if (lastRotationDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
                this._rigidbody.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * deltaTime));
            }
        }

        else if (_playerInput._characterCamera.mainCamera != null)
        {
            Vector3 camForward = _playerInput._characterCamera.mainCamera.transform.forward;
            camForward.y = 0;

            if (camForward./*direction.*/sqrMagnitude > 0.001f)
            {
                lastRotationDirection = camForward.normalized /*direction*/;
            }

            if (lastRotationDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
                this._rigidbody.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * deltaTime));
            }
        }
    }

    /// <summary>
    /// Move bằng NavMeshAgent.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="moveSpeed"></param>
    /// <param name="moveType"></param>
    private void MoveNavMesh(Vector3 direction, float moveSpeed, CharacterActionType moveType)
    {
        float speed_n = N_speed;
        float deltaTime = Time.fixedDeltaTime;

        if (_navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is null in MoveNavMesh.");
            return;
        }

        if (direction.sqrMagnitude <= 0.001f && moveType != CharacterActionType.Jump)
        {
            _navMeshAgent.isStopped = true;
            _core_02._stateMachine.SetState(new IdleState_1(_core_02._stateMachine, _playerEvent));
            return;
        }
        _rigidbody.interpolation = RigidbodyInterpolation.None;

        // Cập nhật tốc độ của NavMeshAgent
        _navMeshAgent.speed = moveSpeed * speed_n;
        _navMeshAgent.angularSpeed = config.rotationSpeed * config.rotationSpeed;
        _navMeshAgent.acceleration = config.acceleration * speed_n;

        // Tính toán điểm đích dựa trên hướng di chuyển
        Vector3 targetPosition = transform.position + direction * moveSpeed /* * deltaTime*/;

        // Đặt điểm đích cho NavMeshAgent
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(targetPosition);

        // Xử lý xoay nhân vật
        if (_playerInput == null)
        {
            Debug.LogError("_playerInput is null in MoveNavMesh.");
            return;
        }

        if (_playerInput._characterCamera == null)
        {
            Debug.LogWarning("_characterCamera is null in MoveNavMesh. Using default rotation.");
            if (direction.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = direction;
            }
        }
        else if (!_playerInput._characterCamera.isAiming)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = direction;
            }
        }
        else if (_playerInput._characterCamera.mainCamera != null)
        {
            Vector3 camForward = _playerInput._characterCamera.mainCamera.transform.forward;
            camForward.y = 0;
            if (camForward.sqrMagnitude > 0.001f)
            {
                lastRotationDirection = camForward.normalized;
            }
        }

        // Xoay nhân vật
        if (lastRotationDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, config.rotationSpeed * deltaTime);
        }
    }

    private bool Jump(float jumpForce)
    {
        if (!isGrounded)
        {
            //Debug.Log("Cannot jump: Player is not grounded.");
            return false;
        }

        if (_animator != null)
        {
            SetAnimationParameters(CharacterActionType.Jump, Vector3.zero);
        }

        if (useNavMesh && _navMeshAgent != null)
        {
            _navMeshAgent.enabled = false;
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Vector3 horizontalVel = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);

        if (horizontalVel.sqrMagnitude < 0.01f && _playerInput != null)
            horizontalVel = _playerInput.GetMoveInput().normalized * config.runSpeed;

        float momentumBoost = JumpMomentumBoost; // boost
        Vector3 jumpVelocity = (horizontalVel * momentumBoost) + Vector3.up * jumpForce;

        _rigidbody.linearVelocity = jumpVelocity;

        isGrounded = false;
        _core_02._stateMachine.SetState(new JumpState(_core_02._stateMachine, _playerEvent));
        return true;
    }

    private bool Dash()
    {
        try
        {
            if (isGrounded)
            {
                Debug.Log("Cannot dash: Player is grounded.");
                return false;
            }

            if (_animator != null) _animator.SetTrigger("Dash");
            if (useNavMesh && _navMeshAgent != null)
            {
                _navMeshAgent.enabled = false;
                _rigidbody.isKinematic = false;
            }

            // Giữ vận tốc ngang hiện tại và tăng cường
            Vector3 horizontalVel = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            if (horizontalVel.sqrMagnitude < 0.01f && _playerInput != null)
            {
                Vector3 inputDir = _playerInput.GetMoveInput();
                if (inputDir.sqrMagnitude > 0.01f)
                    horizontalVel = inputDir.normalized * config.runSpeed;
            }

            // Tạo vận tốc dash
            Vector3 dashVelocity = horizontalVel.normalized * (horizontalVel.magnitude * dashMomentumBoost + dashForce);
            dashVelocity.y = _rigidbody.linearVelocity.y; // giữ Y (đang rơi)

            // Gán velocity ngay lập tức
            _rigidbody.linearVelocity = dashVelocity;

            //Debug.Log($"Dashing with velocity: {dashVelocity}");

            // Chuyển state Dash
            _core_02._stateMachine.SetState(new DashState(_core_02._stateMachine, _playerEvent));

            // Dừng dash sau dashDuration
            StartCoroutine(EndDashAfterTime(dashDuration));

            return true;
        }
        catch (Exception e)
        {
            mess = e.Message;
            return false;
        }
    }

    private IEnumerator EndDashAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Sau khi dash, trở về JumpState (vì vẫn đang trên không)
        if (!isGrounded)
        {
            _core_02._stateMachine.SetState(new JumpState(_core_02._stateMachine, _playerEvent));
        }
    }

    private void EnableNavMeshAgent()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;

        }
    }

    #endregion

    #region Attack
    private IEnumerator ThrownCoroutine(float force, float delay)
    {

        _animator.SetBool("Throw", true);

        // Đợi một khoảng thời gian để animation chạy
        yield return new WaitForSeconds(delay);

        GameObject thrownObject = RightHandObject?.gameObject;
        if (thrownObject != null)
        {
            thrownObject.SetActive(true);
            RightHandObject.gameObject.transform.SetParent(null);
            RightHandObject = null;
        }
        if (thrownObject == null)
        {
            // Tạo một đối tượng ném mặc định nếu không có item nào được trang bị
            GameObject DefaulObjThrow = Resources.Load<GameObject>(config.prefabPath);
            if (DefaulObjThrow == null)
            {
                Debug.LogError($"Prefab not found at path: {config.prefabPath}");
                yield break;
            }
            thrownObject = Instantiate(DefaulObjThrow);
        }

        _core_02._stateMachine.SetState(new ThrowWeaponState(_core_02._stateMachine, _playerEvent));

        if (_animator != null)
        {
            SetAnimationParameters(CharacterActionType.ThrowItem, Vector3.zero);
        }

        Ray camRay = GetCameraRayCenter();
        Vector3 forward = camRay.direction.normalized;

        Vector3 spawnPosition = FacePlayer.transform.position + forward * 1.5f;
        thrownObject.transform.position = spawnPosition;

        if (thrownObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = forward * force;
        }
        if (thrownObject.TryGetComponent<ThrowableObject>(out var throwable))
        {
            throwable.SetThrower_02(this);
            if (CurrentUsable != null) throwable.SetUsableData(CurrentUsable.Name, CurrentUsable.GetEffectValue());
        }

        CurrentUsable?.OnUse_2(this);

    }

    private void Thrown(float force)
    {
        StartCoroutine(ThrownCoroutine(force, 0.2f)); // Độ trễ 0.5 giây, bạn có thể điều chỉnh
    }

    private int GetAttackIndexFromDirection(Vector3 direction)
    {
        // Chuẩn hóa direction để dễ so sánh
        direction = direction.normalized;

        // Tính góc giữa direction và các hướng chuẩn
        float dotRight = Vector3.Dot(direction, Vector3.right);
        float dotLeft = Vector3.Dot(direction, Vector3.left);
        float dotForward = Vector3.Dot(direction, Vector3.forward);
        float dotBack = Vector3.Dot(direction, Vector3.back);

        // Tìm hướng có giá trị dot lớn nhất (gần nhất với direction)
        float maxDot = Mathf.Max(dotRight, dotLeft, dotForward, dotBack);

        if (maxDot < 0.5f) // Ngưỡng để đảm bảo hướng rõ ràng
        {
            return 3; // Không xác định được hướng
        }

        if (maxDot == dotRight)
            return 1; // Right
        if (maxDot == dotLeft)
            return 2; // Left
        if (maxDot == dotForward)
            return 3; // Forward
        if (maxDot == dotBack)
            return 4; // Back

        return 0; // Trường hợp không xác định
    }

    private bool TryAttack(int attackIndex)
    {
        // Kiểm tra chỉ số hợp lệ
        if (attackIndex < 1 || attackIndex > attackDurations.Length)
        {
            Debug.LogError($"Invalid attack index: {attackIndex}");
            return false;
        }

        // Lấy thời lượng animation tương ứng
        float attackDuration = attackDurations[attackIndex - 1];
        float attackCooldown = attackDuration + 0.5f; // Cooldown bằng thời gian animation + buffer

        // Kiểm tra cooldown
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log("Attack on cooldown.");
            return false;
        }

        return Attack(Vector3.forward, config.attackRange, attackDuration, config.attackDamage, attackCooldown, attackIndex);
    }

    private bool Attack(Vector3 attackDir, float attackRange, float attackDuration, float attackDamage, float attackCooldown, int attackIndex)
    {
        try
        {
            if (_animator == null)
            {
                Debug.LogWarning("Animator is null in Attack. No animation will be played.");
                return false;
            }

            // Thiết lập parameter để chạy animation
            isAttacking = true;
            lastAttackTime = Time.time;
            _animator.SetInteger("AnimationIndex", attackIndex);


            //Debug.Log($"Performing attack {attackIndex} with range={attackRange}, duration={attackDuration}, damage={attackDamage}, cooldown={attackCooldown}");

            _core_02._stateMachine.SetState(new AttackState(_core_02._stateMachine, _playerEvent));

            //Kiểm tra va chạm với đối tượng trong phạm vi tấn công
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * attackRange * 0.5f, attackRange);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Boss"))
                {
                    PlayerEvent.Instance.TriggerTakeOutDamage(this.gameObject, attackDamage, hit.gameObject);
                    //Debug.Log($"Hit {hit.gameObject.name} with attack {attackIndex}, damage: {attackDamage} at range: {attackRange}.");
                }
            }

            // Đặt lại trạng thái sau khi animation hoàn thành
            Invoke(nameof(ResetAttackState), attackDuration);

            //StartCoroutine(WaitApplyForceAttack(attackIndex, _rigidbody));

            //Debug.Log($"Attack {attackIndex} performed: range={attackRange}, duration={attackDuration}, damage={attackDamage}, cooldown={attackCooldown}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Attack failed: {e.Message}");
            isAttacking = false; // Đặt lại trạng thái nếu lỗi
            return false;
        }
    }

    private void ResetAttackState()
    {
        isAttacking = false; // Cho phép attack tiếp theo sau khi animation hoàn thành
        _animator.SetInteger("AnimationIndex", 0); // Đặt lại parameter về trạng thái mặc định (nếu cần)
    }

    private IEnumerator WaitApplyForceAttack(int attackIndex, Rigidbody rb)
    {
        // Đợi một khoảng thời gian trước khi áp dụng lực
        yield return new WaitForSeconds(0.7f);
        ApplyForceAttack(attackIndex, rb);
    }

    /// <summary>
    /// tạo lực cho Rigidbody dựa trên attackIndex.
    /// </summary>
    /// <param name="attackIndex"></param>
    /// <param name="rb"></param>
    public void ApplyForceAttack(int attackIndex, Rigidbody rb)
    {
        // Lấy hướng phía trước của nhân vật
        Vector3 forwardDirection = transform.forward;

        switch (attackIndex)
        {
            case 1:
                // Lực hướng về phía trước, cường độ 5f
                //rb.AddForce(forwardDirection * 15f, ForceMode.Impulse);
                ApplyRadialForce(rb, 15f, false);
                Debug.LogWarning("Applied force for attack 1: Forward direction with intensity 5f.");
                break;

            case 2:
                // Lực hướng về phía trước, cường độ 4f
                //rb.AddForce(forwardDirection * 20f, ForceMode.Impulse);
                ApplyRadialForce(rb, 20f, true);
                Debug.LogWarning("Applied force for attack 2: Forward direction with intensity 4f.");
                break;

            case 3:
                // Lực hướng ra tất cả các hướng xung quanh (trừ hướng xuống)
                ApplyRadialForce(rb, 30f, false);
                Debug.LogWarning("Applied radial force for attack 3: All directions except down with intensity 6f.");
                break;

            case 4:
                // Lực hướng ra tất cả các hướng xung quanh (trừ hướng lên)
                ApplyRadialForce(rb, 40f, true);
                Debug.LogWarning("Applied radial force for attack 4: All directions except up with intensity 10f.");
                break;

            default:
                Debug.LogWarning("Invalid attackIndex: " + attackIndex);
                break;
        }
    }

    /// <summary>
    /// hàm phụ để tạo lực hướng ra xung quanh từ vị trí của nhân vật.
    /// nhận vào 3 tham số: Rigidbody, cường độ lực và Bool ForceUp.
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="forceMagnitude"></param>
    /// <param name="excludeUp"></param>
    private void ApplyRadialForce(Rigidbody rb, float forceMagnitude, bool excludeUp)
    {
        // Lấy tất cả các Rigidbody trong bán kính 10f
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);

        foreach (Collider col in colliders)
        {
            Rigidbody targetRb = col.GetComponent<Rigidbody>();
            if (targetRb != null && targetRb != rb) // Không áp dụng lực cho chính nhân vật
            {
                // Tính hướng từ nhân vật đến đối tượng
                Vector3 direction = (col.transform.position - transform.position).normalized;

                // Nếu excludeUp = true, loại bỏ thành phần hướng lên (y dương)
                if (excludeUp && direction.y > 0)
                {
                    direction.y = 0;
                    direction = direction.normalized;
                }
                // Nếu excludeUp = false, loại bỏ thành phần hướng xuống (y âm)
                else if (!excludeUp && direction.y < 0)
                {
                    direction.y = 0;
                    direction = direction.normalized;
                }

                // Áp dụng lực
                targetRb.AddForce(direction * forceMagnitude, ForceMode.Impulse);
            }
        }
    }

    #endregion

    #region Animation Parameters
    private void SetAnimationParameters(CharacterActionType actionType, Vector3 dir)
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is null in SetAnimationParameters.");
            return;
        }

        // Ánh xạ actionType sang State
        int state = actionType switch
        {
            CharacterActionType.Idle => 0,
            CharacterActionType.Walk => 1,
            CharacterActionType.Run => 2,
            CharacterActionType.Sprint => 3,
            CharacterActionType.Dash => 4,
            CharacterActionType.Attack => 5,
            CharacterActionType.Jump => 6,
            CharacterActionType.ThrowItem => 7,
            CharacterActionType.ThrowWeapon => 8,
            _ => 0 // Mặc định là Idle
        };

        // Set State
        _animator.SetInteger("State", state);

        // Tính toán hướng nếu cần
        float targetDirectionX = 0f;
        float targetDirectionZ = 0f;
        if (actionType == CharacterActionType.Walk || actionType == CharacterActionType.Run ||
            actionType == CharacterActionType.Sprint || actionType == CharacterActionType.Dash ||
            actionType == CharacterActionType.Attack)
        {
            // Nếu không có hướng di chuyển, sử dụng hướng nhân vật đang đối mặt
            if (dir.sqrMagnitude <= 0.001f)
            {
                if (actionType != CharacterActionType.Attack)
                {
                    _animator.SetInteger("State", 0); // Idle
                    _animator.SetFloat("DirectionX", 0f);
                    _animator.SetFloat("DirectionZ", 0f);
                    currentDirectionX = 0f;
                    currentDirectionZ = 0f;
                    return;
                }
                dir = lastRotationDirection;
            }

            // Lấy hướng camera (nếu có) hoặc hướng nhân vật
            Vector3 referenceForward = _playerInput._characterCamera?.mainCamera != null
                ? _playerInput._characterCamera.mainCamera.transform.forward
                : transform.forward;
            referenceForward.y = 0;
            referenceForward = referenceForward.normalized;

            Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward).normalized;

            // Tính Dot product để xác định hướng mục tiêu
            targetDirectionX = Vector3.Dot(dir.normalized, referenceRight);
            targetDirectionZ = Vector3.Dot(dir.normalized, referenceForward);
        }

        // Làm mượt DirectionX và DirectionZ
        currentDirectionX = Mathf.Lerp(currentDirectionX, targetDirectionX, smoothSpeed * Time.deltaTime);
        currentDirectionZ = Mathf.Lerp(currentDirectionZ, targetDirectionZ, smoothSpeed * Time.deltaTime);

        // Set DirectionX và DirectionZ
        _animator.SetFloat("DirectionX", currentDirectionX);
        _animator.SetFloat("DirectionZ", currentDirectionZ);

        //Debug.Log($"SetAnimationParameters: State={state}, DirectionX={currentDirectionX}, DirectionZ={currentDirectionZ}, TargetX={targetDirectionX}, TargetZ={targetDirectionZ}, dir={dir}");
    }

    // Hàm chạy animation tương ứng
    private void PlayAnimation(int index)
    {
        // Đảm bảo Animator đã được gán
        if (_animator == null)
        {
            Debug.LogError("Animator chưa được gán trong Inspector!");
            return;
        }

        // Tên parameter trong Animator, giả sử là "AnimationIndex"
        _animator.SetInteger("AnimationIndex", index);
    }
    #endregion

    #region check collision

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;

            // Triệt tiêu quán tính ngang khi tiếp đất
            Vector3 vel = _rigidbody.linearVelocity;
            vel.x = 0;
            vel.z = 0;
            _rigidbody.linearVelocity = vel;

            // Thoát JumpState
            if (CurrentPlayerState == CharacterStateType.Jump)
            {
                _core_02._stateMachine.SetState(new IdleState_1(_core_02._stateMachine, _playerEvent));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision != null && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    private void HelpCheckGround()
    {
        //if (Time.time - lastCheckTime < checkInterval) return;
        //lastCheckTime = Time.time;

        groundCheckPosition = transform.position + Vector3.down * 0.2f;

        isGrounded = Physics.CheckSphere(groundCheckPosition, groundCheckRadius, invironmentLayer);
    }

    private void OnDrawGizmos()
    {
        if (config != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * config.attackRange * 0.5f, config.attackRange);

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
        }
    }
    #endregion

    #region Actions & logic Action

    private bool WaitCheck()
    {
        if (Input.GetMouseButtonDown(0) && !wait)
        {
            wait = true;
            timer = waitTimeAt;
        }
        if (wait)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                wait = false;
            }
        }
        return true;
    }

    /// <summary>
    /// Trả về hướng nhìn hiện tại của camera từ trung tâm màn hình.
    /// </summary>
    public Ray GetCameraRayCenter()
    {
        Camera cam = Camera.main;
        return cam != null
            ? cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
            : new Ray(Vector3.zero, Vector3.forward);
    }

    public void MarkHitPoint(Vector3 point)
    {
        // Tạo marker
        if (currentMarker == null && markerPrefab != null)
        {
            currentMarker = Instantiate(markerPrefab);
        }

        if (currentMarker != null)
        {
            currentMarker.transform.position = point;
        }

        // Tạo laser
        if (laserLine == null)
        {
            GameObject laserObj = new GameObject("LaserLine");
            laserLine = laserObj.AddComponent<LineRenderer>();
            laserLine.material = laserMaterial;
            laserLine.positionCount = 2;
            laserLine.startWidth = laserWidth;
            laserLine.endWidth = laserWidth;
            laserLine.useWorldSpace = true;
        }

        // 3. Cập nhật line
        Camera cam = Camera.main;
        if (cam != null)
        {
            laserLine.SetPosition(0, FacePlayer.transform.position);
            laserLine.SetPosition(1, point);
        }
    }

    public Vector3 ReturnPoinHit()
    {
        if (CurrentLookAtHitPoint != null)
        {
            return CurrentLookAtHitPoint;
        }
        else
        {
            Debug.LogWarning("CurrentLookAtHitPoint is null. Returning Vector3.zero.");
            return Vector3.zero;
        }
    }

    /// <summary>
    /// chiếu tia từ camera để kiểm tra đối tượng người chơi đang nhìn vào.
    /// </summary>
    public void CheckItemByLooking()
    {
        try
        {
            if (Time.time - lastCheckTime < checkInterval) return;
            lastCheckTime = Time.time;

            Ray ray = GetCameraRayCenter();
            Debug.DrawRay(ray.origin, ray.direction * 15f, Color.red, 0.1f, true);

            if (Physics.Raycast(ray, out RaycastHit hit, 50f, lookAtLayerMask))
            {
                CurrentSourcesLookAt = hit.collider.gameObject;
                CurrentLookAtHitPoint = hit.point; // Lưu tọa độ điểm va chạm
                MarkHitPoint(hit.point); // Gọi hàm đánh dấu
                                         //Debug.Log($"Looking at: {CurrentSourcesLookAt.name}");
            }
        }
        catch (System.Exception)
        {
            Debug.Log("[PlayerController] lỗi con me no roi, ngu vai lon");
        }

    }

    public void PickUp()
    {
        if (CurrentSourcesLookAt == null) return;

        if (CurrentSourcesLookAt.layer == LayerMask.NameToLayer("Item"))
        {
            CurrentSourcesLookAt.transform.SetParent(ListSlot.transform);
            CurrentSourcesLookAt.SetActive(false);
            Debug.Log($"Added {CurrentSourcesLookAt.name} to ListSlot.");
        }
        else
        {
            Debug.LogWarning("The object is not an item and cannot be added to ListSlot.");
        }
    }

    public float CalculateThrowForce()
    {
        float holdTime = throwTimer.UpdateTimer(false);
        if (holdTime <= 0f) return config.throwForceMin;
        float force = Mathf.Lerp(config.throwForceMin, config.throwForceMax, holdTime / config.maxHoldTime);
        return Mathf.Clamp(force, config.throwForceMin, config.throwForceMax);
    }

    private bool aim = false;
    /// <summary>
    /// Thay đổi góc nhìn của camera khi nhấn nút Aim.
    /// </summary>
    /// <param name="Input_M"></param>
    public void Aim(bool Input_M)
    {
        if (_playerInput._characterCamera == null)
        {
            Debug.LogWarning("CharacterCamera is null in Aim!");
            return;
        }

        // Lưu trạng thái camera hiện tại nếu chưa lưu
        if (!isCameraSettingsSaved)
        {
            savedDistance = _playerInput._characterCamera.maxDistance;
            savedHeight = _playerInput._characterCamera.height;
            isCameraSettingsSaved = true;
            //Debug.Log($"Aim: Saved camera state - Distance={savedDistance}, Height={savedHeight}");
        }

        if (Input_M)
        {

            _playerInput._characterCamera.transform.SetParent(transform);
            _playerInput._characterCamera.SetTargetValues(config.targetMaxDistance, config.targetHeight, config.rightOffset, config.isAiming);
            _playerInput._characterCamera.useInterpolation = false;
            //Debug.Log("Aim: Entered aiming mode");
            aim = true;
        }
        else
        {
            _playerInput._characterCamera.transform.SetParent(null);
            _playerInput._characterCamera.useInterpolation = true;
            _playerInput._characterCamera.SetTargetValues(savedDistance, savedHeight, _playerInput._characterCamera.rightOffset, false);
            //Debug.Log($"Aim: Exited aiming mode, Restored - Distance={savedDistance}, Height={savedHeight}");
            isCameraSettingsSaved = false;
            aim = false;
        }
    }

    /// <summary>
    /// Thay đổi tài nguyên giữa các slot trong ListBody.
    /// </summary>
    /// <param name="SLotBody"></param>
    public void ChangeResource(int SLotBody)
    {
        if (ListBody[SLotBody].childCount > 0)
        {
            if (ListBody[SLotBody].childCount > 0 && ListBody[1].childCount > 0)
            {
                Transform childFromSlot3 = ListBody[SLotBody].GetChild(0);
                Transform childFromSlot1 = ListBody[1].GetChild(0);

                childFromSlot3.SetParent(ListBody[1], false);
                childFromSlot1.SetParent(ListBody[SLotBody], false);

                Debug.Log("Successfully swapped first children between ListBody[3] and ListBody[1]");
            }
            else if (ListBody[SLotBody].childCount > 0)
            {
                Transform childFromSlot3 = ListBody[SLotBody].GetChild(0);
                childFromSlot3.SetParent(ListBody[1], false);
                Debug.Log("Moved child from ListBody[3] to ListBody[1]");
            }
            else if (ListBody[1].childCount > 0)
            {
                Transform childFromSlot1 = ListBody[1].GetChild(0);
                childFromSlot1.SetParent(ListBody[SLotBody], false);
                Debug.Log("Moved child from ListBody[1] to ListBody[3]");
            }
            else
            {
                Debug.LogWarning("No children to swap in ListBody[3] or ListBody[1]!");
            }
        }
    }

    /// <summary>
    /// lấy GameObject từ list Slot và thay đổi chỉ mục của nó vào ListBody[2] 
    /// </summary>
    /// <param name="Index"></param>
    /// <returns></returns>
    /// <summary>
    /// lấy GameObject từ list Slot và thay đổi chỉ mục của nó vào ListBody[2] 
    /// </summary>
    /// <param name="Index"></param>
    /// <returns></returns>
    public GameObject ChangeSlotIndex(int index)
    {
        if (ListSlot == null || ListSlot.transform.childCount == 0)
        {
            Debug.LogWarning("[PlayerController] ListSlot is null or empty.");
            return null;
        }

        if (index < 0 || index >= ListSlot.transform.childCount)
        {
            Debug.LogWarning($"[PlayerController] Index {index} is out of bounds.");
            return null;
        }

        // Trả item cũ về slot nếu có
        if (previousIndex >= 0 && RightHandObject != null)
        {
            RightHandObject.transform.SetParent(ListSlot.transform, false);
            //RightHandObject.SetActive(false);
        }

        // Lấy item mới từ index
        Transform itemTransform = ListSlot.transform.GetChild(index);
        GameObject newItem = itemTransform.gameObject;

        newItem.transform.SetParent(ListBody[1].transform, false);
        newItem.transform.localPosition = new Vector3(1f, 2f, 0f);
        newItem.transform.localRotation = Quaternion.identity;
        newItem.SetActive(true);

        RightHandObject = newItem;
        previousIndex = index;

        if (deactivateCoroutine != null)
            StopCoroutine(deactivateCoroutine);
        deactivateCoroutine = StartCoroutine(DeactivateAfterDelay(newItem, 0.6f));

        Debug.Log($"[PlayerController] Equipped item '{newItem.name}' from ListSlot[{index}]");
        return newItem;
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
    #endregion
    
    #region Lộc thêm để giảm tốc độ di chuyển của player
    /// <summary>
    /// Giảm tốc độ di chuyển của player (dùng cho fear zone effect)
    /// </summary>
    /// <param name="speedMultiplier">Hệ số nhân tốc độ (0.5f = giảm 50%)</param>
    public void ReduceMovementSpeed(float speedMultiplier = 0.5f)
    {
        if (!isSpeedReduced)
        {
            originalSpeed = N_speed;
            isSpeedReduced = true;
        }
        N_speed = originalSpeed * speedMultiplier;
        Debug.Log($"[PlayerController] Speed reduced to {N_speed} (multiplier: {speedMultiplier})");
    }

    /// <summary>
    /// Khôi phục tốc độ di chuyển ban đầu của player
    /// </summary>
    public void RestoreMovementSpeed()
    {
        if (!isSpeedReduced) return;
        N_speed = originalSpeed;
        isSpeedReduced = false;
        Debug.Log($"[PlayerController] Speed restored to {N_speed}");
    }
    #endregion
}

