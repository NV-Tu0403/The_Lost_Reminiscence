using Duckle;
using UnityEngine;

namespace DuckLe
{
    public class CharacterInput : MonoBehaviour
    {
        private PlayerController _pc;
        public CharacterCamera _characterCamera;

        public bool isInputLocked = false; // Lộc thêm để kiểm soát input

        private float _lastSpacePressTime;
        private int _spacePressCount;

        private void Start()
        {
            _pc = GetComponent<PlayerController>();
            if (_pc == null)
            {
                Debug.LogError("PlayerController không tìm thấy trong Start!");
            }

            InitializeCharacterCamera();
        }

        void Update()
        {
            // Lộc thêm để kiểm soát input

            //// Khi bị lock input, đảm bảo nhân vật về Idle
            //if (_pc != null && _pc._stateMachine != null)
            //{
            //    _pc._stateMachine.SetPrimaryState(new IdleState());
            //}
            if (_pc == null)
            {
                Debug.LogError("PlayerController is not assigned in CharacterInput.");
                return;
            }
            Vector3 dir = GetMoveInput();
            MoveType moveType = GetSpecialActionsInput();

            //GetAttackInput();
            GetUseResourceInput();
            InteractInput();

            _pc.PerformMoveInput(moveType, dir);
        }

        private void InitializeCharacterCamera()
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
            _characterCamera.SetTarget(transform);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Vector3 GetMoveInput()
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
            return dir;
        }

        private MoveType GetSpecialActionsInput()
        {
            float doubleTapTimeWindow = 0.5f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                float currentTime = Time.time;
                if (currentTime - _lastSpacePressTime <= doubleTapTimeWindow)
                {
                    _spacePressCount++;
                }
                else
                {
                    _spacePressCount = 1;
                }
                _lastSpacePressTime = currentTime;
            }

            MoveType moveType = MoveType.Walk; // Mặc định
            bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

            if (isShiftHeld)
            {
                moveType = MoveType.Sprint;
            }

            if (_spacePressCount >= 2 && Time.time - _lastSpacePressTime <= doubleTapTimeWindow)
            {
                moveType = MoveType.Dash;
                _spacePressCount = 0;
            }

            return moveType;
        }

        private void GetAttackInput()
        {
            if (Input.GetMouseButtonDown(0)) _pc.PerformMeleeInput(Duckle.MeleeType.Melee_01);
            if (Input.GetKeyDown(KeyCode.Q)) _pc.PerformThrowInput(Duckle.ThrowType.ThrowItem, 2f);

            if (Input.GetMouseButtonDown(1))
            {
                _pc.throwTimer.UpdateTimer(true);
                if (_characterCamera != null)
                {
                    _pc.Aim(true);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _pc.Aim(false);
                _pc.config.throwForce = _pc.CalculateThrowForce();
                _pc.PerformThrowInput(Duckle.ThrowType.ThrowWeapon, _pc.config.throwForce);
            }
        }

        private void GetUseResourceInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _pc.ChangeResource(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _pc.ChangeResource(3);
            }
        }

        public void InteractInput()
        {
            if (Input.GetKeyDown(KeyCode.E)) // nhặt
            {
                _pc.PerformInteractInput(InteractType.PickUp, _pc.CurrentSourcesLookAt);
            }
            if (Input.GetKeyDown(KeyCode.F)) // kích hoạt vật thể
            {

            }
        }
    }
}
//_____________________________________ ĐANG LỖI / CẦN CẢI TIẾN _____________________________________

//Logic double-tap trong GetSpecialActionsInput đơn giản nhưng có thể gây lỗi nếu người chơi nhấn phím Space nhiều lần liên tiếp (cần thêm logic giới hạn số lần nhấn).

//Thiếu kiểm tra null cho _characterCamera ở một số chỗ, có thể gây lỗi nếu camera không được khởi tạo đúng.

//Thay vì sử dụng Resources.Load (làm tăng thời gian tải và khó quản lý), sử dụng các tham chiếu trực tiếp trong Inspector hoặc một hệ thống quản lý prefab (ví dụ: Addressables).

// Tạo các interface hoặc lớp cơ sở cho các thành phần chính (như IInputHandler, IActionHandler) để dễ dàng thay thế hoặc mở rộng triển khai.