using System;
using System.Collections.Generic;
using Loc_Backend.Scripts;
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
        /// Bắt đầu một event, truyền thẳng vào EventExecutor.
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            Debug.Log("[EventManager] Triggering event: " + eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
        }

        /// <summary>
        /// Callback từ IEventAction khi một event hoàn thành.
        /// </summary>
        public void OnEventFinished(string eventId)
        {
            Debug.Log($"[EventManager] Event '{eventId}' đã hoàn thành");
            // Sau đó ta gọi ProgressionManager để cập nhật progression
            ProgressionManager.Instance.HandleEventFinished(eventId);

            // Nếu progression có event kế tiếp, tự trigger
            //string nextEventId = ProgressionManager.Instance.GetNextEventId();
            //if (!string.IsNullOrEmpty(nextEventId))
            //{
            //    Debug.Log($"[EventManager] Trigger tiếp event: {nextEventId}");
            //    TriggerEvent(nextEventId); // Tức quay lại EventExecutor.TriggerEvent(nextEventId)
            //}
            //else
            //    Debug.Log("[EventManager] Không còn event nào trong progression!");
        }
    }
}

