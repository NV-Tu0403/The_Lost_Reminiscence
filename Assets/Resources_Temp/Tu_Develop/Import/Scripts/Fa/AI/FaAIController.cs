using UnityEngine;
using Fa.AI.Perception;

namespace Fa.AI
{
    /// <summary>
    /// Fa AI Controller - Implementation cụ thể cho Fa
    /// </summary>
    public class FaAIController : FaAIBase
    {
        [Header("Fa Follow Settings")]
        [SerializeField] private float minFollowDistance = 1.5f;
        [SerializeField] private float maxFollowDistance = 3f;
        [SerializeField] private float smoothFollowSpeed = 2f;
        
        [Header("NavMesh Integration")]
        [SerializeField] private bool preferNavMesh = true;
        [SerializeField] private bool autoSwitchToBasicWhenStuck = true;
        
        [Header("Perception Integration")]
        [SerializeField] private bool enablePerception = true;
        [SerializeField] private bool logPerceptionData = false;
        
        private FaMovement movementComponent;
        private FaPerceptionModule perceptionModule;
        private Vector3 lastPlayerPosition;
        private float timeSinceLastMovement;
        private bool isNavMeshAvailable = false;
        private PerceptionData currentPerceptionData;
        
        #region FaAIBase Implementation
        
        protected override void InitializeMovementSystem()
        {
            // Tạo hoặc lấy component FaMovement
            movementComponent = GetComponent<FaMovement>();
            if (movementComponent == null)
            {
                movementComponent = gameObject.AddComponent<FaMovement>();
            }
            
            movementSystem = movementComponent;
            movementComponent.MoveSpeed = moveSpeed;
            
            // Khởi tạo Perception Module
            InitializePerceptionModule();
            
            // Kiểm tra NavMesh availability
            isNavMeshAvailable = movementComponent.IsNavMeshAvailable;
            
            // Thiết lập movement mode dựa trên preference và availability
            bool shouldUseNavMesh = preferNavMesh && isNavMeshAvailable;
            movementComponent.SetMovementMode(shouldUseNavMesh);
            
            // Khởi tạo vị trí ban đầu
            lastPlayerPosition = playerTransform != null ? playerTransform.position : transform.position;
            
            if (isNavMeshAvailable)
            {
                Debug.Log("Fa: Sử dụng NavMesh cho movement");
            }
            else
            {
                Debug.Log("Fa: Sử dụng basic movement (NavMesh không khả dụng)");
            }
        }
        
