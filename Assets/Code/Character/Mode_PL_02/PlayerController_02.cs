using Duckle;
using UnityEngine;

public class PlayerController_02 : PlayerEventListenerBase
{
    [SerializeField] private Core_02 _core_02;
    public PlayerConfig config;
    public PlayerInput_02 _playerInput;
    public Rigidbody _rigidbody { get; private set; }
    public IUsable CurrentUsable { get; set; }

    public string CurrentPlayerState = null;

    #region Dictionary Type Action
    private Vector3 lastRotationDirection = Vector3.forward;

    #endregion

    protected override void Awake()
    {
        if (_core_02 == null) _core_02 = Core_02.Instance;
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();

        base.Awake();
        if (_core_02 == null)
        {
            Debug.LogError("Core_02 instance is null in PlayerController_02.");
            _core_02 = Core_02.Instance;
        }
        _core_02._stateMachine.SetState(new IdleState_1(_core_02._stateMachine, _playerEvent));

    }

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput_02>();
    }

    public void Update()
    {
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
        CurrentPlayerState = stateType.ToString();
        //Debug.Log($"[Core] CurrentCoreState updated to: {CurrentCoreState}");
    }

    #region get input
    public void PerformMoveInput(CharacterActionType actionType, Vector3 direction)
    {
        //Debug.LogWarning($"PerformMoveInput: _core = {_core_02}, _stateMachine = {_core_02?._stateMachine}");

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

        // Chuyển state trước
        _core_02._stateMachine.HandleAction(actionType);

        // Chọn tốc độ dựa trên state
        float moveSpeed = config.walkSpeed;
        switch (actionType)
        {
            case CharacterActionType.Run: moveSpeed = config.runSpeed; break;
            case CharacterActionType.Sprint: moveSpeed = config.sprintSpeed; break;
            case CharacterActionType.Dash: moveSpeed = config.dashSpeed; break;
        }

        // Gọi hàm Move
        Move(direction * moveSpeed, actionType);
    }

    #endregion

    #region Move

    private void Move(Vector3 direction, CharacterActionType moveType)
    {
        float acceleration = config.acceleration;
        float rotationSpeed = config.rotationSpeed;

        if (this._rigidbody == null)
        {
            Debug.LogError("Rigidbody is null in MoveAction.");
            return;
        }

        if (direction.sqrMagnitude <= 0.001f)
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

    #endregion

    #region
    #endregion

    #region
    #endregion


}
