using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;

namespace _MyGame.Codes.Guidance
{
    /// <summary>
    /// Manager đơn giản cho hệ thống guidance
    /// Quản lý việc hiển thị/ẩn các GuideObject dựa trên progression
    /// </summary>
    public class GuidanceManager : MonoBehaviour
    {
        public static GuidanceManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableGuidance = true;
        [SerializeField] private bool autoShowNextGuide = true; // Tự động hiển thị guide cho event tiếp theo

        // Dictionary lưu trữ tất cả GuideObject đã đăng ký
        private readonly Dictionary<string, GuideObject> registeredGuides = new Dictionary<string, GuideObject>();
        private string currentActiveGuideId = "";

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to event completed để biết khi nào cần hiển thị guide tiếp theo
            EventBus.Subscribe("event_completed", OnEventCompleted);
            
            // Khởi tạo guidance cho event đầu tiên sau khi tất cả component đã setup
            Invoke(nameof(InitializeFirstGuidance), 1f);
        }

        /// <summary>
        /// Tìm và hiển thị guidance cho event đầu tiên cần tương tác
        /// </summary>
        private void InitializeFirstGuidance()
        {
            if (!autoShowNextGuide) return;
            
            var nextEventId = FindNextManualEvent();
            if (!string.IsNullOrEmpty(nextEventId))
            {
                ShowGuide(nextEventId);
            }
        }

        /// <summary>
        /// Tìm event tiếp theo có TriggerType = Manual và chưa completed
        /// </summary>
        private string FindNextManualEvent()
        {
            if (ProgressionManager.Instance == null)
            {
                Debug.LogWarning("[GuidanceManager] ProgressionManager.Instance is null");
                return "";
            }

            // Duyệt qua tất cả registered guides để tìm event có thể trigger và có TriggerType = Manual
            foreach (var eventId 
                     in from eventId 
                         in registeredGuides.Keys 
                     where !ProgressionManager.Instance.IsEventCompleted(eventId) 
                     where ProgressionManager.Instance.CanTrigger(eventId) 
                           || ProgressionManager.Instance.IsWaitingForEvent(eventId) 
                     let processData = ProgressionManager.Instance.GetProcessData(eventId) 
                     where processData is SubProcess { trigger: MainProcess.TriggerType.Manual } 
                     select eventId)
            {
                return eventId;
            }

            return "";
        }

        /// <summary>
        /// Đăng ký một GuideObject với eventId
        /// </summary>
        public void RegisterGuide(string eventId, GuideObject guideObject)
        {
            if (string.IsNullOrEmpty(eventId) || guideObject == null)
            {
                Debug.LogWarning("[GuidanceManager] Invalid eventId or guideObject for registration");
                return;
            }

            registeredGuides[eventId] = guideObject;
            
            // Ẩn guide ban đầu
            guideObject.Hide();
        }

        /// <summary>
        /// Hủy đăng ký GuideObject
        /// </summary>
        public void UnregisterGuide(string eventId)
        {
            if (!registeredGuides.Remove(eventId)) return;
        }

        /// <summary>
        /// Hiển thị guide cho một event cụ thể
        /// </summary>
        private void ShowGuide(string eventId)
        {
            if (!enableGuidance) return;

            // Ẩn guide hiện tại nếu có
            HideCurrentGuide();

            // Hiển thị guide mới
            if (registeredGuides.TryGetValue(eventId, out var guideObject))
            {
                guideObject.Show();
                currentActiveGuideId = eventId;
                //Debug.Log($"[GuidanceManager] Showing guide for: {eventId}");
            }
            else
            {
                Debug.LogWarning($"[GuidanceManager] No guide registered for eventId: {eventId}");
            }
        }

        /// <summary>
        /// Ẩn guide cho một event cụ thể
        /// </summary>
        private void HideGuide(string eventId)
        {
            if (!registeredGuides.TryGetValue(eventId, out var guideObject)) return;
            guideObject.Hide();
                
            if (currentActiveGuideId == eventId)
            {
                currentActiveGuideId = "";
            }
        }

        /// <summary>
        /// Ẩn guide hiện tại đang active
        /// </summary>
        private void HideCurrentGuide()
        {
            if (!string.IsNullOrEmpty(currentActiveGuideId))
            {
                HideGuide(currentActiveGuideId);
            }
        }

        /// <summary>
        /// Được gọi khi player tương tác với TriggerZone
        /// </summary>
        public void OnPlayerInteraction(string eventId)
        {
            HideGuide(eventId);
        }

        /// <summary>
        /// Được gọi khi event hoàn thành
        /// </summary>
        private void OnEventCompleted(object data)
        {
            var eventData = data as BaseEventData;
            if (eventData == null) return;
            
            var completedEventId = eventData.eventId;
            // Ẩn guide của event vừa completed
            HideGuide(completedEventId);
            
            // Tự động hiển thị guide cho event tiếp theo
            if (!autoShowNextGuide) return;
            var nextEventId = FindNextManualEvent();
            if (!string.IsNullOrEmpty(nextEventId)) ShowGuide(nextEventId);
            // else
            // {
            //     Debug.Log("[GuidanceManager] No more manual events to guide");
            // }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe("event_completed", OnEventCompleted);
        }
    }
}
