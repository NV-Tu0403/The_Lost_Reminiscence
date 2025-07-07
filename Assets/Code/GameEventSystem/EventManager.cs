using System.Collections.Generic;
using Code.Procession;
using Script.Procession;
using UnityEngine;

// Script này quản lý các sự kiện trong game,
// bao gồm cả việc khởi chạy các sự kiện như cắt cảnh, thay đổi bản đồ, đối thoại, v.v.

namespace Code.GameEventSystem
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private List<string> _eventSequence = new List<string>();
        private int _currentEventIndex = 0;

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
        /// <param name="eventSequence">Danh sách các eventId sẽ được xử lý theo thứ tự.</param>
        public void Init(List<string> eventSequence)
        {
            _eventSequence = eventSequence;
            _currentEventIndex = 0;

            RegisterEventBusListeners();
            AutoTriggerFirstEvent();
        }

        /// <summary>
        /// Đăng ký các listener cho từng eventId trong chuỗi sự kiện lên EventBus.
        /// </summary>
        private void RegisterEventBusListeners()
        {
            foreach (var eventId in _eventSequence)
                EventBus.Subscribe(eventId, OnEventFinished);

            //Debug.Log($"[EventManager] Subscribed to {_eventSequence.Count} events via EventBus.");
        }

        /// <summary>
        /// Được gọi khi một sự kiện kết thúc, cập nhật tiến trình và thử kích hoạt sự kiện tiếp theo.
        /// </summary>
        /// <param name="eventData">Dữ liệu sự kiện vừa hoàn thành.</param>
        private void OnEventFinished(object eventData)
        {
            string eventId = (eventData as BaseEventData)?.eventId ?? eventData?.ToString();

            if (string.IsNullOrEmpty(eventId))
            {
                Debug.LogWarning("[EventManager] Event received but eventId is null.");
                return;
            }

            //Debug.Log($"[EventManager] Event '{eventId}' finished. Updating progression...");

            ProgressionManager.Instance.HandleEventFinished(eventId);
            UpdateEventIndex(eventId);
            TryTriggerNextEvent();
        }

        /// <summary>
        /// Cập nhật chỉ số sự kiện hiện tại dựa trên eventId vừa hoàn thành.
        /// </summary>
        /// <param name="eventId">ID của sự kiện vừa hoàn thành.</param>
        private void UpdateEventIndex(string eventId)
        {
            if (_eventSequence[_currentEventIndex] == eventId)
            {
                _currentEventIndex++;
            }
            else
            {
                int idx = _eventSequence.IndexOf(eventId);
                if (idx >= 0) _currentEventIndex = idx + 1;
            }
        }

        /// <summary>
        /// Tự động kích hoạt sự kiện đầu tiên nếu có thể.
        /// </summary>
        private void AutoTriggerFirstEvent()
        {
            if (_eventSequence.Count == 0) return;

            string firstEventId = _eventSequence[0];
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
            if (_currentEventIndex >= _eventSequence.Count) return;

            string nextEventId = _eventSequence[_currentEventIndex];
            if (!ProgressionManager.Instance.CanTrigger(nextEventId)) return;

            var processData = ProgressionManager.Instance.GetProcessData(nextEventId) as SubProcess;
            if (processData != null && processData.Trigger == MainProcess.TriggerType.Automatic)
            {
                //Debug.Log($"[EventManager] Auto triggering next event: {nextEventId}");
                EventExecutor.Instance.TriggerEvent(nextEventId);
            }
            else
            {
                Debug.Log($"[EventManager] Next event '{nextEventId}' is not Auto. Waiting...");
            }
        }

        /// <summary>
        /// Hủy đăng ký tất cả các listener khi đối tượng bị hủy.
        /// </summary>
        private void OnDestroy()
        {
            foreach (var eventId in _eventSequence)
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
