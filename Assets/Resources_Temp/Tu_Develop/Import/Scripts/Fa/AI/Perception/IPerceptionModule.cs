using UnityEngine;

namespace Fa.AI.Perception
{
    /// <summary>
    /// Interface cho Perception Module - thu thập dữ liệu môi trường
    /// </summary>
    public interface IPerceptionModule
    {
        /// <summary>
        /// Cập nhật perception data
        /// </summary>
        /// <param name="deltaTime">Thời gian giữa các frame</param>
        void UpdatePerception(float deltaTime);
        
        /// <summary>
        /// Lấy perception data hiện tại
        /// </summary>
        PerceptionData GetCurrentData();
        
        /// <summary>
        /// Khởi tạo perception module
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Kiểm tra xem module có đang hoạt động không
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Thiết lập target để perception
        /// </summary>
        /// <param name="target">Target transform</param>
        void SetTarget(Transform target);
    }
    
    /// <summary>
    /// Dữ liệu perception được thu thập
    /// </summary>
    [System.Serializable]
    public class PerceptionData
    {
        [Header("Player State")]
        public bool PlayerUnderAttack = false;
        public float PlayerHealth = 100f;
        public Vector3 PlayerPosition = Vector3.zero;
        public float PlayerMovementSpeed = 0f;
        public bool PlayerIsStuck = false;
        
        [Header("Environment State")]
        public bool IsDarkArea = false;
        public bool HasHiddenObjects = false;
        public bool IsDangerousArea = false;
        public float LightLevel = 1f;
        public List<GameObject> NearbyEnemies = new List<GameObject>();
        public List<GameObject> NearbyInteractables = new List<GameObject>();
        
        [Header("Time-based Data")]
        public float TimeSinceLastHint = 0f;
        public float TimeSinceLastSkill = 0f;
        public float TimeSincePlayerMoved = 0f;
        
        [Header("Distance Data")]
        public float DistanceToPlayer = 0f;
        public float DistanceToNearestEnemy = float.MaxValue;
        public float DistanceToNearestInteractable = float.MaxValue;
        
        [Header("Behavior Patterns")]
        public List<string> RecentlyUsedSkills = new List<string>();
        public Vector3 LastPlayerPosition = Vector3.zero;
        public bool PlayerDirectionChanged = false;
        
        /// <summary>
        /// Tạo bản sao của data
        /// </summary>
        public PerceptionData Clone()
        {
            return new PerceptionData
            {
                PlayerUnderAttack = this.PlayerUnderAttack,
                PlayerHealth = this.PlayerHealth,
                PlayerPosition = this.PlayerPosition,
                PlayerMovementSpeed = this.PlayerMovementSpeed,
                PlayerIsStuck = this.PlayerIsStuck,
                IsDarkArea = this.IsDarkArea,
                HasHiddenObjects = this.HasHiddenObjects,
                IsDangerousArea = this.IsDangerousArea,
                LightLevel = this.LightLevel,
                NearbyEnemies = new List<GameObject>(this.NearbyEnemies),
                NearbyInteractables = new List<GameObject>(this.NearbyInteractables),
                TimeSinceLastHint = this.TimeSinceLastHint,
                TimeSinceLastSkill = this.TimeSinceLastSkill,
                TimeSincePlayerMoved = this.TimeSincePlayerMoved,
                DistanceToPlayer = this.DistanceToPlayer,
                DistanceToNearestEnemy = this.DistanceToNearestEnemy,
                DistanceToNearestInteractable = this.DistanceToNearestInteractable,
                RecentlyUsedSkills = new List<string>(this.RecentlyUsedSkills),
                LastPlayerPosition = this.LastPlayerPosition,
                PlayerDirectionChanged = this.PlayerDirectionChanged
            };
        }
        
        /// <summary>
        /// Lấy thông tin debug
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Player Health: {PlayerHealth:F1}\n" +
                   $"Player Under Attack: {PlayerUnderAttack}\n" +
                   $"Distance to Player: {DistanceToPlayer:F2}\n" +
                   $"Is Dark Area: {IsDarkArea}\n" +
                   $"Light Level: {LightLevel:F2}\n" +
                   $"Nearby Enemies: {NearbyEnemies.Count}\n" +
                   $"Time Since Last Hint: {TimeSinceLastHint:F1}s\n" +
                   $"Player Movement Speed: {PlayerMovementSpeed:F2}";
        }
    }
} 