        protected override void UpdateAILogic(float deltaTime)
        {
            if (playerTransform == null) return;
            
            // Cập nhật thời gian
            timeSinceLastMovement += deltaTime;
            
            // Cập nhật perception data
            UpdatePerceptionData();
            
            // Kiểm tra NavMesh availability (có thể thay đổi runtime)
            CheckNavMeshAvailability();
            
            // Logic đi theo người chơi với perception
            UpdateFollowPlayerLogicWithPerception();
            
            // Cập nhật vị trí cuối cùng của player
            lastPlayerPosition = playerTransform.position;
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializePerceptionModule()
        {
            if (!enablePerception) return;
            
            perceptionModule = GetComponent<FaPerceptionModule>();
            if (perceptionModule == null)
            {
                perceptionModule = gameObject.AddComponent<FaPerceptionModule>();
            }
            
            perceptionModule.Initialize();
            perceptionModule.SetTarget(playerTransform);
            
            Debug.Log("Fa: Perception Module đã được khởi tạo");
        }
        
        private void UpdatePerceptionData()
        {
            if (perceptionModule != null && perceptionModule.IsActive)
            {
                currentPerceptionData = perceptionModule.GetCurrentData();
                
                if (logPerceptionData && currentPerceptionData != null)
                {
                    Debug.Log($"Fa Perception: {currentPerceptionData.GetDebugInfo()}");
                }
            }
        }
        
        private void CheckNavMeshAvailability()
        {
            bool currentNavMeshAvailable = movementComponent.IsNavMeshAvailable;
            
            // Nếu NavMesh availability thay đổi
            if (currentNavMeshAvailable != isNavMeshAvailable)
            {
                isNavMeshAvailable = currentNavMeshAvailable;
                
                if (isNavMeshAvailable && preferNavMesh)
                {
                    movementComponent.SetMovementMode(true);
                    Debug.Log("Fa: Chuyển sang NavMesh mode");
                }
                else if (!isNavMeshAvailable)
                {
                    movementComponent.SetMovementMode(false);
                    Debug.Log("Fa: Chuyển sang basic movement mode");
                }
            }
        }
        
        private void UpdateFollowPlayerLogicWithPerception()
        {
            float distanceToPlayer = GetDistanceToPlayer();
            
            // Kiểm tra nếu Fa bị stuck
            if (movementComponent.CurrentState == MovementState.Stuck)
            {
                HandleStuckSituation();
                return;
            }
            
            // Sử dụng perception data để quyết định behavior
            if (currentPerceptionData != null)
            {
                // Nếu player bị stuck, thử giúp đỡ
                if (currentPerceptionData.PlayerIsStuck)
                {
                    HandlePlayerStuckSituation();
                    return;
                }
                
                // Nếu có enemy gần đó, tăng khoảng cách follow
                if (currentPerceptionData.IsDangerousArea)
                {
                    HandleDangerousSituation();
                    return;
                }
                
                // Nếu ở khu vực tối, sử dụng skill ánh sáng
                if (currentPerceptionData.IsDarkArea)
                {
                    HandleDarkAreaSituation();
                }
            }
            
            // Logic follow cơ bản
            if (distanceToPlayer > maxFollowDistance)
            {
                Vector3 targetPosition = CalculateOptimalFollowPosition();
                movementComponent.MoveTo(targetPosition);
                timeSinceLastMovement = 0f;
            }
            else if (distanceToPlayer < minFollowDistance)
            {
                Vector3 awayDirection = (transform.position - playerTransform.position).normalized;
                Vector3 targetPosition = playerTransform.position + awayDirection * minFollowDistance;
                movementComponent.MoveTo(targetPosition);
                timeSinceLastMovement = 0f;
            }
            else if (ShouldAdjustPosition())
            {
                Vector3 targetPosition = CalculateOptimalFollowPosition();
                movementComponent.MoveTo(targetPosition);
                timeSinceLastMovement = 0f;
            }
        }
        
        private void HandlePlayerStuckSituation()
        {
            Debug.Log("Fa: Phát hiện player bị stuck, thử giúp đỡ...");
            
            // Di chuyển đến gần player hơn để hỗ trợ
            Vector3 supportPosition = playerTransform.position + Vector3.right * 1f;
            movementComponent.MoveTo(supportPosition);
            
            // Có thể thêm logic gợi ý hoặc skill hỗ trợ ở đây
        }
        
        private void HandleDangerousSituation()
        {
            Debug.Log("Fa: Phát hiện khu vực nguy hiểm, tăng khoảng cách follow");
            
            // Tăng khoảng cách follow khi có enemy
            float safeDistance = maxFollowDistance * 1.5f;
            if (GetDistanceToPlayer() < safeDistance)
            {
                Vector3 awayDirection = (transform.position - playerTransform.position).normalized;
                Vector3 safePosition = playerTransform.position + awayDirection * safeDistance;
                movementComponent.MoveTo(safePosition);
            }
        }
        
        private void HandleDarkAreaSituation()
        {
            Debug.Log("Fa: Phát hiện khu vực tối, chuẩn bị skill ánh sáng");
            
            // Có thể thêm logic để sử dụng skill ánh sáng
            // perceptionModule.AddRecentlyUsedSkill("KnowledgeLight");
        }
        
        private void HandleStuckSituation()
        {
            if (autoSwitchToBasicWhenStuck)
            {
                // Chuyển sang basic movement khi bị stuck
                movementComponent.SetMovementMode(false);
                Debug.Log("Fa: Bị stuck, chuyển sang basic movement");
                
                // Thử di chuyển đến vị trí khác
                Vector3 randomDirection = Random.insideUnitSphere.normalized;
                randomDirection.y = 0;
                Vector3 escapePosition = transform.position + randomDirection * 2f;
                movementComponent.MoveTo(escapePosition);
            }
        }
        
        private Vector3 CalculateOptimalFollowPosition()
        {
            if (playerTransform == null) return transform.position;
            
            // Tính toán vị trí tối ưu để follow player
            Vector3 playerPosition = playerTransform.position;
            Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
            
            // Vị trí mục tiêu ở giữa khoảng cách min và max
            float optimalDistance = (minFollowDistance + maxFollowDistance) * 0.5f;
            Vector3 optimalPosition = playerPosition - directionToPlayer * optimalDistance;
            
            // Thêm một chút randomness để tránh follow quá cứng nhắc
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );
            
            return optimalPosition + randomOffset;
        }
        
