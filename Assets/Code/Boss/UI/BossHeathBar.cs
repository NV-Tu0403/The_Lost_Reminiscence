using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Code.Boss
{
    /// <summary>
    /// Thanh máu của Boss ở giữa trên màn hình
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI phaseText;
        
        private BossController bossController;
        private UIConfig uiConfig;
        private Coroutine healthAnimationCoroutine;
        private Image fillImage;

        public void Initialize(BossController controller)
        {
            bossController = controller;
            uiConfig = controller.Config.uiConfig;
            
            SetupUI();
            RegisterEvents();
        }

        private void SetupUI()
        {
            // Setup health slider
            if (healthSlider != null)
            {
                healthSlider.maxValue = bossController.Config.maxHealthPerPhase;
                healthSlider.value = bossController.Config.maxHealthPerPhase;
                
                // Get fill image reference for animations
                fillImage = healthSlider.fillRect?.GetComponent<Image>();
                if (fillImage != null && uiConfig != null)
                {
                    fillImage.color = uiConfig.bossHealthColor;
                }
            }
        }

        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.HealthChanged, OnHealthChanged);
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChanged);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeated);
        }

        private void OnHealthChanged(BossEventData data)
        {
            var currentHealth = data.intValue;
            var maxHealth = (int)data.floatValue;
            
            // Update text immediately
            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }

            // Start smooth health bar animation
            if (healthAnimationCoroutine != null)
            {
                StopCoroutine(healthAnimationCoroutine);
            }
            healthAnimationCoroutine = StartCoroutine(AnimateHealthBarSmooth(currentHealth, maxHealth));
        }

        private IEnumerator AnimateHealthBarSmooth(int currentHealth, int maxHealth)
        {
            if (healthSlider == null || uiConfig == null) yield break;
            
            var startValue = healthSlider.value;
            float targetValue = currentHealth;
            var elapsed = 0f;
            var duration = uiConfig.uiAnimationSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Sử dụng animation curve từ config để smooth hơn
                float smoothT = uiConfig.uiAnimationCurve.Evaluate(t);
                healthSlider.value = Mathf.Lerp(startValue, targetValue, smoothT);
                
                yield return null;
            }

            healthSlider.value = targetValue;
        }

        private void OnPhaseChanged(BossEventData data)
        {
            int newPhase = data.intValue;
            
            if (phaseText != null)
            {
                phaseText.text = $"Phase {newPhase}";
            }
        }

        private void OnBossDefeated(BossEventData data)
        {
            // Hide boss health bar and phase name UI
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.HealthChanged, OnHealthChanged);
            BossEventSystem.Unsubscribe(BossEventType.PhaseChanged, OnPhaseChanged);
        }
    }
}