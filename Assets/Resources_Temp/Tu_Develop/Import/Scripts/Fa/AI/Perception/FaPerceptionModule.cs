using UnityEngine;
using System.Collections.Generic;

namespace Fa.AI.Perception
{
    /// <summary>
    /// Implementation của Perception Module cho Fa
    /// </summary>
    public class FaPerceptionModule : MonoBehaviour, IPerceptionModule
    {
        [Header("Perception Settings")]
        [SerializeField] private float perceptionRadius = 10f;
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private LayerMask enemyLayerMask = -1;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        
        [Header("Detection Settings")]
        [SerializeField] private float stuckDetectionDistance = 0.1f;
        [SerializeField] private float stuckDetectionTime = 3f;
        [SerializeField] private float lightLevelThreshold = 0.3f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool logPerceptionData = false;
        
        private PerceptionData currentData;
        private Transform targetTransform;
        private bool isActive = false;
        private float updateTimer = 0f;
        private Vector3 lastTargetPosition;
        private float stuckTimer = 0f;
        
        #region IPerceptionModule Implementation
        
        public void UpdatePerception(float deltaTime)
        {
            if (!isActive || targetTransform == null) return;
            
            updateTimer += deltaTime;
            if (updateTimer >= updateInterval)
            {
                CollectPerceptionData();
                updateTimer = 0f;
            }
        }
        
        public PerceptionData GetCurrentData()
        {
            return currentData?.Clone();
        }
        
        public void Initialize()
        {
            currentData = new PerceptionData();
            isActive = true;
            
            // Tìm target nếu chưa có
            if (targetTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    SetTarget(player.transform);
                }
            }
            
            Debug.Log("Fa Perception Module đã được khởi tạo");
        }
        
        public bool IsActive => isActive;
        
        public void SetTarget(Transform target)
        {
            targetTransform = target;
            if (target != null)
            {
                lastTargetPosition = target.position;
                currentData.PlayerPosition = target.position;
                currentData.LastPlayerPosition = target.position;
                Debug.Log($"Fa Perception: Target set to {target.name}");
            }
        }
        
        #endregion
        
