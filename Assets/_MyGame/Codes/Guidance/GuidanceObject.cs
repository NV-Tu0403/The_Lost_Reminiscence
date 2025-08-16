using _MyGame.Codes.Trigger;
using UnityEngine;
using DG.Tweening;

namespace _MyGame.Codes.SimpleGuidance
{
    /// <summary>
    /// Component gắn vào GameObject mũi tên guidance
    /// Tạo hiệu ứng di chuyển lên xuống thay vì animation
    /// </summary>
    public class GuidanceObject : MonoBehaviour
    {
        [Header("Guidance Settings")]
        [SerializeField] private string eventId; // EventId tương ứng với TriggerZone
        [SerializeField] private bool autoGetEventIdFromParent = true;
        
        [Header("Movement Animation")]
        [SerializeField] private float moveDistance = 1f; // Khoảng cách di chuyển lên xuống
        [SerializeField] private float moveDuration = 1f; // Thời gian di chuyển một chiều
        [SerializeField] private Ease moveEase = Ease.InOutSine; // Kiểu easing
        [SerializeField] private bool startOnEnable = true; // Tự động bắt đầu khi được kích hoạt

        private Vector3 originalPosition;
        private Tween moveTween;

        public string EventId => eventId;

        private void Start()
        {
            if (autoGetEventIdFromParent)
            {
                AutoSetupEventId();
            }
            
            // Lưu vị trí gốc
            originalPosition = transform.position;
        }

        /// <summary>
        /// Tự động lấy eventId từ TriggerZone parent
        /// </summary>
        private void AutoSetupEventId()
        {
            // Tìm TriggerZone trong parent hoặc cùng GameObject
            var triggerZone = GetComponentInParent<TriggerZone>();
            if (triggerZone != null)
            {
                eventId = triggerZone.eventId;
                Debug.Log($"[GuidanceObject] Auto-setup eventId: {eventId}");
            }
        }

        /// <summary>
        /// Được gọi khi GameObject được SetActive(true)
        /// </summary>
        private void OnEnable()
        {
            if (startOnEnable)
            {
                StartFloatingAnimation();
            }
        }

        /// <summary>
        /// Bắt đầu animation di chuyển lên xuống
        /// </summary>
        public void StartFloatingAnimation()
        {
            // Dừng animation cũ nếu có
            StopFloatingAnimation();
            
            // Đặt về vị trí gốc
            transform.position = originalPosition;
            
            // Tạo animation di chuyển lên xuống vô hạn
            moveTween = transform.DOLocalMoveY(originalPosition.y + moveDistance, moveDuration)
                .SetEase(moveEase)
                .SetLoops(-1, LoopType.Yoyo); // Yoyo = lên xuống liên tục
        }

        /// <summary>
        /// Dừng animation di chuyển
        /// </summary>
        public void StopFloatingAnimation()
        {
            if (moveTween != null)
            {
                moveTween.Kill();
                moveTween = null;
            }
        }

        /// <summary>
        /// Ẩn guidance với animation (nếu có)
        /// </summary>
        public void HideWithAnimation()
        {
            StopFloatingAnimation();
            
            // Animation fade out trước khi ẩn
            transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => gameObject.SetActive(false));
        }

        /// <summary>
        /// Hiển thị guidance với animation
        /// </summary>
        public void ShowWithAnimation()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            
            // Animation scale up
            transform.DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => StartFloatingAnimation());
        }

        private void OnDisable()
        {
            StopFloatingAnimation();
        }

        private void OnDestroy()
        {
            StopFloatingAnimation();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Tự động setup eventId trong editor
            if (string.IsNullOrEmpty(eventId) && autoGetEventIdFromParent)
            {
                AutoSetupEventId();
            }
            
            // Clamp values
            moveDistance = Mathf.Max(0.1f, moveDistance);
            moveDuration = Mathf.Max(0.1f, moveDuration);
        }

        private void OnDrawGizmosSelected()
        {
            // Hiển thị preview movement range trong Scene view
            Vector3 basePos = Application.isPlaying ? originalPosition : transform.position;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(basePos, Vector3.one * 0.5f);
            
            // Vẽ range di chuyển
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(basePos + Vector3.up * moveDistance, Vector3.one * 0.3f);
            Gizmos.DrawLine(basePos, basePos + Vector3.up * moveDistance);
            
            // Hiển thị eventId
            UnityEditor.Handles.Label(basePos + Vector3.up * (moveDistance + 1f), 
                $"Guidance: {eventId}\nMove: {moveDistance}u in {moveDuration}s");
        }
#endif
    }
}
