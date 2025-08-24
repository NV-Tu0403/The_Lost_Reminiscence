using _MyGame.Codes.Boss.CoreSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Code.Boss
{
   /// <summary>
    /// Thanh cast skill ở dưới thanh máu boss
    /// </summary>
    public class BossSkillCastBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider castSlider;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private GameObject castBarContainer;
        
        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [SerializeField] private Ease slideInEase = Ease.OutBack;
        [SerializeField] private Ease slideOutEase = Ease.InBack;
        
        private UIConfig uiConfig;
        private bool isVisible = false;
        
        private Coroutine castAnimationCoroutine;
        private Vector2 originalPosition;
        private Vector2 hiddenPosition;
        private RectTransform rectTransform;
        private Tween currentTween;

        public void Initialize(BossController controller)
        {
            uiConfig = controller.Config.uiConfig;
            
            SetupUI();
            SetupAnimation();
            RegisterEvents();
            
            // Hide initially
            SetVisible(false);
        }

        private void SetupUI()
        {
            // Setup cast slider
            if (castSlider == null) return;
            castSlider.maxValue = 1f;
            castSlider.value = 0f;
                
            // Set colors
            var fillImage = castSlider.fillRect.GetComponent<Image>();
            if (fillImage != null && uiConfig != null)
            {
                fillImage.color = uiConfig.skillCastColor;
            }
        }

        private void SetupAnimation()
        {
            // Get RectTransform for animation
            rectTransform = castBarContainer?.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // Store original position (current position in scene)
            originalPosition = rectTransform.anchoredPosition;
            
            // Calculate hidden position (off-screen to the left)
            hiddenPosition = originalPosition + Vector2.left * (rectTransform.rect.width + 100f);
            
            // Start at hidden position
            rectTransform.anchoredPosition = hiddenPosition;
        }

        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.SkillCasted, OnSkillCasted);
            BossEventSystem.Subscribe(BossEventType.SkillCastProgress, OnSkillCastProgress);
            BossEventSystem.Subscribe(BossEventType.SkillInterrupted, OnSkillInterrupted);
            BossEventSystem.Subscribe(BossEventType.StateChanged, OnStateChanged);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeated);
        }

        private void OnSkillCasted(BossEventData data)
        {
            Debug.Log($"[BossSkillCastBar] OnSkillCasted triggered - Skill: {data?.stringValue}");
            SetVisible(true);
            
            if (skillNameText != null && data != null)
            {
                skillNameText.text = data.stringValue ?? "Casting Skill...";
                Debug.Log($"[BossSkillCastBar] Skill name set to: {skillNameText.text}");
            }
            else
            {
                Debug.LogWarning("[BossSkillCastBar] skillNameText is null or data is null");
            }
            
            if (castSlider != null)
            {
                castSlider.value = 0f;
                Debug.Log("[BossSkillCastBar] Cast slider reset to 0");
            }
            else
            {
                Debug.LogWarning("[BossSkillCastBar] castSlider is null");
            }
        }

        private void OnSkillCastProgress(BossEventData data)
        {
            if (castSlider != null && isVisible) castSlider.value = data.floatValue;
        }

        private void OnSkillInterrupted(BossEventData data)
        {
            Debug.Log("[BossSkillCastBar] OnSkillInterrupted called");
            SetVisible(false);
        }

        private void OnStateChanged(BossEventData data)
        {
            if (isVisible && data?.stringValue != "ScreamState" 
                          && data?.stringValue != "FearZoneState" 
                          && data?.stringValue != "DecoyState"
                          && data?.stringValue != "SoulState")
            {
                SetVisible(false);
            }
        }

        private void OnBossDefeated(BossEventData data)
        {
            // Hide boss skill cast bar UI
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            isVisible = visible;
            if (castBarContainer == null || rectTransform == null) return;

            // Kill any existing tween
            currentTween?.Kill();
            
            // Always make sure the container is active for animation
            castBarContainer.SetActive(true);

            if (visible)
            {
                // Slide in from left - Start at hidden position, animate to original position
                rectTransform.anchoredPosition = hiddenPosition;
                currentTween = DOTween.To(() => rectTransform.anchoredPosition, 
                                        x => rectTransform.anchoredPosition = x, 
                                        originalPosition, slideInDuration)
                    .SetEase(slideInEase)
                    .OnComplete(() => {
                        Debug.Log("[BossSkillCastBar] Slide in animation completed");
                    });
            }
            else
            {
                // Slide out to left - Start at current position, animate to hidden position
                currentTween = DOTween.To(() => rectTransform.anchoredPosition, 
                                        x => rectTransform.anchoredPosition = x, 
                                        hiddenPosition, slideOutDuration)
                    .SetEase(slideOutEase)
                    .OnComplete(() => {
                        castBarContainer.SetActive(false);
                        Debug.Log("[BossSkillCastBar] Slide out animation completed");
                    });
            }
        }

        private void OnDestroy()
        {
            // Kill any active tweens
            currentTween?.Kill();
            
            BossEventSystem.Unsubscribe(BossEventType.SkillCasted, OnSkillCasted);
            BossEventSystem.Unsubscribe(BossEventType.SkillCastProgress, OnSkillCastProgress);
            BossEventSystem.Unsubscribe(BossEventType.SkillInterrupted, OnSkillInterrupted);
            BossEventSystem.Unsubscribe(BossEventType.StateChanged, OnStateChanged);
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnBossDefeated);
        }
    }
}