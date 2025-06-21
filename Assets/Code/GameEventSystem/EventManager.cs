using Script.Procession;
using UnityEngine;

// Script này quản lý các sự kiện trong game,
// bao gồm cả việc khởi chạy các sự kiện như cắt cảnh, thay đổi bản đồ, đối thoại, v.v.

namespace Script.GameEventSystem
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }

        /// <summary>
        /// Callback từ IEventAction khi một event hoàn thành.
        /// </summary>
        public void OnEventFinished(string eventId)
        {
            Debug.Log($"[EventManager] Event '{eventId}' đã hoàn thành");
            
            // Sau đó gọi ProgressionManager để cập nhật progression
            ProgressionManager.Instance.HandleEventFinished(eventId);
        }
    }
}

