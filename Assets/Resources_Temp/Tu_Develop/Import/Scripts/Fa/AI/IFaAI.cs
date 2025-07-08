using UnityEngine;

namespace Fa.AI
{
    /// <summary>
    /// Interface cơ bản cho hệ thống AI của Fa
    /// </summary>
    public interface IFaAI
    {
        /// <summary>
        /// Khởi tạo AI
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Cập nhật AI mỗi frame
        /// </summary>
        /// <param name="deltaTime">Thời gian giữa các frame</param>
        void UpdateAI(float deltaTime);
        
        /// <summary>
        /// Dừng AI
        /// </summary>
        void StopAI();
        
        /// <summary>
        /// Kiểm tra xem AI có đang hoạt động không
        /// </summary>
        bool IsActive { get; }
    }
} 