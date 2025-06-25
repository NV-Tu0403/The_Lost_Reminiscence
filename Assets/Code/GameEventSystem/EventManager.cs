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

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Init(List<string> eventSequence)
        {
            _eventSequence = eventSequence;
            _currentEventIndex = 0;

            RegisterEventBusListeners();
            AutoTriggerFirstEvent();
        }

        private void RegisterEventBusListeners()
        {
            foreach (var eventId in _eventSequence)
                EventBus.Subscribe(eventId, OnEventFinished);

            Debug.Log($"[EventManager] Subscribed to {_eventSequence.Count} events via EventBus.");
        }

        private void OnEventFinished(object eventData)
        {
            string eventId = (eventData as BaseEventData)?.eventId ?? eventData?.ToString();

            if (string.IsNullOrEmpty(eventId))
            {
                Debug.LogWarning("[EventManager] Event received but eventId is null.");
                return;
            }

            Debug.Log($"[EventManager] Event '{eventId}' finished. Updating progression...");

            ProgressionManager.Instance.HandleEventFinished(eventId);
            UpdateEventIndex(eventId);
            TryTriggerNextEvent();
        }

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

        private void AutoTriggerFirstEvent()
        {
            if (_eventSequence.Count == 0) return;

            string firstEventId = _eventSequence[0];
            if (ProgressionManager.Instance.CanTrigger(firstEventId))
            {
                Debug.Log($"[EventManager] Auto trigger first event: {firstEventId}");
                EventExecutor.Instance.TriggerEvent(firstEventId);
            }
        }

        private void TryTriggerNextEvent()
        {
            if (_currentEventIndex >= _eventSequence.Count) return;

            string nextEventId = _eventSequence[_currentEventIndex];
            if (!ProgressionManager.Instance.CanTrigger(nextEventId)) return;

            var processData = ProgressionManager.Instance.GetProcessData(nextEventId) as SubProcess;
            if (processData != null && processData.Trigger == MainProcess.TriggerType.Automatic)
            {
                Debug.Log($"[EventManager] Auto triggering next event: {nextEventId}");
                EventExecutor.Instance.TriggerEvent(nextEventId);
            }
            else
            {
                Debug.Log($"[EventManager] Next event '{nextEventId}' is not Auto. Waiting...");
            }
        }

        private void OnDestroy()
        {
            foreach (var eventId in _eventSequence)
                EventBus.Unsubscribe(eventId, OnEventFinished);

            Debug.Log("[EventManager] Unsubscribed from all events.");
        }
    }
}

