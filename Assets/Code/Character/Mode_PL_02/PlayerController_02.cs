using Duckle;
using System;
using UnityEngine;
using UnityEngine.AI;

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

    public CharacterStateType CurrentPlayerState;

    #region Dictionary Type Action
    private string mess = null;

    private Vector3 lastRotationDirection = Vector3.forward;
    [SerializeField] private float N_speed = 32;
    [SerializeField] private float airDrag = 0.97f;
    [SerializeField] private float extraGravity = 20f;
    [SerializeField] private bool isGrounded = true;
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

        // cập nhật _playerInput.isInputLocked theo !isGrounded
        _playerInput.isInputLocked = !isGrounded;
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

        float moveSpeed = config.walkSpeed; // mặc định là walk
        // Chọn tốc độ dựa trên state
        switch (actionType)
        {
            case CharacterActionType.Run:
                moveSpeed = config.runSpeed;
                break;
            case CharacterActionType.Sprint:
                moveSpeed = config.sprintSpeed;
                break;
            case CharacterActionType.Dash:
                moveSpeed = config.dashSpeed;
                break;
            case CharacterActionType.Jump:
                Jump(config.jumpImpulse);
                break;
        }

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

    public void PerformAttackInput(CharacterActionType actionType, Vector3 direction)
    {
        _core_02._stateMachine.HandleAction(actionType);
        if (actionType == CharacterActionType.Attack)
        {
            Attack(config.attackRange, config.attackDuration, config.attackDamage, config.attackCooldown);
        }
    }

    public void PerformSpecialInput(CharacterActionType actionType, Vector3 direction)
    {
        _core_02._stateMachine.HandleAction(actionType);

        switch (actionType)
        {
            case CharacterActionType.Jump:
                Jump(config.jumpImpulse);
                break;
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

        // Cập nhật tốc độ của NavMeshAgent
        _navMeshAgent.speed = moveSpeed * speed_n;
        _navMeshAgent.angularSpeed = config.rotationSpeed * speed_n;
        _navMeshAgent.acceleration = config.acceleration * speed_n;

        // Tính toán điểm đích dựa trên hướng di chuyển
        Vector3 targetPosition = transform.position + direction * moveSpeed/* * Time.deltaTime*/;

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
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, config.rotationSpeed /** Time.deltaTime*/);
        }
    }

    #endregion

    #region Jump
    private bool Jump(float jumpForce)
    {
        if (!isGrounded) return false;

        if (_animator != null) _animator.SetTrigger("Jump");
        if (useNavMesh && _navMeshAgent != null)
        {
            _navMeshAgent.enabled = false;
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Vector3 horizontalVel = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);

        if (horizontalVel.sqrMagnitude < 0.01f && _playerInput != null)
            horizontalVel = _playerInput.GetMoveInput().normalized * config.runSpeed;

        float momentumBoost = 15f; // boost
        Vector3 jumpVelocity = (horizontalVel * momentumBoost) + Vector3.up * jumpForce;

        _rigidbody.linearVelocity = jumpVelocity;

        isGrounded = false;
        _core_02._stateMachine.SetState(new JumpState(_core_02._stateMachine, _playerEvent));
        return true;
    }

    private void EnableNavMeshAgent()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
            _rigidbody.interpolation = RigidbodyInterpolation.None;

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

    /// <summary>
    /// gọi hàm này để để đánh
    /// </summary>
    /// <param name="attackRange"></param>
    /// <param name="attackDuration"></param>
    /// <param name="attackDamage"></param>
    /// <param name="attackCooldown"></param>
    /// <returns></returns>
    private bool Attack(float attackRange, float attackDuration, float attackDamage, float attackCooldown)
    {
        try
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }

            // Kiểm tra va chạm với đối tượng trong phạm vi tấn công
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * config.attackRange * 0.5f, config.attackRange);
            foreach (Collider hit in hits)
            {
                // Giả sử đối tượng có thể bị tấn công có tag "Enemy" và component Health
                if (hit.CompareTag("Enemy"))
                {
                    TakeDamage(config.attackDamage);
                    Debug.Log($"Hit enemy {hit.name} with {config.attackDamage} damage.");
                }
            }

            return true;
        }
        catch (Exception e)
        {
            mess = e.Message;
            return false;
        }
        finally
        {
            _animator = null;
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
}
