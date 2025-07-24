using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                
                // Set colors
                var fillImage = healthSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = uiConfig.bossHealthColor;
                }
            }
        }


        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.HealthChanged, OnHealthChanged);
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChanged);
        }

        private void OnHealthChanged(BossEventData data)
        {
            int currentHealth = data.intValue;
            int maxHealth = (int)data.floatValue;
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }

        private void OnPhaseChanged(BossEventData data)
        {
            int newPhase = data.intValue;
            
            if (phaseText != null)
            {
                phaseText.text = $"Phase {newPhase}";
            }
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.HealthChanged, OnHealthChanged);
            BossEventSystem.Unsubscribe(BossEventType.PhaseChanged, OnPhaseChanged);
        }
    }
}