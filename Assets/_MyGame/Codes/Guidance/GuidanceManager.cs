using System.Collections.Generic;
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
        private Dictionary<string, GuideObject> registeredGuides = new Dictionary<string, GuideObject>();
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
                Debug.Log($"[GuidanceManager] Initial guidance shown for: {nextEventId}");
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
            foreach (var eventId in registeredGuides.Keys)
            {
                // Kiểm tra event đã completed chưa
                if (ProgressionManager.Instance.IsEventCompleted(eventId))
                    continue;

                // Kiểm tra event có thể trigger được không
                if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                    !ProgressionManager.Instance.IsWaitingForEvent(eventId))
                    continue;

                // Lấy process data để kiểm tra TriggerType
                var processData = ProgressionManager.Instance.GetProcessData(eventId);
                
                // Chỉ hiển thị guidance cho SubProcess có TriggerType = Manual
                if (processData is SubProcess subProcess && subProcess.trigger == MainProcess.TriggerType.Manual)
                {
                    Debug.Log($"[GuidanceManager] Found next Manual event: {eventId}");
                    return eventId;
                }
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
            
            Debug.Log($"[GuidanceManager] Registered guide for event: {eventId}");
        }

        /// <summary>
        /// Hủy đăng ký GuideObject
        /// </summary>
        public void UnregisterGuide(string eventId)
        {
            if (registeredGuides.ContainsKey(eventId))
            {
                registeredGuides.Remove(eventId);
                Debug.Log($"[GuidanceManager] Unregistered guide for event: {eventId}");
            }
        }

        /// <summary>
        /// Hiển thị guide cho một event cụ thể
        /// </summary>
        public void ShowGuide(string eventId)
        {
            if (!enableGuidance) return;

            // Ẩn guide hiện tại nếu có
            HideCurrentGuide();

            // Hiển thị guide mới
            if (registeredGuides.TryGetValue(eventId, out var guideObject))
            {
                guideObject.Show();
                currentActiveGuideId = eventId;
                Debug.Log($"[GuidanceManager] Showing guide for: {eventId}");
            }
            else
            {
                Debug.LogWarning($"[GuidanceManager] No guide registered for eventId: {eventId}");
            }
        }

        /// <summary>
        /// Ẩn guide cho một event cụ thể
        /// </summary>
        public void HideGuide(string eventId)
        {
            if (registeredGuides.TryGetValue(eventId, out var guideObject))
            {
                guideObject.Hide();
                
                if (currentActiveGuideId == eventId)
                {
                    currentActiveGuideId = "";
                }
                
                Debug.Log($"[GuidanceManager] Hidden guide for: {eventId}");
            }
        }

        /// <summary>
        /// Ẩn guide hiện tại đang active
        /// </summary>
        public void HideCurrentGuide()
        {
            if (!string.IsNullOrEmpty(currentActiveGuideId))
            {
                HideGuide(currentActiveGuideId);
            }
        }

        /// <summary>
        /// Ẩn tất cả guides
        /// </summary>
        public void HideAllGuides()
        {
            foreach (var guide in registeredGuides.Values)
            {
                guide.Hide();
            }
            currentActiveGuideId = "";
            Debug.Log("[GuidanceManager] Hidden all guides");
        }

        /// <summary>
        /// Được gọi khi player tương tác với TriggerZone
        /// </summary>
        public void OnPlayerInteraction(string eventId)
        {
            HideGuide(eventId);
            Debug.Log($"[GuidanceManager] Player interacted with: {eventId}");
        }

        /// <summary>
        /// Được gọi khi event hoàn thành
        /// </summary>
        private void OnEventCompleted(object data)
        {
            var eventData = data as BaseEventData;
            if (eventData == null) return;
            
            var completedEventId = eventData.eventId;
            Debug.Log($"[GuidanceManager] Event completed: {completedEventId}");
            
            // Ẩn guide của event vừa completed
            HideGuide(completedEventId);
            
            // Tự động hiển thị guide cho event tiếp theo
            if (autoShowNextGuide)
            {
                var nextEventId = FindNextManualEvent();
                if (!string.IsNullOrEmpty(nextEventId))
                {
                    ShowGuide(nextEventId);
                    Debug.Log($"[GuidanceManager] Auto showing next guide: {nextEventId}");
                }
                else
                {
                    Debug.Log("[GuidanceManager] No more manual events to guide");
                }
            }
        }

        /// <summary>
        /// Enable/disable guidance system
        /// </summary>
        public void SetGuidanceEnabled(bool enabled)
        {
            enableGuidance = enabled;
            if (!enabled)
            {
                HideAllGuides();
            }
        }

        /// <summary>
        /// API public để show guide từ bên ngoài (dùng cho manual control)
        /// </summary>
        public void ShowGuideForEvent(string eventId)
        {
            ShowGuide(eventId);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe("event_completed", OnEventCompleted);
        }

        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 250));
            GUILayout.Label($"Current Active Guide: {currentActiveGuideId}");
            GUILayout.Label($"Registered Guides: {registeredGuides.Count}");
            GUILayout.Label($"Guidance Enabled: {enableGuidance}");
            GUILayout.Label($"Auto Show Next: {autoShowNextGuide}");
            
            if (GUILayout.Button("Hide All Guides"))
            {
                HideAllGuides();
            }
            
            if (GUILayout.Button("Find Next Manual Event"))
            {
                var nextEvent = FindNextManualEvent();
                Debug.Log($"Next manual event: {nextEvent}");
            }
            
            if (GUILayout.Button("Show Next Guide"))
            {
                var nextEventId = FindNextManualEvent();
                if (!string.IsNullOrEmpty(nextEventId))
                {
                    ShowGuide(nextEventId);
                }
            }
            
            GUILayout.EndArea();
        }
        #endif
    }
}