        private bool ShouldAdjustPosition()
        {
            // Điều chỉnh vị trí nếu player di chuyển đủ xa
            float playerMovementDistance = Vector3.Distance(lastPlayerPosition, playerTransform.position);
            return playerMovementDistance > 0.5f && timeSinceLastMovement > 1f;
        }
        
        #endregion
        
        #region Debug Methods
        
        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;
            
            // Vẽ khoảng cách follow
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, minFollowDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, maxFollowDistance);
            
            // Vẽ vị trí tối ưu
            Gizmos.color = Color.blue;
            Vector3 optimalPosition = CalculateOptimalFollowPosition();
            Gizmos.DrawWireSphere(optimalPosition, 0.3f);
            
            // Vẽ movement state
            if (movementComponent != null)
            {
                Color stateColor = movementComponent.CurrentState switch
                {
                    MovementState.Idle => Color.white,
                    MovementState.Moving => Color.yellow,
                    MovementState.Following => Color.cyan,
                    MovementState.Pathfinding => Color.green,
                    MovementState.Stuck => Color.red,
                    _ => Color.gray
                };
                
                Gizmos.color = stateColor;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Thiết lập khoảng cách follow
        /// </summary>
        public void SetFollowDistance(float minDistance, float maxDistance)
        {
            minFollowDistance = minDistance;
            maxFollowDistance = maxDistance;
        }
        
        /// <summary>
        /// Thiết lập NavMesh preference
        /// </summary>
        public void SetNavMeshPreference(bool preferNavMesh)
        {
            this.preferNavMesh = preferNavMesh;
            if (movementComponent != null)
            {
                bool shouldUseNavMesh = preferNavMesh && isNavMeshAvailable;
                movementComponent.SetMovementMode(shouldUseNavMesh);
            }
        }
        
        /// <summary>
        /// Bật/tắt perception module
        /// </summary>
        public void SetPerceptionEnabled(bool enabled)
        {
            enablePerception = enabled;
            if (perceptionModule != null)
            {
                if (enabled)
                {
                    perceptionModule.Initialize();
                }
            }
        }
        
        /// <summary>
        /// Lấy perception data hiện tại
        /// </summary>
        public PerceptionData GetCurrentPerceptionData()
        {
            return currentPerceptionData;
        }
        
        /// <summary>
        /// Lấy thông tin debug về trạng thái follow
        /// </summary>
        public string GetDebugInfo()
        {
            if (playerTransform == null) return "Không có player";
            
            float distance = GetDistanceToPlayer();
            string movementState = movementComponent?.CurrentState.ToString() ?? "Unknown";
            string navMeshStatus = isNavMeshAvailable ? "Available" : "Not Available";
            string movementMode = movementComponent?.IsNavMeshAvailable == true ? "NavMesh" : "Basic";
            string perceptionStatus = perceptionModule?.IsActive == true ? "Active" : "Inactive";
            
            string debugInfo = $"Distance to Player: {distance:F2}\n" +
                             $"Min Distance: {minFollowDistance:F2}\n" +
                             $"Max Distance: {maxFollowDistance:F2}\n" +
                             $"Movement State: {movementState}\n" +
                             $"NavMesh: {navMeshStatus}\n" +
                             $"Mode: {movementMode}\n" +
                             $"Perception: {perceptionStatus}\n" +
                             $"Is Moving: {movementComponent?.IsMoving}\n" +
                             $"Time Since Movement: {timeSinceLastMovement:F2}";
            
            // Thêm perception data nếu có
            if (currentPerceptionData != null)
            {
                debugInfo += $"\n\nPerception Data:\n{currentPerceptionData.GetDebugInfo()}";
            }
            
            return debugInfo;
        }
        
        #endregion
    }
} 