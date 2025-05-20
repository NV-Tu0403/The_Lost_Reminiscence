using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DuckLe;

namespace Duckle
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho các hành động của nhân vật.
    /// </summary>
    public abstract class CharacterAction
    {
        protected PlayerController controller;
        protected float cooldown;
        protected float lastActionTime;

        protected CharacterAction(PlayerController controller, float cooldown)
        {
            this.controller = controller;
            this.cooldown = cooldown;
        }

        /// <summary>
        /// Thực hiện hành động của nhân vật.
        /// </summary>
        public void Perform(IUsable usable = null)
        {
            if (!CanPerform()) return;
            lastActionTime = Time.time;

            if (Core.Instance.IsOffline)
            {
                PerformOffline(usable);
            }
            else
            {
                PerformOnline(usable);
            }
        }

        /// <summary>
        /// Kiểm tra xem hành động có thể thực hiện được hay không.
        /// </summary>
        protected virtual bool CanPerform()
        {
            return Time.time - lastActionTime >= cooldown && !controller.IsAction;
        }

        /// <summary>
        /// Cập nhật trạng thái của hành động.
        /// </summary>
        public virtual void update() { }

        /// <summary>
        /// Thực hiện hành động trong chế độ offline.
        /// </summary>
        protected abstract void PerformOffline(IUsable usable = null);

        /// <summary>
        /// Thực hiện hành động trong chế độ online.
        /// </summary>
        protected abstract void PerformOnline(IUsable usable = null);
    }

    /// <summary>
    /// Hành động di chuyển của nhân vật.
    /// </summary>
    public class MoveAction : CharacterAction
    {
        private float speed;
        private float acceleration;
        private float rotationSpeed;
        private Vector3 lastRotationDirection;
        private MoveType moveType;

        public MoveAction(PlayerController controller, float cooldown, float speed, float acceleration, float rotationSpeed, MoveType moveType)
            : base(controller, cooldown)
        {
            this.speed = speed;
            this.acceleration = acceleration;
            this.rotationSpeed = rotationSpeed;
            this.moveType = moveType;
            this.lastRotationDirection = controller.transform.forward;
        }

        protected override bool CanPerform()
        {
            return Time.time - lastActionTime >= cooldown;
        }

        public void Perform(Vector3 direction)
        {
            if (!CanPerform()) return;
            lastActionTime = Time.time;

            if (Core.Instance.IsOffline)
            {
                PerformOffline(direction);
            }
            else
            {
                PerformOnline(direction);
            }
        }

        protected override void PerformOffline(IUsable usable = null)
        {
            Debug.LogError("PerformOffline(IUsable) không được dùng cho MoveAction. Sử dụng Perform(Vector3) thay thế.");
        }

        protected override void PerformOnline(IUsable usable = null)
        {
            Debug.LogError("PerformOnline(IUsable) không được dùng cho MoveAction. Sử dụng Perform(Vector3) thay thế.");
        }

        private void PerformOffline(Vector3 direction)
        {
            if (controller._rigidbody == null)
            {
                Debug.LogError("Rigidbody is null in MoveAction.");
                return;
            }

            // Cập nhật trạng thái
            if (direction.sqrMagnitude > 0.001f)
            {
                if (moveType == MoveType.Dash)
                {
                    controller._stateMachine.SetPrimaryState(new DashingState(0.3f));
                }
                else
                {
                    controller._stateMachine.SetPrimaryState(new MovingState());
                }
            }
            else
            {
                controller._stateMachine.SetPrimaryState(new IdleState());
            }

            float deltaTime = Time.fixedDeltaTime;
            Vector3 currentVelocity = controller._rigidbody.linearVelocity;

            Vector3 targetVelocity = direction.normalized * speed;
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            Vector3 velocityChange = targetVelocity - horizontalVelocity;

            velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration * deltaTime);
            controller._rigidbody.linearVelocity += new Vector3(velocityChange.x, 0f, velocityChange.z);

            Quaternion currentRotation = controller._rigidbody.rotation;

            if (controller._playerInput == null)
            {
                Debug.LogError("_playerInput is null in MoveAction.");
                return;
            }

            if (controller._playerInput._characterCamera == null)
            {
                Debug.LogWarning("_characterCamera is null in MoveAction. Using default rotation.");
                if (direction.sqrMagnitude > 0.001f)
                {
                    lastRotationDirection = direction;
                }

                if (lastRotationDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
                    controller._rigidbody.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * deltaTime));
                }
                return;
            }

            if (!controller._playerInput._characterCamera.isAiming)
            {
                if (direction.sqrMagnitude > 0.001f)
                {
                    lastRotationDirection = direction;
                }

                if (lastRotationDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
                    controller._rigidbody.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * deltaTime));
                }
            }
            else if (controller._playerInput._characterCamera.mainCamera != null)
            {
                Vector3 camForward = controller._playerInput._characterCamera.mainCamera.transform.forward;
                camForward.y = 0;

                if (camForward.sqrMagnitude > 0.001f)
                {
                    lastRotationDirection = camForward.normalized;
                }

                if (lastRotationDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lastRotationDirection);
                    controller._rigidbody.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * deltaTime));
                }
            }
        }

        private void PerformOnline(Vector3 direction)
        {
            PerformOffline(direction); // TODO: Thêm RPC
        }

        public override void update()
        {
            // Không cần kiểm tra dashTimer vì thời gian được quản lý trong DashingState
        }
    }

    /// <summary>
    /// Hành động tấn công cận chiến của nhân vật.
    /// </summary>
    public class MeleeAction : CharacterAction
    {
        private float range;
        private float duration;
        private MeleeType meleeType;

        public MeleeAction(PlayerController controller, float cooldown, float range, float duration, MeleeType meleeType)
            : base(controller, cooldown)
        {
            this.range = range;
            this.duration = duration;
            this.meleeType = meleeType;
        }

        protected override void PerformOffline(IUsable usable = null)
        {
            Vector3 attackDirection = GetAttackDirection();
            Collider[] hitColliders = Physics.OverlapSphere(controller.transform.position + controller.transform.forward + attackDirection * range / 2f, range);

            float effectValue = usable?.GetEffectValue() ?? controller.config.attackDamage;
            string usableName = usable?.Name ?? "Default";

            foreach (var hit in hitColliders)
            {
                if (hit.TryGetComponent<PlayerManager>(out var playerManager) &&
                    playerManager != controller.GetComponent<PlayerManager>())
                {
                    playerManager.ApplyDamage(effectValue, controller.name, usableName);
                    Debug.Log($"Hit {hit.name} with {meleeType} damage: {effectValue}");
                }
            }
            usable?.OnUse(controller);
            controller._stateMachine.AddSecondaryState(new MeleeAttackingState(duration, meleeType));
        }

        protected override void PerformOnline(IUsable usable = null)
        {
            float effectValue = usable?.GetEffectValue() ?? controller.config.attackDamage;
            controller._stateMachine.AddSecondaryState(new MeleeAttackingState(duration, meleeType));
            controller.RPC_RequestPerformMelee(meleeType, effectValue, usable?.Name ?? "Default");
            Debug.Log($"RPC Request: {controller.name} melee {meleeType} with effect value: {effectValue}");
        }

        private Vector3 GetAttackDirection()
        {
            switch (meleeType)
            {
                case MeleeType.Melee_01: return controller.transform.forward;
                case MeleeType.Melee_02: return -controller.transform.forward;
                case MeleeType.Melee_03: return controller.transform.right;
                case MeleeType.Melee_04: return -controller.transform.right;
                default: return controller.transform.forward;
            }
        }

        public override void update()
        {
            // Không cần timer vì thời gian được quản lý trong MeleeAttackingState
        }
    }

    /// <summary>
    /// Hành động ném của nhân vật.
    /// </summary>
    public class ThrowAction : CharacterAction
    {
        private string prefabPath;
        public float force;
        private ThrowType throwType;

        public ThrowAction(PlayerController controller, float cooldown, string prefabPath, float force, ThrowType throwType)
            : base(controller, cooldown)
        {
            this.prefabPath = prefabPath;
            this.force = force;
            this.throwType = throwType;
        }

        protected override void PerformOffline(IUsable usable = null)
        {
            GameObject gameObject = Resources.Load<GameObject>(prefabPath);
            if (gameObject == null)
            {
                Debug.LogError($"Prefab not found at path: {prefabPath}");
                return;
            }

            Vector3 spawnPosition = controller.transform.position + controller.transform.forward * 1.5f + Vector3.up * 1f;
            GameObject thrownObject = Object.Instantiate(gameObject, spawnPosition, Quaternion.identity);
            if (thrownObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = controller.transform.forward * force;
            }
            if (thrownObject.TryGetComponent<ThrowableObject>(out var throwable))
            {
                throwable.SetThrower(controller);
                if (usable != null) throwable.SetUsableData(usable.Name, usable.GetEffectValue());
            }

            usable?.OnUse(controller);
            controller._stateMachine.AddSecondaryState(new ThrowingState(cooldown));
        }

        protected override void PerformOnline(IUsable usable = null)
        {
            controller._stateMachine.AddSecondaryState(new ThrowingState(cooldown));
            controller.RPC_RequestPerformThrow(
                controller.transform.position,
                controller._playerInput._characterCamera.transform.forward,
                throwType,
                force,
                usable?.Name ?? "Default",
                usable?.GetEffectValue() ?? 0f
            );
            Debug.Log($"RPC Request: {controller.name} throw {usable?.Name ?? "Default"}");
        }

        public override void update()
        {
            // Không cần timer vì thời gian được quản lý trong ThrowingState
        }
    }

    /// <summary>
    /// Hành động tương tác của nhân vật.
    /// </summary>
    public class InteractAction : CharacterAction
    {
        private InteractType interactType;

        public InteractAction(PlayerController controller, float cooldown, InteractType interactType)
            : base(controller, cooldown)
        {
            this.interactType = interactType;
        }

        protected override bool CanPerform()
        {
            if (controller.IsAction && interactType == InteractType.PickUp)
            {
                return false; // Không thể nhặt khi đang hành động
            }
            if (interactType == InteractType.PickUp && controller.CurrentSourcesLookAt == null)
            {
                return false; // Không có đối tượng để nhặt
            }
            return Time.time - lastActionTime >= cooldown;
        }

        protected override void PerformOffline(IUsable usable = null)
        {
            switch (interactType)
            {
                case InteractType.PickUp:
                    if (controller.CurrentSourcesLookAt != null)
                    {
                        controller.PickUp();
                        //Debug.Log($"Picked up {controller.CurrentSourcesLookAt.name}");
                    }
                    break;
                case InteractType.Drop:
                    //controller.DropItem();
                    Debug.Log("Dropped item");
                    break;
                case InteractType.Active:
                    //controller.ActivateObject();
                    Debug.Log("Activated object");
                    break;
                case InteractType.Interact_04:
                    Debug.Log("Performed custom interaction");
                    break;
                default:
                    Debug.LogWarning($"Unknown InteractType: {interactType}");
                    break;
            }
            usable?.OnUse(controller);
            controller._stateMachine.AddSecondaryState(new InteractingState(0.5f, interactType));
        }

        protected override void PerformOnline(IUsable usable = null)
        {
            //controller.RPC_RequestPerformInteract(interactType, controller.CurrentSourcesLookAt);
            PerformOffline(usable);
        }

        public override void update()
        {
            // Không cần timer vì thời gian được quản lý trong InteractingState
        }
    }
}

//_____________________________________ ĐANG LỖI / CẦN CẢI TIẾN _____________________________________

//Một số hành động (như MoveAction) có logic phức tạp, với nhiều kiểm tra null và điều kiện, làm tăng nguy cơ lỗi.

//Logic online (PerformOnline) hiện tại chỉ gọi lại PerformOffline hoặc thêm RPC, chưa được triển khai đầy đủ.