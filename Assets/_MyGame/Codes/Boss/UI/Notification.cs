using _MyGame.Codes.Boss.CoreSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Code.Boss
{
    public class Notification : MonoBehaviour
    {
        [Header("UI Components (Auto-find if not assigned)")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform panelTransform;
        
        [Header("Animation Settings")]
        [SerializeField] private float showDuration = 3f;
        [SerializeField] private float fadeInTime = 0.5f;
        [SerializeField] private float fadeOutTime = 0.5f;
        [SerializeField] private float scaleAnimationTime = 0.3f;
        
        [Header("Text Settings")]
        [SerializeField] private string defeatMessage = "";
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);

        [Header("Setup Options")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool hideOnStart = true;

        private Sequence animationSequence;

        private void Awake()
        {
            // Setup panel transform reference
            if (panelTransform == null)
                panelTransform = GetComponent<RectTransform>();

            if (autoFindComponents)
            {
                AutoFindComponents();
            }

            SetupInitialState();
        }

        private void AutoFindComponents()
        {
            // Auto-find CanvasGroup - prioritize this GameObject, then children, then parent
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = GetComponentInChildren<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = GetComponentInParent<CanvasGroup>();
            }

            // Auto-find Text component
            if (notificationText == null)
            {
                notificationText = GetComponentInChildren<TextMeshProUGUI>();
            }

            // Auto-find Background Image
            if (backgroundImage == null)
            {
                // Tìm Image component, ưu tiên Image có tên chứa "background", "panel", hoặc "bg"
                var images = GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    string imgName = img.name.ToLower();
                    if (imgName.Contains("background") || imgName.Contains("panel") || imgName.Contains("bg"))
                    {
                        backgroundImage = img;
                        break;
                    }
                }
                
                // Nếu không tìm thấy, lấy Image đầu tiên
                if (backgroundImage == null && images.Length > 0)
                {
                    backgroundImage = images[0];
                }
            }

            // Log setup status
            LogSetupStatus();
        }

        private void LogSetupStatus()
        {
            Debug.Log($"[BossDefeatNotification] Setup Status:" +
                     $"\n- CanvasGroup: {(canvasGroup != null ? "✓" : "✗")}" +
                     $"\n- Text: {(notificationText != null ? "✓" : "✗")}" +
                     $"\n- Background: {(backgroundImage != null ? "✓" : "✗")}" +
                     $"\n- Panel Transform: {(panelTransform != null ? "✓" : "✗")}");
        }

        private void SetupInitialState()
        {
            // Setup text
            if (notificationText != null)
            {
                notificationText.text = defeatMessage;
                notificationText.color = textColor;
            }

            // Setup background
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            // Setup CanvasGroup
            if (canvasGroup != null)
            {
                if (hideOnStart)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }

            // Setup initial scale
            if (panelTransform != null && hideOnStart)
            {
                panelTransform.localScale = Vector3.zero;
            }
        }

        private void Start()
        {
            // Ensure proper initial state
            if (hideOnStart)
            {
                SetVisible(false);
            }
        }

        private void OnEnable()
        {
            BossEventSystem.Subscribe(BossEventType.ShowDefeatNotification, OnShowDefeatNotification);
        }

        private void OnDisable()
        {
            BossEventSystem.Unsubscribe(BossEventType.ShowDefeatNotification, OnShowDefeatNotification);
            
            // Clean up any running animations
            CleanupAnimation();
        }

        private void CleanupAnimation()
        {
            if (animationSequence != null)
            {
                animationSequence.Kill();
                animationSequence = null;
            }
        }

        private void OnShowDefeatNotification(BossEventData data)
        {
            var message = defeatMessage;
            if (data != null && !string.IsNullOrEmpty(data.stringValue))
            {
                message = data.stringValue;
            }
            ShowNotification(message);
        }

        public void ShowNotification()
        {
            ShowNotification(defeatMessage);
        }

        private void ShowNotification(string message)
        {
            if (!ValidateComponents())
            {
                Debug.LogWarning("[BossDefeatNotification] Cannot show notification - missing required components!");
                return;
            }

            // Kill any existing animation
            CleanupAnimation();

            // Update message if provided
            if (notificationText != null && !string.IsNullOrEmpty(message))
            {
                notificationText.text = message;
            }

            // Create animation sequence
            animationSequence = DOTween.Sequence();

            // Set initial state
            SetupAnimationInitialState();

            // Build animation sequence
            BuildAnimationSequence();

            Debug.Log("[BossDefeatNotification] Showing defeat notification: " + message);
        }

        private bool ValidateComponents()
        {
            var isValid = panelTransform != null;
            
            if (!isValid)
            {
                Debug.LogError("[BossDefeatNotification] Missing RectTransform component!");
            }

            return isValid;
        }

        private void SetupAnimationInitialState()
        {
            // Initial visibility state
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                // Fallback: use GameObject.SetActive if no CanvasGroup
                gameObject.SetActive(true);
            }

            // Initial scale
            if (panelTransform != null)
            {
                panelTransform.localScale = Vector3.zero;
            }
        }

        private void BuildAnimationSequence()
        {
            // Scale animation with bounce effect
            if (panelTransform != null)
            {
                animationSequence
                    .Append(panelTransform.DOScale(Vector3.one * 1.1f, scaleAnimationTime)
                        .SetEase(Ease.OutBack))
                    .Append(panelTransform.DOScale(Vector3.one, scaleAnimationTime * 0.5f)
                        .SetEase(Ease.InOutQuad));
            }

            // Fade animation
            if (canvasGroup != null)
            {
                if (panelTransform != null)
                {
                    // Join fade with scale animation
                    animationSequence.Join(canvasGroup.DOFade(1f, fadeInTime));
                }
                else
                {
                    // Standalone fade animation
                    animationSequence.Append(canvasGroup.DOFade(1f, fadeInTime));
                }
            }

            // Wait for display duration
            animationSequence.AppendInterval(showDuration - fadeInTime - fadeOutTime);

            // Fade out animation
            if (canvasGroup != null)
            {
                animationSequence.Append(canvasGroup.DOFade(0f, fadeOutTime));
            }

            // Scale down during fade out
            if (panelTransform != null)
            {
                animationSequence.Join(panelTransform.DOScale(Vector3.one * 0.9f, fadeOutTime)
                    .SetEase(Ease.InQuad));
            }

            // On complete callback
            animationSequence.OnComplete(OnAnimationComplete);
        }

        private void OnAnimationComplete()
        {
            SetVisible(false);
            CleanupAnimation();
        }

        public void HideNotification()
        {
            CleanupAnimation();

            // Quick fade out animation
            animationSequence = DOTween.Sequence();

            if (canvasGroup != null)
            {
                animationSequence.Append(canvasGroup.DOFade(0f, 0.2f));
            }

            if (panelTransform != null)
            {
                animationSequence.Join(panelTransform.DOScale(Vector3.zero, 0.2f));
            }

            animationSequence.OnComplete(() => {
                SetVisible(false);
                CleanupAnimation();
            });
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }

            if (panelTransform != null && !visible)
            {
                panelTransform.localScale = Vector3.zero;
            }
        }

        // Public methods for customization
        public void SetMessage(string message)
        {
            defeatMessage = message;
            if (notificationText != null)
                notificationText.text = message;
        }

        public void SetColors(Color textCol, Color backgroundCol)
        {
            textColor = textCol;
            backgroundColor = backgroundCol;
            
            if (notificationText != null)
                notificationText.color = textColor;
            
            if (backgroundImage != null)
                backgroundImage.color = backgroundColor;
        }

        public void SetDuration(float duration)
        {
            showDuration = duration;
        }

        // Method để manual setup components nếu cần
        public void ManualSetup(CanvasGroup canvas, TextMeshProUGUI text, Image background)
        {
            canvasGroup = canvas;
            notificationText = text;
            backgroundImage = background;
            autoFindComponents = false;
            
            SetupInitialState();
            LogSetupStatus();
        }
    }
}
