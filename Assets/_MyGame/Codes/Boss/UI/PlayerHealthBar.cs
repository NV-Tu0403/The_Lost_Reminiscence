using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Code.Boss;

namespace _MyGame.Codes.Boss.UI
{
    /// <summary>
    /// Thanh máu người chơi ở giữa dưới màn hình
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        
        private int maxHealth = 3; // Default player health
        private int currentHealth = 3;
        private BossConfig bossConfig;
        private UIConfig uiConfig;
        private Coroutine healthAnimationCoroutine;
        private Image fillImage;

        public void Initialize(int playerMaxHealth, BossConfig config)
        {
            bossConfig = config;
            uiConfig = config?.uiConfig;
            maxHealth = playerMaxHealth;
            currentHealth = playerMaxHealth;
            
            SetupUI();
            RegisterEvents();
        }

        private void SetupUI()
        {
            // Setup health slider
            if (healthSlider == null) 
            {
                Debug.LogError("[PlayerHealthBar] HealthSlider is NULL!");
                return;
            }
            
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            fillImage = healthSlider.fillRect?.GetComponent<Image>();
            
            if (fillImage != null && uiConfig != null)
            {
                fillImage.color = uiConfig.playerHealthColor;
            }
            
            // Setup health text
            if (healthText != null)
            {
                healthText.color = Color.black;
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
            else
            {
                Debug.LogError("[PlayerHealthBar] HealthText is NULL!");
            }
        }

        private void RegisterEvents()
        {

            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeated);
            BossEventSystem.Subscribe(BossEventType.PlayerHealthReset, OnPlayerHealthReset);
            
            // Subscribe vào BossGameManager event để update UI
            if (BossGameManager.Instance != null)
            {
                BossGameManager.Instance.OnPlayerHealthChanged += OnPlayerHealthChangedFromManager;
            }
        }
        
        // Chỉ cập nhật UI dựa trên current health từ BossGameManager
        private void OnPlayerHealthChangedFromManager(int currentHealth)
        {
            // BossGameManager đã trừ máu rồi, chúng ta chỉ cập nhật UI với giá trị hiện tại
            this.currentHealth = currentHealth;
            if (healthSlider != null)
            {
                AnimateHealthChange(currentHealth);
            }
            UpdateHealthText();
            // Không cần check defeated ở đây vì BossGameManager đã xử lý
        }

        private void UpdateHealthText()
        {
            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }

        private void OnPlayerDefeated()
        {
            Debug.Log("[PlayerHealthBar] Player Defeated!");
            // Trigger PlayerDefeated event để GameManager xử lý
            BossEventSystem.Trigger(BossEventType.PlayerDefeated);
        }

        private void OnBossDefeated(BossEventData data)
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnBossDefeated);
            BossEventSystem.Unsubscribe(BossEventType.PlayerHealthReset, OnPlayerHealthReset);
            
            // Unsubscribe từ BossGameManager event
            if (BossGameManager.Instance != null)
            {
                BossGameManager.Instance.OnPlayerHealthChanged -= OnPlayerHealthChangedFromManager;
            }
        }

        private void OnPlayerHealthReset(BossEventData data)
        {
            int newMaxHealth = data.intValue;
            currentHealth = newMaxHealth;
            maxHealth = newMaxHealth;
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
            UpdateHealthText();
            Debug.Log($"[PlayerHealthBar] Health reset to {currentHealth}/{maxHealth}");
        }

        private void AnimateHealthChange(int newHealth)
        {
            if (healthSlider == null || bossConfig == null) return;
            
            // Kiểm tra GameObject có active không trước khi chạy coroutine
            if (!gameObject.activeInHierarchy)
            {
                // Nếu GameObject không active, set value trực tiếp
                healthSlider.value = newHealth;
                return;
            }
            
            var duration = bossConfig.uiConfig.uiAnimationSpeed;
            var curve = bossConfig.uiConfig.uiAnimationCurve;
            var startValue = healthSlider.value;
            float endValue = newHealth;
            
            // Stop existing coroutines before starting new one
            if (healthAnimationCoroutine != null)
            {
                StopCoroutine(healthAnimationCoroutine);
            }
            
            healthAnimationCoroutine = StartCoroutine(AnimateSliderCoroutine(startValue, endValue, duration, curve));
        }

        private IEnumerator AnimateSliderCoroutine(float start, float end, float duration, AnimationCurve curve)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var value = Mathf.Lerp(start, end, curve.Evaluate(t));
                healthSlider.value = value;
                yield return null;
            }
            healthSlider.value = end;
        }
    }
}
