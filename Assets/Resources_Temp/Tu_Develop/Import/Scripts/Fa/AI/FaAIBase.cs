using UnityEngine;

namespace Fa.AI
{
    /// <summary>
    /// Abstract base class cho Fa AI
    /// </summary>
    public abstract class FaAIBase : MonoBehaviour, IFaAI
    {
        [Header("Fa AI Settings")]
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float followDistance = 2f;
        [SerializeField] protected Transform playerTransform;
        
        protected bool isActive = false;
        protected IFaMovement movementSystem;
        
        #region IFaAI Implementation
        
        public virtual void Initialize()
        {
            if (playerTransform == null)
            {
                // Tìm người chơi trong scene
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    Debug.LogWarning("Fa: Không tìm thấy Player trong scene!");
                }
            }
            
            // Khởi tạo movement system
            InitializeMovementSystem();
            
            isActive = true;
            Debug.Log("Fa AI đã được khởi tạo");
        }
        
        public virtual void UpdateAI(float deltaTime)
        {
            if (!isActive || playerTransform == null) return;
            
            // Cập nhật logic AI
            UpdateAILogic(deltaTime);
        }
        
        public virtual void StopAI()
        {
            isActive = false;
            if (movementSystem != null)
            {
                movementSystem.StopMoving();
            }
            Debug.Log("Fa AI đã dừng");
        }
        
        public bool IsActive => isActive;
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Khởi tạo hệ thống di chuyển
        /// </summary>
        protected abstract void InitializeMovementSystem();
        
        /// <summary>
        /// Cập nhật logic AI chính
        /// </summary>
        /// <param name="deltaTime">Thời gian giữa các frame</param>
        protected abstract void UpdateAILogic(float deltaTime);
        
        #endregion
        
        #region Unity Methods
        
        protected virtual void Start()
        {
            Initialize();
        }
        
        protected virtual void Update()
        {
            UpdateAI(Time.deltaTime);
        }
        
        protected virtual void OnDestroy()
        {
            StopAI();
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Lấy khoảng cách đến người chơi
        /// </summary>
        protected float GetDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector3.Distance(transform.position, playerTransform.position);
        }
        
        /// <summary>
        /// Kiểm tra xem có cần di chuyển đến người chơi không
        /// </summary>
        protected bool ShouldMoveToPlayer()
        {
            return GetDistanceToPlayer() > followDistance;
        }
        
        #endregion
    }
} 