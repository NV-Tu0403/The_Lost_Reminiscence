using UnityEngine;
using DG.Tweening;
using _MyGame.Codes.Trigger;

namespace _MyGame.Codes.Guidance
{
    /// <summary>
    /// Component gắn vào GameObject để tạo hiệu ứng guidance (mũi tên, glow, etc.)
    /// Tự động đăng ký với GuidanceManager khi Start()
    /// </summary>
    public class GuideObject : MonoBehaviour
    {
        [Header("Guide Settings")]
        [SerializeField] private string eventId; // EventId tương ứng với TriggerZone
        [SerializeField] private bool autoGetEventIdFromParent = true; // Tự động lấy eventId từ TriggerZone cha
        [SerializeField] private bool autoRegisterOnStart = true; // Tự động đăng ký với GuidanceManager
        
        [Header("Animation Settings")]
        [SerializeField] private bool useFloatingAnimation = true;
        [SerializeField] private float floatDistance = 1f; // Khoảng cách di chuyển lên xuống
        [SerializeField] private float floatDuration = 1f; // Thời gian di chuyển một chiều
        [SerializeField] private Ease floatEase = Ease.InOutSine;
        
        [Header("Show/Hide Animation")]
        [SerializeField] private bool useScaleAnimation = true;
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Tween floatTween;
        private Tween scaleTween;
        private bool isVisible;

        public string EventId => eventId;

        private void Start()
        {
            Setup();
            
            // Tự động lấy eventId từ TriggerZone cha nếu được bật
            if (autoGetEventIdFromParent)
            {
                AutoGetEventIdFromParent();
            }
            
            if (autoRegisterOnStart && GuidanceManager.Instance != null)
            {
                GuidanceManager.Instance.RegisterGuide(eventId, this);
            }
        }

        /// <summary>
        /// Tự động lấy eventId từ TriggerZone parent
        /// </summary>
        private void AutoGetEventIdFromParent()
        {
            // Tìm TriggerZone trong parent hierarchy
            var triggerZone = GetComponentInParent<TriggerZone>();
            if (triggerZone != null && !string.IsNullOrEmpty(triggerZone.eventId))
            {
                eventId = triggerZone.eventId;
                Debug.Log($"[GuideObject] Auto-setup eventId from parent TriggerZone: {eventId}");
            }
            else
            {
                Debug.LogWarning($"[GuideObject] No TriggerZone parent found or parent has empty eventId for {gameObject.name}");
            }
        }

        private void Setup()
        {
            originalPosition = transform.position;
            originalScale = transform.localScale;
            
            // Ẩn object ban đầu
            if (useScaleAnimation)
            {
                transform.localScale = Vector3.zero;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Hiển thị guide object với animation
        /// </summary>
        public void Show()
        {
            if (isVisible) return;
            
            isVisible = true;
            gameObject.SetActive(true);
            
            // Stop các animation đang chạy
            StopAllAnimations();
            
            if (useScaleAnimation)
            {
                // Scale animation
                transform.localScale = Vector3.zero;
                scaleTween = transform.DOScale(originalScale, showDuration)
                    .SetEase(showEase)
                    .OnComplete(StartFloatingAnimation);
            }
            else
            {
                StartFloatingAnimation();
            }
        }

        /// <summary>
        /// Ẩn guide object với animation
        /// </summary>
        public void Hide()
        {
            if (!isVisible) return;
            
            isVisible = false;
            
            // Stop floating animation
            StopFloatingAnimation();
            
            if (useScaleAnimation)
            {
                scaleTween = transform.DOScale(Vector3.zero, hideDuration)
                    .SetEase(hideEase)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Bắt đầu animation floating
        /// </summary>
        private void StartFloatingAnimation()
        {
            if (!useFloatingAnimation || !isVisible) return;
            
            // Reset position
            transform.position = originalPosition;
            
            // Tạo floating animation
            floatTween = transform.DOMoveY(originalPosition.y + floatDistance, floatDuration)
                .SetEase(floatEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Dừng animation floating
        /// </summary>
        private void StopFloatingAnimation()
        {
            if (floatTween != null)
            {
                floatTween.Kill();
                floatTween = null;
            }
            
            // Reset position
            transform.position = originalPosition;
        }

        /// <summary>
        /// Dừng tất cả animations
        /// </summary>
        private void StopAllAnimations()
        {
            StopFloatingAnimation();

            if (scaleTween == null) return;
            scaleTween.Kill();
            scaleTween = null;
        }

        /// <summary>
        /// Set eventId (dùng để setup từ code)
        /// </summary>
        public void SetEventId(string newEventId)
        {
            eventId = newEventId;
        }

        /// <summary>
        /// Toggle visibility
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }

        private void OnDestroy()
        {
            StopAllAnimations();
            
            // Unregister from GuidanceManager
            if (GuidanceManager.Instance != null)
            {
                GuidanceManager.Instance.UnregisterGuide(eventId);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values
            floatDistance = Mathf.Max(0.1f, floatDistance);
            floatDuration = Mathf.Max(0.1f, floatDuration);
            showDuration = Mathf.Max(0.1f, showDuration);
            hideDuration = Mathf.Max(0.1f, hideDuration);
            
            // Tự động lấy eventId từ parent trong editor nếu được bật
            if (autoGetEventIdFromParent && Application.isPlaying == false)
            {
                var triggerZone = GetComponentInParent<TriggerZone>();
                if (triggerZone != null && !string.IsNullOrEmpty(triggerZone.eventId))
                {
                    eventId = triggerZone.eventId;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Hiển thị preview floating range trong Scene view
            Vector3 basePos = Application.isPlaying ? originalPosition : transform.position;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(basePos, Vector3.one * 0.5f);
            
            if (useFloatingAnimation)
            {
                // Vẽ floating range
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(basePos + Vector3.up * floatDistance, Vector3.one * 0.3f);
                Gizmos.DrawLine(basePos, basePos + Vector3.up * floatDistance);
            }
            
            // Hiển thị eventId
            UnityEditor.Handles.Label(basePos + Vector3.up * (floatDistance + 1f), 
                $"Guide: {eventId}\nFloat: {floatDistance}u in {floatDuration}s");
        }
        #endif
    }
}
