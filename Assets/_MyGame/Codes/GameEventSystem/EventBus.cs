using System;
using System.Collections.Generic;

namespace _MyGame.Codes.GameEventSystem
{
    public static class EventBus
    {
        private static Dictionary<string, Action<object>> eventTable = new();

        /// <summary>
        /// Đăng ký một callback để lắng nghe sự kiện với eventKey.
        /// Khi sự kiện này được Publish, callback sẽ được gọi với dữ liệu truyền đi.
        /// Thường dùng để các hệ thống giao tiếp mà không cần tham chiếu trực tiếp.
        /// </summary>
        public static void Subscribe(string eventKey, Action<object> callback)
        {
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
            if (eventTable.ContainsKey(eventKey))
                eventTable[eventKey] -= callback;
        }

        /// <summary>
        /// Phát (publish) một sự kiện với eventKey và dữ liệu đi kèm (data).
        /// Tất cả các callback đã đăng ký với eventKey này sẽ được gọi.
        /// Dùng để thông báo cho các hệ thống khác khi có sự kiện xảy ra.
        /// </summary>
        public static void Publish(string eventKey, object data = null)
        {
            if (eventTable.ContainsKey(eventKey)) eventTable[eventKey]?.Invoke(data);
        }

        /// <summary>
        /// Xóa toàn bộ các callback đã đăng ký cho mọi eventKey.
        /// Thường dùng khi chuyển scene hoặc reset game để tránh callback cũ bị giữ lại.
        /// </summary>
        public static void ClearAll()
        {
            eventTable.Clear();
        }
    }
}