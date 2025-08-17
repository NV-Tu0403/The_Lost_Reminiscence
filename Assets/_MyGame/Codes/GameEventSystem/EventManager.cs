using System.Collections.Generic;
using _MyGame.Codes.Procession;
using UnityEngine;

// Script này quản lý các sự kiện trong game,
// bao gồm cả việc khởi chạy các sự kiện như cắt cảnh, thay đổi bản đồ, đối thoại, v.v.

namespace _MyGame.Codes.GameEventSystem
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private List<string> eventSequence = new List<string>();
        private int currentEventIndex = 0;

        /// <summary>
        /// Đảm bảo chỉ có một instance EventManager tồn tại (singleton pattern).
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Khởi tạo chuỗi sự kiện và đăng ký lắng nghe các sự kiện.
        /// </summary>
        /// <param name="sequence">Danh sách các eventId sẽ được xử lý theo thứ tự.</param>
        public void Init(List<string> sequence)
        {
            this.eventSequence = sequence;
            currentEventIndex = 0;

            RegisterEventBusListeners();
            AutoTriggerFirstEvent();
        }

        /// <summary>
        /// Đăng ký các listener cho từng eventId trong chuỗi sự kiện lên EventBus.
        /// </summary>
        private void RegisterEventBusListeners()
        {
            foreach (var eventId in eventSequence)
                EventBus.Subscribe(eventId, OnEventFinished);

            //Debug.Log($"[EventManager] Subscribed to {_eventSequence.Count} events via EventBus.");
        }

        /// <summary>
        /// Được gọi khi một sự kiện kết thúc, cập nhật tiến trình và thử kích hoạt sự kiện tiếp theo.
        /// </summary>
        /// <param name="eventData">Dữ liệu sự kiện vừa hoàn thành.</param>
        private void OnEventFinished(object eventData)
        {
            var eventId = (eventData as BaseEventData)?.eventId ?? eventData?.ToString();

            if (string.IsNullOrEmpty(eventId))
            {
                Debug.LogWarning("[EventManager] Event received but eventId is null.");
                return;
            }

            Debug.Log($"[EventManager] Event '{eventId}' finished. Updating progression...");

            ProgressionManager.Instance.HandleEventFinished(eventId);
            UpdateEventIndex(eventId);
            
            // QUAN TRỌNG: Thông báo cho GuidanceManager về event completed
            Debug.Log($"[EventManager] Publishing event_completed for: {eventId}");
            EventBus.Publish("event_completed", eventData);
            
            TryTriggerNextEvent();
        }

        /// <summary>
        /// Cập nhật chỉ số sự kiện hiện tại dựa trên eventId vừa hoàn thành.
        /// </summary>
        /// <param name="eventId">ID của sự kiện vừa hoàn thành.</param>
        private void UpdateEventIndex(string eventId)
        {
            if (eventSequence[currentEventIndex] == eventId)
            {
                currentEventIndex++;
            }
            else
            {
                var idx = eventSequence.IndexOf(eventId);
                if (idx >= 0) currentEventIndex = idx + 1;
            }
        }

        /// <summary>
        /// Tự động kích hoạt sự kiện đầu tiên nếu có thể.
        /// </summary>
        private void AutoTriggerFirstEvent()
        {
            if (eventSequence.Count == 0) return;

            var firstEventId = eventSequence[0];
            if (ProgressionManager.Instance.CanTrigger(firstEventId))
            {
                //Debug.Log($"[EventManager] Auto trigger first event: {firstEventId}");
                EventExecutor.Instance.TriggerEvent(firstEventId);
            }
        }

        /// <summary>
        /// Thử kích hoạt sự kiện tiếp theo nếu điều kiện cho phép và sự kiện đó là tự động.
        /// </summary>
        private void TryTriggerNextEvent()
        {
            if (currentEventIndex >= eventSequence.Count) return;

            var nextEventId = eventSequence[currentEventIndex];
            if (!ProgressionManager.Instance.CanTrigger(nextEventId)) return;

            if (ProgressionManager.Instance.GetProcessData(nextEventId) is SubProcess { trigger: MainProcess.TriggerType.Automatic })
            {
                //Debug.Log($"[EventManager] Auto triggering next event: {nextEventId}");
                EventExecutor.Instance.TriggerEvent(nextEventId);
            }
        }

        /// <summary>
        /// Hủy đăng ký tất cả các listener khi đối tượng bị hủy.
        /// </summary>
        private void OnDestroy()
        {
            foreach (var eventId in eventSequence)
                EventBus.Unsubscribe(eventId, OnEventFinished);
            //Debug.Log("[EventManager] Unsubscribed from all events.");
        }

        /// <summary>
        /// Cho phép hoàn thành cưỡng bức một event (dùng cho DevMode).
        /// </summary>
        public void ForceCompleteEvent(string eventId)
        {
            // Đánh dấu progression đã hoàn thành
            ProgressionManager.Instance.HandleEventFinished(eventId);
            // Phát event lên EventBus để các hệ thống khác (VFX, puzzle, ...) nhận được
            var data = EventExecutor.Instance.GetEventDataById(eventId);
            EventBus.Publish(eventId, data);
            UpdateEventIndex(eventId);
            TryTriggerNextEvent();
        }
    }
}
