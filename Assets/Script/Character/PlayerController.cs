#define ENABLE_UNSAFE_CODE

using Duckle;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using UnityEngine;

namespace DuckLe
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CCData_M3
    {
        [FieldOffset(0)] public Vector3 position;
        [FieldOffset(12)] public Vector3 velocity;
        [FieldOffset(24)] public Quaternion rotation;
        [FieldOffset(48)] public int collisionFlags;
        [FieldOffset(96)] public void* extraData;

        public CCData_M3(Vector3 pos, Quaternion rot, float moveSpd, float rotSpd, float jumpH)
        {
            position = pos;
            velocity = Vector3.zero;
            rotation = rot;
            collisionFlags = 0;
            extraData = null;
        }
    }

    public class PlayerController : MonoBehaviour
    {

        [SerializeField] public PlayerConfig config; // Tham chiếu đến PlayerConfig
        private CCData_M3 _data;
        private ref CCData_M3 Data => ref _data;
        public Rigidbody _rigidbody { get; private set; }
        private PlayerManager _playerManager;
        private PlayerAnimator _animator;
        public CharacterInput _playerInput;
        public Timer throwTimer = new Timer();

        private Dictionary<MeleeType, MeleeAction> meleeActions;
        private Dictionary<ThrowType, ThrowAction> throwActions;
        private Dictionary<MoveType, MoveAction> moveActions;
        private Dictionary<InteractType, InteractAction> interactAction;

        [Header("Weapon Settings")]
        [SerializeField] private List<Transform> ListBody; //   Danh sách các slot vũ khí
        [SerializeField] private GameObject _Object;

        [Header("Slot Settings")]
        [SerializeField] private GameObject ListSlot; // Danh sách Slot (tạm thời)

        [Header("Resource Settings")]
        public CharacterStateMachine _stateMachine; // Changed from private to public

        /// <summary>
        /// curentUsable: lưu trữ đối tượng hiện tại mà người chơi đang sử dụng.
        /// </summary>
        public IUsable CurrentUsable { get; set; }
        public GameObject CurrentSourcesLookAt { get; set; } // Đối tượng mà người chơi đang nhìn vào

        private MoveType _currentMoveType = MoveType.Walk; // Lưu MoveType hiện tại
        private float _meleeActionEndTime;
        private float _throwActionEndTime;

        private float savedDistance = 0f;
        private float savedHeight = 0f;
        private bool isCameraSettingsSaved = false;

        private float checkInterval = 0.1f;
        private float lastCheckTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("Rigidbody is not assigned in PlayerController.");
                return;
            }

            _playerInput = GetComponent<CharacterInput>();
            if (_playerInput == null)
            {
                Debug.LogError("CharacterInput not found on the same GameObject!");
            }

            if (config == null)
            {
                Debug.LogError("PlayerConfig is not assigned in PlayerController!");
                return;
            }

            _data = new CCData_M3(transform.position, transform.rotation, config.walkSpeed, config.rotationSpeed, config.jumpImpulse);
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _stateMachine = new CharacterStateMachine(this);
            DictionaryType();
        }

        private void Update()
        {
            UpdateAction();
            UsingResource();
            CheckItemByLooking();

            //PickUp();
        }

        #region status
        public Vector3 Velocity
        {
            get => Core.Instance.IsOffline ? Data.velocity : _data.velocity;
            set
            {
                if (Core.Instance.IsOffline) Data.velocity = value;
                else _data.velocity = value;
            }
        }

        public bool IsAction
        {
            get => _stateMachine.HasState(new MovingState()) ||
                    _stateMachine.HasState(new JumpingState()) ||
                    _stateMachine.HasState(new CrouchingState()) ||
                    _stateMachine.HasState(new DashingState(0f)) || // Provide a default duration
                    _stateMachine.HasState(new MeleeAttackingState(0f, MeleeType.Melee_01)) ||
                    _stateMachine.HasState(new ThrowingState(0f)) ||
                    _stateMachine.HasState(new InteractingState(0.5f, InteractType.PickUp)); // Provide default duration and type
            set
            {
                if (value)
                {
                    _stateMachine.SetPrimaryState(new MovingState());
                }
                else
                {
                    _stateMachine.SetPrimaryState(new IdleState());
                }
            }
        }


        public void UpdateAction()
        {
            _stateMachine.Update();

            if (meleeActions != null)
            {
                foreach (var action in meleeActions.Values)
                {
                    action.update();
                }
            }
            if (throwActions != null)
            {
                foreach (var action in throwActions.Values)
                {
                    action.update();
                }
            }
            if (moveActions != null)
            {
                foreach (var action in moveActions.Values)
                {
                    action.update();
                }
            }

            // Kiểm tra trạng thái rơi
            if (_rigidbody != null && _rigidbody.linearVelocity.y < -0.1f && (_data.collisionFlags & 1) == 0)
            {
                _stateMachine.AddSecondaryState(new FallingState());
            }
            else
            {
                _stateMachine.RemoveSecondaryState(new FallingState());
            }

#if UNITY_EDITOR
            Debug.Log($"Trạng thái: {string.Join(", ", _stateMachine.GetAllStateNames())}");
#endif
        }

        public T ReinterpretState<T>() where T : struct
        {
            return System.Runtime.InteropServices.Marshal.PtrToStructure<T>((System.IntPtr)System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(new[] { _data }, 0));
        }

        public AnimationCCData GetAnimationData()
        {
            float inputMagnitude = _playerInput != null ? GetInputMagnitude() : 0f;

            float speedMultiplier = _currentMoveType switch
            {
                MoveType.Walk => 0.5f,
                MoveType.Run => 1f,
                MoveType.Sprint => 1.5f,
                MoveType.Dash => 3.0f,
                _ => 1.0f
            };
            inputMagnitude *= speedMultiplier;

            AnimationCCData animData = new AnimationCCData
            {
                IsStrafing = _playerInput?._characterCamera?.isAiming ?? false,
                Grounded = (_data.collisionFlags & 1) != 0,
                GroundDistance = (_data.collisionFlags & 1) != 0 ? 0f : Mathf.Abs(_rigidbody.position.y - transform.position.y),
                HorizontalSpeed = new Vector2(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.z).magnitude,
                VerticalSpeed = _rigidbody.linearVelocity.y,
                InputMagnitude = inputMagnitude,
                IsMeleeAttacking = _stateMachine.HasState(new MeleeAttackingState(0, MeleeType.Melee_01)),
                IsThrowing = _stateMachine.HasState(new ThrowingState(0))
            };
            return animData;
        }

        private float GetInputMagnitude()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 input = new Vector2(h, v);
            return input.sqrMagnitude > 0 ? input.normalized.magnitude : 0f; // Chuẩn hóa để tối đa là 1
        }
        #endregion

        #region Dictionary Type Action
        public void DictionaryType()
        {
            meleeActions = new Dictionary<MeleeType, MeleeAction>
            {
                { MeleeType.Melee_01, new MeleeAction(this, config.attackCooldown, config.attackRange, config.attackDuration, MeleeType.Melee_01) },
                { MeleeType.Melee_02, new MeleeAction(this, config.attackCooldown, config.attackRange, config.attackDuration, MeleeType.Melee_02) },
                { MeleeType.Melee_03, new MeleeAction(this, config.attackCooldown, config.attackRange, config.attackDuration, MeleeType.Melee_03) },
                { MeleeType.Melee_04, new MeleeAction(this, config.attackCooldown, config.attackRange, config.attackDuration, MeleeType.Melee_04) }
            };

            throwActions = new Dictionary<ThrowType, ThrowAction>
            {
                { ThrowType.ThrowWeapon, new ThrowAction(this, config.throwCooldown, config.prefabPath, 100f, ThrowType.ThrowWeapon) },
                { ThrowType.ThrowItem, new ThrowAction(this, config.throwCooldown, config.prefabPath, 10f, ThrowType.ThrowItem) }
            };

            moveActions = new Dictionary<MoveType, MoveAction>
            {
                { MoveType.Walk, new MoveAction(this, 0f, config.walkSpeed, config.acceleration, config.rotationSpeed, MoveType.Walk) },
                { MoveType.Run, new MoveAction(this, 0f, config.runSpeed, config.acceleration, config.rotationSpeed, MoveType.Run) },
                { MoveType.Sprint, new MoveAction(this, 0f, config.sprintSpeed, config.acceleration, config.rotationSpeed, MoveType.Sprint) },
                { MoveType.Dash, new MoveAction(this, config.dashCooldown, config.dashSpeed, config.acceleration * 2f, config.rotationSpeed, MoveType.Dash) }
            };
            interactAction = new Dictionary<InteractType, InteractAction>
            {
                { InteractType.PickUp, new InteractAction(this, 0f, InteractType.PickUp) },
                { InteractType.Drop, new InteractAction(this, 0f, InteractType.Drop) },
                { InteractType.Active, new InteractAction(this, 0f, InteractType.Active) },
                { InteractType.Interact_04, new InteractAction(this, 0f, InteractType.Interact_04) }
            };
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
                Debug.LogWarning("ListBody[1] is null or has no children!");
            }
        }
        #endregion

        #region Movement
        public void Teleport(Vector3? position = null, Quaternion? rotation = null)
        {
            _rigidbody.position = position ?? transform.position;
            _rigidbody.rotation = rotation ?? transform.rotation;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        public void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
        {
            if (Data.collisionFlags == 1 || ignoreGrounded)
            {
                float impulse = overrideImpulse ?? config.jumpImpulse;
                _rigidbody.AddForce(Vector3.up * impulse, ForceMode.Impulse);
            }
        }
        #endregion

        #region Input Action
        public void PerformMoveInput(MoveType moveType, Vector3 direction)
        {
            if (moveActions == null || !moveActions.ContainsKey(moveType))
            {
                Debug.LogError($"moveActions is null or MoveType {moveType} not found in moveActions.");
                return;
            }
            _currentMoveType = moveType;
            moveActions[moveType].Perform(direction);
        }

        public void PerformMeleeInput(MeleeType meleeType)
        {
            meleeActions[meleeType].Perform(CurrentUsable);
            _stateMachine.AddSecondaryState(new MeleeAttackingState(config.attackDuration, meleeType));
        }

        public void PerformThrowInput(ThrowType throwType, float force)
        {
            throwActions[throwType].force = force;
            throwActions[throwType].Perform(CurrentUsable);
            _stateMachine.AddSecondaryState(new ThrowingState((config.throwCooldown)));
        }

        public void PerformInteractInput(InteractType interactType, GameObject currentSources)
        {
            interactAction[interactType].Perform();
            _stateMachine.AddSecondaryState(new InteractingState(0.5f, interactType));
        }

        #endregion

        #region Actions & logic Action

        public void RPC_RequestPerformMelee(MeleeType meleeType, float effectValue, string usableName = "Default")
        {
            Vector3 attackDirection = meleeType switch
            {
                MeleeType.Melee_01 => transform.forward,
                MeleeType.Melee_02 => -transform.forward,
                MeleeType.Melee_03 => transform.right,
                MeleeType.Melee_04 => -transform.right,
                _ => transform.forward
            };
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + attackDirection * config.attackRange / 2f, config.attackRange);
            foreach (var hit in hitColliders)
            {
                if (hit.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddForce(attackDirection * CurrentUsable.GetEffectValue());
                    Debug.Log($"Hit {hit.name} with {meleeType} damage: {CurrentUsable.GetEffectValue()} (Rigidbody)");
                }
            }
            _stateMachine.AddSecondaryState(new MeleeAttackingState(config.attackDuration, meleeType)); _meleeActionEndTime = Time.time + config.attackDuration;
            IsAction = true;
        }

        public void RPC_RequestPerformThrow(Vector3 position, Vector3 forward, ThrowType throwType, float force, string usableName = "Default", float effectValue = 0f)
        {
            GameObject prefab = Resources.Load<GameObject>(config.prefabPath);
            if (prefab == null) return;

            Vector3 spawnPosition = position + transform.forward + transform.right * 0.5f + Vector3.up * 1.5f;
            GameObject networkObject = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (networkObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = forward * force;
            }
            if (networkObject.TryGetComponent<ThrowableObject>(out var throwable))
            {
                throwable.SetThrower(this);
                Debug.Log($"Spawned ThrowableObject at {spawnPosition} with force: {force}");
            }
            _stateMachine.AddSecondaryState(new ThrowingState(config.throwCooldown));
            _throwActionEndTime = Time.time + (config.throwCooldown);
            IsAction = true;
        }

        /// <summary>
        /// trả về đối tượng mà người chơi đang nhìn vào.
        /// </summary>
        public void CheckItemByLooking()
        {
            if (Time.time - lastCheckTime < checkInterval) return;
            lastCheckTime = Time.time;
            //Ray ray = new Ray(transform.position + Vector3.up * 1.5f, Camera.main.ScreenPointToRay(Input.mousePosition).direction);\
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Debug.DrawRay(ray.origin, ray.direction * 15f, Color.red, 0.1f, true);
            if (Physics.Raycast(ray, out RaycastHit hit, 15f))
            {
                CurrentSourcesLookAt = hit.collider.gameObject;
            }
        }

        ///// <summary>
        ///// Cập nhật vị trí con trỏ chuột trong game.
        ///// </summary>
        ///// <param name="position"></param>
        //public void MouseCursorPosition(Vector3 position)
        //{
        //    // Ẩn con trỏ chuột của Windows
        //    Cursor.visible = false;
        //    Cursor.lockState = CursorLockMode.Confined; // Giữ chuột trong cửa sổ game

        //    GameObject customCursor = GameObject.Find("CursorMouse");
        //    if (customCursor == null)
        //    {
        //        customCursor = new GameObject("CursorMouse");
        //        // tải một gameObject từ prefab
        //        GameObject prefab = Resources.Load<GameObject>("Prefabs/CursorMouse");
        //    }
        //    //if (customCursor == null)
        //    //{
        //    //    // Tạo Canvas
        //    //    GameObject canvasObj = new GameObject("CursorCanvas");
        //    //    Canvas canvas = canvasObj.AddComponent<Canvas>();
        //    //    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        //    //    // Tạo UI Image cho con trỏ
        //    //    customCursor = new GameObject("CustomCursor");
        //    //    customCursor.transform.SetParent(canvasObj.transform);
        //    //    Image cursorImage = customCursor.AddComponent<Image>();
        //    //    cursorImage.sprite = Resources.Load<Sprite>("CursorSprite");

        //    //    // Đặt kích thước con trỏ
        //    //    cursorImage.rectTransform.sizeDelta = new Vector2(32, 32); // Kích thước con trỏ
        //    //}

        //    // Lấy tâm màn hình
        //    Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        //    // Chuyển tọa độ màn hình sang tọa độ thế giới (nếu dùng SpriteRenderer)
        //    Camera mainCamera = Camera.main;
        //    Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenCenter);
        //    worldPosition.z = 0;

        //    // Đặt vị trí con trỏ
        //    customCursor.transform.position = worldPosition;
        //}

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

        public void ZoomCamaraThrow(bool Input_M)
        {
            if (Input_M)
            {
                Vector3 rightOffset = transform.right * 10f;
                Vector3 targetPosition = transform.position + rightOffset + Vector3.up * _playerInput._characterCamera.height;

                if (_playerInput._characterCamera.mainCamera.orthographic)
                {
                    _playerInput._characterCamera.mainCamera.orthographicSize /= 8f;
                }
                else
                {
                    _playerInput._characterCamera.mainCamera.fieldOfView /= 8f;
                }

                _playerInput._characterCamera.mainCamera.transform.position = Vector3.Lerp(
                    _playerInput._characterCamera.mainCamera.transform.position,
                    targetPosition,
                    Time.deltaTime * 2f
                );
            }
            else
            {
                if (_playerInput._characterCamera.mainCamera.orthographic)
                {
                    _playerInput._characterCamera.mainCamera.orthographicSize *= 8f;
                }
                else
                {
                    _playerInput._characterCamera.mainCamera.fieldOfView *= 8f;
                }

                _playerInput._characterCamera.mainCamera.transform.position = _playerInput._characterCamera.transform.position;
            }
        }

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
                Debug.Log($"Aim: Saved camera state - Distance={savedDistance}, Height={savedHeight}");
            }

            if (Input_M)
            {

                _playerInput._characterCamera.transform.SetParent(transform);
                _playerInput._characterCamera.SetTargetValues(1f, 1.7f, 0.7f, true);
                //_playerInput._characterCamera.useInterpolation = false;
                Debug.Log("Aim: Entered aiming mode");
            }
            else
            {
                _playerInput._characterCamera.transform.SetParent(null);
                _playerInput._characterCamera.useInterpolation = true;
                _playerInput._characterCamera.SetTargetValues(savedDistance, savedHeight, 0f, false);
                Debug.Log($"Aim: Exited aiming mode, Restored - Distance={savedDistance}, Height={savedHeight}");
                isCameraSettingsSaved = false;
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

        #endregion
    }
}

