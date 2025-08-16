using System;
using System.Collections.Generic;

namespace _MyGame.Codes.GameEventSystem
{
    public static class EventBus
    {
        private static readonly Dictionary<string, Action<object>> eventTable = new();
        private static readonly Queue<EventData> eventQueue = new(); // Queue để batch process events
        private static bool _isProcessingEvents = false;

        // Struct để giảm GC allocation
        private struct EventData
        {
            public readonly string EventKey;
            public readonly object Data;
            
            public EventData(string key, object eventData)
            {
                EventKey = key;
                Data = eventData;
            }
        }

        /// <summary>
        /// Đăng ký một callback để lắng nghe sự kiện với eventKey.
        /// Khi sự kiện này được Publish, callback sẽ được gọi với dữ liệu truyền đi.
        /// Thường dùng để các hệ thống giao tiếp mà không cần tham chiếu trực tiếp.
        /// </summary>
        public static void Subscribe(string eventKey, Action<object> callback)
        {
            if (string.IsNullOrEmpty(eventKey))
            {
                UnityEngine.Debug.LogError("[EventBus] EventKey cannot be null or empty!");
                return;
            }

            if (callback == null)
            {
                UnityEngine.Debug.LogError("[EventBus] Callback cannot be null!");
                return;
            }

            if (!eventTable.ContainsKey(eventKey))
                eventTable[eventKey] = delegate { };

            eventTable[eventKey] += callback;
        }

        /// <summary>
        /// Hủy đăng ký một callback khỏi sự kiện với eventKey.
        /// Dùng để tránh memory leak hoặc khi không còn cần lắng nghe sự kiện nữa.
        /// </summary>
        public static void Unsubscribe(string eventKey, Action<object> callback)
        {
            if (string.IsNullOrEmpty(eventKey) || callback == null) return;

            if (!eventTable.ContainsKey(eventKey)) return;
            eventTable[eventKey] -= callback;
                
            // Cleanup empty entries to prevent memory leaks
            if (eventTable[eventKey] == null)
                eventTable.Remove(eventKey);
        }

        /// <summary>
        /// Phát (publish) một sự kiện với eventKey và dữ liệu đi kèm (data).
        /// Tất cả các callback đã đăng ký với eventKey này sẽ được gọi.
        /// Dùng để thông báo cho các hệ thống khác khi có sự kiện xảy ra.
        /// </summary>
        public static void Publish(string eventKey, object data = null)
        {
            if (string.IsNullOrEmpty(eventKey)) return;

            // Queue event để process sau, tránh recursive calls
            eventQueue.Enqueue(new EventData(eventKey, data));
            
            if (!_isProcessingEvents)
            {
                ProcessEventQueue();
            }
        }

        /// <summary>
        /// Process tất cả events trong queue
        /// </summary>
        private static void ProcessEventQueue()
        {
            _isProcessingEvents = true;
            
            while (eventQueue.Count > 0)
            {
                var eventData = eventQueue.Dequeue();

                if (!eventTable.TryGetValue(eventData.EventKey, out var callbacks)) continue;
                try
                {
                    callbacks?.Invoke(eventData.Data);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[EventBus] Exception in event '{eventData.EventKey}': {ex}");
                }
            }
            
            _isProcessingEvents = false;
        }

        /// <summary>
        /// Xóa toàn bộ các callback đã đăng ký cho mọi eventKey.
        /// Thường dùng khi chuyển scene hoặc reset game để tránh callback cũ bị giữ lại.
        /// </summary>
        public static void ClearAll()
        {
            eventTable.Clear();
            eventQueue.Clear();
            _isProcessingEvents = false;
        }

        /// <summary>
        /// Kiểm tra xem có callback nào đăng ký cho eventKey không
        /// </summary>
        private static bool HasSubscribers(string eventKey)
        {
            return !string.IsNullOrEmpty(eventKey) && eventTable.ContainsKey(eventKey) && eventTable[eventKey] != null;
        }

        /// <summary>
        /// Lấy số lượng subscribers cho một event
        /// </summary>
        public static int GetSubscriberCount(string eventKey)
        {
            return !HasSubscribers(eventKey) ? 0 : eventTable[eventKey].GetInvocationList().Length;
        }
    }
}