using UnityEngine;
using UnityEngine.AI;

namespace Fa.AI
{
    /// <summary>
    /// Interface cho hệ thống di chuyển của Fa
    /// </summary>
    public interface IFaMovement
    {
        /// <summary>
        /// Di chuyển đến vị trí mục tiêu
        /// </summary>
        /// <param name="targetPosition">Vị trí đích</param>
        void MoveTo(Vector3 targetPosition);
        
        /// <summary>
        /// Di chuyển theo người chơi
        /// </summary>
        /// <param name="playerTransform">Transform của người chơi</param>
        /// <param name="followDistance">Khoảng cách theo dõi</param>
        void FollowPlayer(Transform playerTransform, float followDistance);
        
        /// <summary>
        /// Dừng di chuyển
        /// </summary>
        void StopMoving();
        
        /// <summary>
        /// Kiểm tra xem có đang di chuyển không
        /// </summary>
        bool IsMoving { get; }
        
        /// <summary>
        /// Tốc độ di chuyển
        /// </summary>
        float MoveSpeed { get; set; }
        
        /// <summary>
        /// Khoảng cách hiện tại đến mục tiêu
        /// </summary>
        float DistanceToTarget { get; }
        
        /// <summary>
        /// Kiểm tra xem NavMesh có khả dụng không
        /// </summary>
        bool IsNavMeshAvailable { get; }
        
        /// <summary>
        /// Thiết lập chế độ di chuyển
        /// </summary>
        /// <param name="useNavMesh">Sử dụng NavMesh hay không</param>
        void SetMovementMode(bool useNavMesh);
        
        /// <summary>
        /// Lấy trạng thái di chuyển hiện tại
        /// </summary>
        MovementState CurrentState { get; }
    }
    
    /// <summary>
    /// Trạng thái di chuyển của Fa
    /// </summary>
    public enum MovementState
    {
        Idle,           // Đứng yên
        Moving,         // Đang di chuyển
        Following,      // Đang follow player
        Pathfinding,    // Đang tìm đường (NavMesh)
        Stuck           // Bị kẹt
    }
} 