//_____________________________________ ĐANG LỖI / CẦN CẢI TIẾN _____________________________________

//Quá nhiều trách nhiệm (di chuyển, tấn công, ném, tương tác, quản lý trạng thái), làm tăng độ phức tạp và khó bảo trì.
//Tạo các lớp riêng cho các chức năng cụ thể:
//CameraController: Quản lý camera(Aim, ZoomCamaraThrow).
//InventoryManager: Quản lý tài nguyên và slot (ListBody, ListSlot).
//ActionManager: Quản lý các hành động (meleeActions, throwActions, v.v.).
//PlayerController chỉ nên giữ vai trò điều phối cấp cao và lưu trữ trạng thái cơ bản.

//Một số thuộc tính được khai báo public (_stateMachine, _rigidbody) có thể gây rủi ro nếu bị truy cập không kiểm soát từ bên ngoài.

//Logic kiểm tra đối tượng nhìn vào (CheckItemByLooking) sử dụng raycast mỗi frame, có thể gây tốn hiệu suất nếu số lượng vật thể lớn.

//Sử dụng ScriptableObject để lưu trữ các thông số cố định (walkSpeed, attackRange, throwForceMax, v.v.) thay vì hard-code trong PlayerController

// PlayerController.UpdateAction gọi _stateMachine.Update và lặp qua tất cả các hành động (meleeActions, throwActions, moveActions) mỗi frame, ngay cả khi không có hành động nào được thực hiện. Điều này gây lãng phí tài nguyên.

//Giải pháp: Chỉ cập nhật các hành động hoặc trạng thái khi cần thiết (ví dụ: khi có đầu vào mới hoặc trạng thái thay đổi).

// Cache dữ liệu: Lưu trữ các giá trị thường dùng (như Camera.main, transform.forward) vào biến tạm để tránh truy cập lặp lại.

// Dữ liệu phân tán: Một số dữ liệu (như thông số camera, lực ném) được lưu trong PlayerController, trong khi các tham chiếu như _characterCamera lại nằm trong CharacterInput. Điều này gây khó khăn khi cần truy cập hoặc sửa đổi dữ liệu.

// Phụ thuộc cứng vào cấu trúc scene: Các tham chiếu như Resources.Load (trong CharacterInput và ThrowAction) hoặc ListBody yêu cầu cấu trúc scene cụ thể, làm giảm tính di động của code.