        #region Unity Methods
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            UpdatePerception(Time.deltaTime);
        }
        
        #endregion
        
        #region Private Methods
        
        private void CollectPerceptionData()
        {
            if (targetTransform == null) return;
            
            // Cập nhật player state
            UpdatePlayerState();
            
            // Cập nhật environment state
            UpdateEnvironmentState();
            
            // Cập nhật time-based data
            UpdateTimeBasedData();
            
            // Cập nhật distance data
            UpdateDistanceData();
            
            // Cập nhật behavior patterns
            UpdateBehaviorPatterns();
            
            if (logPerceptionData)
            {
                Debug.Log($"Fa Perception Data: {currentData.GetDebugInfo()}");
            }
        }
        
        private void UpdatePlayerState()
        {
            Vector3 currentPosition = targetTransform.position;
            
            // Player position
            currentData.PlayerPosition = currentPosition;
            
            // Player movement speed
            float distanceMoved = Vector3.Distance(currentPosition, lastTargetPosition);
            currentData.PlayerMovementSpeed = distanceMoved / Time.deltaTime;
            
            // Player stuck detection
            if (distanceMoved < stuckDetectionDistance)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckDetectionTime)
                {
                    currentData.PlayerIsStuck = true;
                }
            }
            else
            {
                stuckTimer = 0f;
                currentData.PlayerIsStuck = false;
            }
            
            // Player under attack (simplified - có thể mở rộng)
            // currentData.PlayerUnderAttack = CheckPlayerUnderAttack();
            
            // Player health (simplified - có thể mở rộng)
            // currentData.PlayerHealth = GetPlayerHealth();
            
            lastTargetPosition = currentPosition;
        }
        
        private void UpdateEnvironmentState()
        {
            Vector3 perceptionCenter = transform.position;
            
            // Detect enemies
            currentData.NearbyEnemies = DetectNearbyObjects(enemyLayerMask, "Enemy");
            
            // Detect interactables
            currentData.NearbyInteractables = DetectNearbyObjects(interactableLayerMask, "Interactable");
            
            // Light level detection (simplified)
            currentData.LightLevel = DetectLightLevel();
            currentData.IsDarkArea = currentData.LightLevel < lightLevelThreshold;
            
            // Hidden objects detection (simplified)
            currentData.HasHiddenObjects = DetectHiddenObjects();
            
            // Dangerous area detection (simplified)
            currentData.IsDangerousArea = currentData.NearbyEnemies.Count > 0;
        }
        
        private void UpdateTimeBasedData()
        {
            // Cập nhật các timer
            currentData.TimeSinceLastHint += Time.deltaTime;
            currentData.TimeSinceLastSkill += Time.deltaTime;
            
            // Time since player moved
            if (currentData.PlayerMovementSpeed > 0.1f)
            {
                currentData.TimeSincePlayerMoved = 0f;
            }
            else
            {
                currentData.TimeSincePlayerMoved += Time.deltaTime;
            }
        }
        
        private void UpdateDistanceData()
        {
            // Distance to player
            currentData.DistanceToPlayer = Vector3.Distance(transform.position, currentData.PlayerPosition);
            
            // Distance to nearest enemy
            currentData.DistanceToNearestEnemy = float.MaxValue;
            foreach (var enemy in currentData.NearbyEnemies)
            {
                if (enemy != null)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < currentData.DistanceToNearestEnemy)
                    {
                        currentData.DistanceToNearestEnemy = distance;
                    }
                }
            }
            
            // Distance to nearest interactable
            currentData.DistanceToNearestInteractable = float.MaxValue;
            foreach (var interactable in currentData.NearbyInteractables)
            {
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, interactable.transform.position);
                    if (distance < currentData.DistanceToNearestInteractable)
                    {
                        currentData.DistanceToNearestInteractable = distance;
                    }
                }
            }
        }
        
        private void UpdateBehaviorPatterns()
        {
            // Player direction change detection
            Vector3 currentDirection = (currentData.PlayerPosition - currentData.LastPlayerPosition).normalized;
            Vector3 lastDirection = (currentData.LastPlayerPosition - transform.position).normalized;
            
            float angleChange = Vector3.Angle(currentDirection, lastDirection);
            currentData.PlayerDirectionChanged = angleChange > 45f;
            
            currentData.LastPlayerPosition = currentData.PlayerPosition;
        }
        
        private List<GameObject> DetectNearbyObjects(LayerMask layerMask, string objectType)
        {
            List<GameObject> detectedObjects = new List<GameObject>();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, perceptionRadius, layerMask);
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.gameObject != targetTransform?.gameObject)
                {
                    detectedObjects.Add(collider.gameObject);
                }
            }
            
            return detectedObjects;
        }
        
        private float DetectLightLevel()
        {
            // Simplified light level detection
            // Trong thực tế, có thể sử dụng Light Probes hoặc custom lighting system
            return 1f; // Default to full light
        }
        
        private bool DetectHiddenObjects()
        {
            // Simplified hidden objects detection
            // Trong thực tế, có thể sử dụng raycast hoặc special tags
            return false;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Reset perception timers
        /// </summary>
        public void ResetTimers()
        {
            currentData.TimeSinceLastHint = 0f;
            currentData.TimeSinceLastSkill = 0f;
            currentData.TimeSincePlayerMoved = 0f;
        }
        
        /// <summary>
        /// Thêm skill vào recently used list
        /// </summary>
        public void AddRecentlyUsedSkill(string skillName)
        {
            if (currentData != null)
            {
                currentData.RecentlyUsedSkills.Add(skillName);
                
                // Giữ chỉ 5 skills gần nhất
                if (currentData.RecentlyUsedSkills.Count > 5)
                {
                    currentData.RecentlyUsedSkills.RemoveAt(0);
                }
                
                currentData.TimeSinceLastSkill = 0f;
            }
        }
        
        #endregion
        
        #region Debug Methods
        
        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;
            
            // Vẽ perception radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, perceptionRadius);
            
            // Vẽ line đến target
            if (targetTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetTransform.position);
            }
            
            // Vẽ nearby enemies
            if (currentData != null)
            {
                Gizmos.color = Color.red;
                foreach (var enemy in currentData.NearbyEnemies)
                {
                    if (enemy != null)
                    {
                        Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
                    }
                }
                
                // Vẽ nearby interactables
                Gizmos.color = Color.green;
                foreach (var interactable in currentData.NearbyInteractables)
                {
                    if (interactable != null)
                    {
                        Gizmos.DrawWireSphere(interactable.transform.position, 0.3f);
                    }
                }
            }
        }
        
        #endregion
    }
} 