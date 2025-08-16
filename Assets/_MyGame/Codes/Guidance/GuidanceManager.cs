using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;

namespace _MyGame.Codes.SimpleGuidance
{
    /// <summary>
    /// Hệ thống guidance đơn giản - chỉ hiển thị mũi tên cho TriggerZone tiếp theo cần tương tác
    /// </summary>
    public class GuidanceManager : MonoBehaviour
    {
        public static GuidanceManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableGuidance = true;
        [SerializeField] private float guidanceActivationDelay = 10f; // Hiện mũi tên sau 10s

        private Dictionary<string, GameObject> guidanceObjects = new Dictionary<string, GameObject>();
        private string currentTargetEventId = "";
        private float lastInteractionTime;

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
            // Subscribe to EventBus để biết khi nào event completed
            EventBus.Subscribe("event_completed", OnEventCompleted);
            
            // Tìm và register tất cả guidance objects trong scene
            RegisterAllGuidanceObjects();
            
            // Khởi tạo guidance cho event đầu tiên
            InitializeFirstGuidance();
        }

        private void Update()
        {
            if (enableGuidance && !string.IsNullOrEmpty(currentTargetEventId))
            {
                CheckShowGuidance();
            }
        }

        /// <summary>
        /// Tìm tất cả GuidanceObject trong scene và register theo eventId
        /// </summary>
        private void RegisterAllGuidanceObjects()
        {
            var guidanceComponents = FindObjectsByType<GuidanceObject>(FindObjectsSortMode.None);
            
            foreach (var guidance in guidanceComponents)
            {
                if (!string.IsNullOrEmpty(guidance.EventId))
                {
                    guidanceObjects[guidance.EventId] = guidance.gameObject;
                    guidance.gameObject.SetActive(false); // Ẩn tất cả guidance ban đầu
                }
            }
            
            Debug.Log($"[SimpleGuidanceManager] Registered {guidanceObjects.Count} guidance objects");
        }

        /// <summary>
        /// Khởi tạo guidance cho event đầu tiên trong sequence
        /// </summary>
        private void InitializeFirstGuidance()
        {
            // Lấy event sequence từ EventManager
            if (EventManager.Instance == null) return;
            
            // Tìm event đầu tiên chưa completed
            var firstIncompleteEvent = FindNextIncompleteEvent();
            if (!string.IsNullOrEmpty(firstIncompleteEvent))
            {
                SetCurrentTarget(firstIncompleteEvent);
            }
        }

        /// <summary>
        /// Tìm event tiếp theo chưa hoàn thành
        /// </summary>
        private string FindNextIncompleteEvent()
        {
            // Duyệt qua tất cả guidance objects để tìm event có thể trigger
            foreach (var eventId in guidanceObjects
                         .Select(kvp => kvp.Key)
                         .Where(eventId 
                             => ProgressionManager.Instance.CanTrigger(eventId) 
                                || ProgressionManager.Instance.IsWaitingForEvent(eventId)))
            {
                return eventId;
            }

            return "";
        }

        /// <summary>
        /// Set target hiện tại và reset timer
        /// </summary>
        private void SetCurrentTarget(string eventId)
        {
            currentTargetEventId = eventId;
            lastInteractionTime = Time.time;
            
            Debug.Log($"[SimpleGuidanceManager] Set current target: {eventId}");
        }

        /// <summary>
        /// Kiểm tra và hiển thị guidance nếu đủ thời gian
        /// </summary>
        private void CheckShowGuidance()
        {
            if (Time.time - lastInteractionTime >= guidanceActivationDelay)
            {
                ShowGuidanceForCurrentTarget();
            }
        }

        /// <summary>
        /// Hiển thị guidance cho event hiện tại
        /// </summary>
        private void ShowGuidanceForCurrentTarget()
        {
            if (string.IsNullOrEmpty(currentTargetEventId)) return;

            if (!guidanceObjects.TryGetValue(currentTargetEventId, out var guidanceObj)) return;
            if (guidanceObj.activeInHierarchy) return;
            guidanceObj.SetActive(true);
            Debug.Log($"[SimpleGuidanceManager] Showing guidance for: {currentTargetEventId}");
        }

        /// <summary>
        /// Ẩn guidance cho event cụ thể
        /// </summary>
        private void HideGuidance(string eventId)
        {
            if (guidanceObjects.TryGetValue(eventId, out var guidanceObj))
            {
                guidanceObj.SetActive(false);
                Debug.Log($"[SimpleGuidanceManager] Hidden guidance for: {eventId}");
            }
        }

        /// <summary>
        /// Được gọi khi một event hoàn thành
        /// </summary>
        private void OnEventCompleted(object data)
        {
            var eventData = data as BaseEventData;
            if (eventData == null) return;
            
            var completedEventId = eventData.eventId;
            
            // Ẩn guidance của event vừa completed
            HideGuidance(completedEventId);
            
            // Tìm và set target cho event tiếp theo
            var nextEvent = FindNextIncompleteEvent();
            if (!string.IsNullOrEmpty(nextEvent))
            {
                SetCurrentTarget(nextEvent);
            }
            else
            {
                currentTargetEventId = "";
                Debug.Log("[SimpleGuidanceManager] No more events to guide");
            }
        }

        /// <summary>
        /// Gọi khi player tương tác với TriggerZone
        /// </summary>
        public void OnPlayerInteraction(string eventId)
        {
            lastInteractionTime = Time.time;
            HideGuidance(eventId);
            
            Debug.Log($"[SimpleGuidanceManager] Player interacted with: {eventId}");
        }

        /// <summary>
        /// Enable/disable guidance system
        /// </summary>
        public void SetGuidanceEnabled(bool enabled)
        {
            enableGuidance = enabled;
            if (enabled) return;
            // Ẩn tất cả guidance
            foreach (var guidanceObj in guidanceObjects.Values)
            {
                guidanceObj.SetActive(false);
            }
        }

        /// <summary>
        /// Force hiển thị guidance cho event cụ thể (dùng cho debug)
        /// </summary>
        public void ForceShowGuidance(string eventId)
        {
            HideAllGuidance();
            SetCurrentTarget(eventId);
            ShowGuidanceForCurrentTarget();
        }

        /// <summary>
        /// Ẩn tất cả guidance
        /// </summary>
        private void HideAllGuidance()
        {
            foreach (var guidanceObj in guidanceObjects.Values)
            {
                guidanceObj.SetActive(false);
            }
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
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Current Target: {currentTargetEventId}");
            GUILayout.Label($"Time since interaction: {Time.time - lastInteractionTime:F1}s");
            GUILayout.Label($"Guidance Objects: {guidanceObjects.Count}");
            
            if (GUILayout.Button("Show Next Guidance"))
            {
                ShowGuidanceForCurrentTarget();
            }
            
            GUILayout.EndArea();
        }
#endif
    }
}
