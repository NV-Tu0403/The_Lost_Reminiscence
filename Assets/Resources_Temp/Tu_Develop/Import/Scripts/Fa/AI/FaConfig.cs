using UnityEngine;

namespace Fa.AI
{
    /// <summary>
    /// ScriptableObject để cấu hình Fa
    /// </summary>
    [CreateAssetMenu(fileName = "FaConfig", menuName = "Fa/AI Configuration")]
    public class FaConfig : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float minFollowDistance = 1.5f;
        [SerializeField] private float maxFollowDistance = 3f;
        [SerializeField] private float stoppingDistance = 0.1f;
        
        [Header("NavMesh Settings")]
        [SerializeField] private bool useNavMesh = true;
        [SerializeField] private float navMeshSampleRadius = 5f;
        [SerializeField] private float navMeshAgentRadius = 0.5f;
        [SerializeField] private float navMeshAgentHeight = 2f;
        
        [Header("Fallback Settings")]
        [SerializeField] private float basicMovementSpeed = 2f;
        [SerializeField] private float stuckDetectionTime = 3f;
        [SerializeField] private float stuckDetectionDistance = 0.1f;
        [SerializeField] private bool enableStuckRecovery = true;
        
        [Header("AI Behavior")]
        [SerializeField] private bool enableSmoothFollow = true;
        [SerializeField] private float smoothFollowSpeed = 2f;
        [SerializeField] private bool enableRandomMovement = true;
        [SerializeField] private float randomMovementRadius = 0.5f;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showOnScreenDebug = true;
        
        #region Properties
        
        public float MoveSpeed => moveSpeed;
        public float MinFollowDistance => minFollowDistance;
        public float MaxFollowDistance => maxFollowDistance;
        public float StoppingDistance => stoppingDistance;
        public bool UseNavMesh => useNavMesh;
        public float NavMeshSampleRadius => navMeshSampleRadius;
        public float NavMeshAgentRadius => navMeshAgentRadius;
        public float NavMeshAgentHeight => navMeshAgentHeight;
        public float BasicMovementSpeed => basicMovementSpeed;
        public float StuckDetectionTime => stuckDetectionTime;
        public float StuckDetectionDistance => stuckDetectionDistance;
        public bool EnableStuckRecovery => enableStuckRecovery;
        public bool EnableSmoothFollow => enableSmoothFollow;
        public float SmoothFollowSpeed => smoothFollowSpeed;
        public bool EnableRandomMovement => enableRandomMovement;
        public float RandomMovementRadius => randomMovementRadius;
        public bool ShowDebugGizmos => showDebugGizmos;
        public bool EnableDebugLogs => enableDebugLogs;
        public bool ShowOnScreenDebug => showOnScreenDebug;
        
        #endregion
        
        #region Validation
        
        private void OnValidate()
        {
            // Đảm bảo các giá trị hợp lý
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            minFollowDistance = Mathf.Max(0.5f, minFollowDistance);
            maxFollowDistance = Mathf.Max(minFollowDistance + 0.5f, maxFollowDistance);
            stoppingDistance = Mathf.Max(0.01f, stoppingDistance);
            navMeshSampleRadius = Mathf.Max(1f, navMeshSampleRadius);
            navMeshAgentRadius = Mathf.Max(0.1f, navMeshAgentRadius);
            navMeshAgentHeight = Mathf.Max(0.5f, navMeshAgentHeight);
            basicMovementSpeed = Mathf.Max(0.1f, basicMovementSpeed);
            stuckDetectionTime = Mathf.Max(0.5f, stuckDetectionTime);
            stuckDetectionDistance = Mathf.Max(0.01f, stuckDetectionDistance);
            smoothFollowSpeed = Mathf.Max(0.1f, smoothFollowSpeed);
            randomMovementRadius = Mathf.Max(0.1f, randomMovementRadius);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Tạo bản sao của config
        /// </summary>
        public FaConfig CreateCopy()
        {
            var copy = CreateInstance<FaConfig>();
            copy.moveSpeed = this.moveSpeed;
            copy.minFollowDistance = this.minFollowDistance;
            copy.maxFollowDistance = this.maxFollowDistance;
            copy.stoppingDistance = this.stoppingDistance;
            copy.useNavMesh = this.useNavMesh;
            copy.navMeshSampleRadius = this.navMeshSampleRadius;
            copy.navMeshAgentRadius = this.navMeshAgentRadius;
            copy.navMeshAgentHeight = this.navMeshAgentHeight;
            copy.basicMovementSpeed = this.basicMovementSpeed;
            copy.stuckDetectionTime = this.stuckDetectionTime;
            copy.stuckDetectionDistance = this.stuckDetectionDistance;
            copy.enableStuckRecovery = this.enableStuckRecovery;
            copy.enableSmoothFollow = this.enableSmoothFollow;
            copy.smoothFollowSpeed = this.smoothFollowSpeed;
            copy.enableRandomMovement = this.enableRandomMovement;
            copy.randomMovementRadius = this.randomMovementRadius;
            copy.showDebugGizmos = this.showDebugGizmos;
            copy.enableDebugLogs = this.enableDebugLogs;
            copy.showOnScreenDebug = this.showOnScreenDebug;
            return copy;
        }
        
        /// <summary>
        /// Áp dụng config vào Fa AI Controller
        /// </summary>
        public void ApplyToController(FaAIController controller)
        {
            if (controller == null) return;
            
            // Áp dụng các giá trị cấu hình
            controller.SetFollowDistance(minFollowDistance, maxFollowDistance);
            
            // Có thể thêm các method khác để áp dụng config
        }
        
        /// <summary>
        /// Áp dụng config vào Fa Movement
        /// </summary>
        public void ApplyToMovement(FaMovement movement)
        {
            if (movement == null) return;
            
            movement.MoveSpeed = moveSpeed;
            movement.SetMovementMode(useNavMesh);
        }
        
        #endregion
    }
} 