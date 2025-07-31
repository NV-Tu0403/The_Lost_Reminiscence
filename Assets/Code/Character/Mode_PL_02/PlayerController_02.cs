using Duckle;
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController_02 : PlayerEventListenerBase
{
    [SerializeField] private Core_02 _core_02;
    public PlayerConfig config;
    [SerializeField] private bool useNavMesh = false;
    public PlayerInput_02 _playerInput;
    public Rigidbody _rigidbody { get; private set; }
    public NavMeshAgent _navMeshAgent { get; private set; }
    [SerializeField] private Animator _animator; // set animator
    public IUsable CurrentUsable { get; set; }
    public Timer throwTimer = new Timer();

    public CharacterStateType CurrentPlayerState;

    #region Dictionary Type Action

    [Header("Weapon Settings")]
    [SerializeField] private List<Transform> ListBody; //   Danh sách các slot vũ khí
    [SerializeField] private GameObject _Object;

    [Header("Slot Settings")]
    public GameObject FacePlayer; // mặt người chơi (tạm thời)
    [SerializeField] private LayerMask lookAtLayerMask;
    public GameObject ListSlot; // Danh sách Slot (tạm thời)

    private string mess = null;

    private Vector3 lastRotationDirection = Vector3.forward;
    [SerializeField] private float N_speed = 32;
    [SerializeField] private float airDrag = 0.97f;
    [SerializeField] private float extraGravity = 20f;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float JumpMomentumBoost = 4f;
    [SerializeField] private float dashForce = 15f;   // lực dash
    [SerializeField] private float dashDuration = 0.3f; // thời gian dash
    [SerializeField] private float dashMomentumBoost = 10f; // nhân vận tốc ngang

    public int previousIndex = -1; // -1 = chưa có gì
    public GameObject currentEquippedItem;
    public Coroutine deactivateCoroutine;

    [SerializeField] private GameObject markerPrefab; // Prefab dùng để đánh dấu (test)
    [SerializeField] private Material laserMaterial; // Material cho laser line (test)
    [SerializeField] private float laserWidth = 0.02f; // Độ rộng của laser line (test)

    private GameObject currentMarker;
    private LineRenderer laserLine;

    private float savedDistance = 0f;
    private float savedHeight = 0f;
    private bool isCameraSettingsSaved = false;

    private float currentDirectionX = 0f; // Giá trị hiện tại của DirectionX
    private float currentDirectionZ = 0f; // Giá trị hiện tại của DirectionZ
    [SerializeField] private float smoothSpeed = 10f; // Tốc độ làm mượt (có thể chỉnh trong Inspector)

    [SerializeField] private float checkInterval = 0.1f;
    private float lastCheckTime;

    public GameObject CurrentSourcesLookAt { get; set; } // Đối tượng mà người chơi đang nhìn vào
    public Vector3 CurrentLookAtHitPoint { get; private set; } // tọa độ điểm va chạm của tia nhìn
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
        if (Input.GetKeyDown(KeyCode.N))
        {
            useNavMesh = !useNavMesh;
            if (_navMeshAgent != null)
            {
                _navMeshAgent.updatePosition = useNavMesh;
                _rigidbody.isKinematic = useNavMesh;
            }
            Debug.Log($"Switched to {(useNavMesh ? "NavMesh" : "Rigidbody")} movement.");
        }

        if (_core_02 == null)
        {
            Debug.LogError("Core_02 instance is null in PlayerController_02 Update.");
            return;
        }

        //_playerInput.isInputLocked = !isGrounded;

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

        // Set animation trigger cho Movement hoặc Idle
        if (_animator != null)
        {
            SetAnimationParameters(actionType, direction);
        }


        //if (actionType == CharacterActionType.Jump)
        //{
        //    Jump();
        //    return;
        //}

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
                break;
            case CharacterActionType.Dash:
                Dash();
                return;
        }

        if (isGrounded) // chỉ di chuyển khi đang trên mặt đất
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

        switch (actionType)
        {
            case CharacterActionType.Attack:
                Attack(direction, config.attackRange, config.attackDuration, config.attackDamage, config.attackCooldown);
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

    public void UsingResource()
    {
        if (ListBody == null)
        {
            Debug.LogError("ListBody is null! Please initialize it in the Inspector or code.");
            return;
        }

        if (ListBody.Count > 2 && ListBody[1] != null && ListBody[1].childCount > 0)
        {
            Transform childTransform = ListBody[1].transform.GetChild(0);
            if (childTransform != null)
            {
                GameObject childObject = childTransform.gameObject;
                _Object = childObject;
                Component component = null;

                Component[] components = _Object.GetComponents<Component>();
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
                    Debug.LogWarning($"Weapons component not found on {_Object.name}!");
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision?.gameObject.CompareTag("Ground") == true)
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
        if (collision == null) { return; }

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    #endregion

    #region Attack
    private IEnumerator ThrownCoroutine(float force, float delay)
    {

        _animator.SetBool("Throw", true);

        // Đợi một khoảng thời gian để animation chạy
        yield return new WaitForSeconds(delay);

        GameObject thrownObject = currentEquippedItem?.gameObject;
        if (thrownObject != null)
        {
            thrownObject.SetActive(true);
            currentEquippedItem.gameObject.transform.SetParent(null);
            currentEquippedItem = null;
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

    /// <summary>
    /// gọi hàm này để để đánh
    /// </summary>
    /// <param name="attackRange"></param>
    /// <param name="attackDuration"></param>
    /// <param name="attackDamage"></param>
    /// <param name="attackCooldown"></param>
    /// <returns></returns>
    private bool Attack(Vector3 attackDir, float attackRange, float attackDuration, float attackDamage, float attackCooldown)
    {
        //if (Time.time - lastAttackTime < attackCooldown)
        //{
        //    Debug.Log("Attack on cooldown.");
        //    return false;
        //}
        //lastAttackTime = Time.time;

        try
        {
            if (_animator == null)
            {
                Debug.LogWarning("Animator is null in Attack. No animation will be played.");
            }
            else
            {
                SetAnimationParameters(CharacterActionType.Attack, attackDir);
                Debug.Log($"Performing attack with range={attackRange}, duration={attackDuration}, damage={attackDamage}, cooldown={attackCooldown}");
            }

            _core_02._stateMachine.SetState(new AttackState(_core_02._stateMachine, _playerEvent));

            // Kiểm tra va chạm với đối tượng trong phạm vi tấn công
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * attackRange * 0.5f, attackRange);
            foreach (Collider hit in hits)
            {
                //if (hit.CompareTag("Enemy"))
                //{
                //    Health enemyHealth = hit.GetComponent<Health>();
                //    if (enemyHealth != null)
                //    {
                //        enemyHealth.TakeDamage(attackDamage);
                //        Debug.Log($"Hit enemy {hit.name} with {attackDamage} damage.");
                //    }
                //}
            }

            Debug.Log($"Attack performed: range={attackRange}, duration={attackDuration}, damage={attackDamage}, cooldown={attackCooldown}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Attack failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// hàm tính damage tạm thời (sẽ sửa lại)
    /// </summary>
    /// <returns></returns>
    public float TakeDamage(float attackDamage)
    {
        float outDamaged = 0f;

        return (outDamaged);
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
    #endregion

    //#region Gizmos
    //private void OnDrawGizmos()
    //{
    //    if (config != null)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(transform.position + transform.forward * config.attackRange * 0.5f, config.attackRange);
    //    }
    //}
    //#endregion

    #region Actions & logic Action

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
        if (previousIndex >= 0 && currentEquippedItem != null)
        {
            currentEquippedItem.transform.SetParent(ListSlot.transform, false);
            currentEquippedItem.SetActive(false);
        }

        // Lấy item mới từ index
        Transform itemTransform = ListSlot.transform.GetChild(index);
        GameObject newItem = itemTransform.gameObject;

        newItem.transform.SetParent(ListBody[1].transform, false);
        newItem.transform.localPosition = new Vector3(1f, 2f, 0f);
        newItem.transform.localRotation = Quaternion.identity;
        newItem.SetActive(true);

        currentEquippedItem = newItem;
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
}

