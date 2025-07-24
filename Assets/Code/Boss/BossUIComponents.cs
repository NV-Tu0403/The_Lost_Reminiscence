using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Code.Boss
{
    /// <summary>
    /// Thanh máu của Boss ở giữa trên màn hình
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Canvas canvas;
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
            // Create canvas if not assigned
            if (canvas == null)
            {
                CreateCanvas();
            }
            
            // Position at top center of screen
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -50); // 50 pixels from top
            rectTransform.sizeDelta = uiConfig.healthBarSize;
            
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

        private void CreateCanvas()
        {
            GameObject canvasGO = new GameObject("BossHealthCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
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

    /// <summary>
    /// Thanh cast skill ở dưới thanh máu boss
    /// </summary>
    public class BossSkillCastBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider castSlider;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private GameObject castBarContainer;
        
        private UIConfig uiConfig;
        private bool isVisible = false;

        public void Initialize(BossController controller)
        {
            uiConfig = controller.Config.uiConfig;
            
            SetupUI();
            RegisterEvents();
            
            // Hide initially
            SetVisible(false);
        }

        private void SetupUI()
        {
            // Position below boss health bar
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -100); // Below health bar
            rectTransform.sizeDelta = uiConfig.skillCastBarSize;
            
            // Setup cast slider
            if (castSlider != null)
            {
                castSlider.maxValue = 1f;
                castSlider.value = 0f;
                
                // Set colors
                var fillImage = castSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = uiConfig.skillCastColor;
                }
            }
        }

        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.SkillCasted, OnSkillCasted);
            BossEventSystem.Subscribe(BossEventType.SkillCastProgress, OnSkillCastProgress);
            BossEventSystem.Subscribe(BossEventType.SkillInterrupted, OnSkillInterrupted);
            BossEventSystem.Subscribe(BossEventType.StateChanged, OnStateChanged);
        }

        private void OnSkillCasted(BossEventData data)
        {
            SetVisible(true);
            
            if (skillNameText != null && data != null)
            {
                skillNameText.text = data.stringValue ?? "Casting Skill...";
            }
            
            if (castSlider != null)
            {
                castSlider.value = 0f;
            }
        }

        private void OnSkillCastProgress(BossEventData data)
        {
            if (castSlider != null && isVisible)
            {
                castSlider.value = data.floatValue;
            }
        }

        private void OnSkillInterrupted(BossEventData data)
        {
            SetVisible(false);
        }

        private void OnStateChanged(BossEventData data)
        {
            // Hide cast bar when state changes (skill completed)
            if (isVisible)
            {
                SetVisible(false);
            }
        }

        private void SetVisible(bool visible)
        {
            isVisible = visible;
            if (castBarContainer != null)
            {
                castBarContainer.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.SkillCasted, OnSkillCasted);
            BossEventSystem.Unsubscribe(BossEventType.SkillCastProgress, OnSkillCastProgress);
            BossEventSystem.Unsubscribe(BossEventType.SkillInterrupted, OnSkillInterrupted);
            BossEventSystem.Unsubscribe(BossEventType.StateChanged, OnStateChanged);
        }
    }

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

        public void Initialize(int playerMaxHealth)
        {
            maxHealth = playerMaxHealth;
            currentHealth = playerMaxHealth;
            
            SetupUI();
            RegisterEvents();
        }

        private void SetupUI()
        {
            // Position at bottom center of screen
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0, 50); // 50 pixels from bottom
            
            // Setup health slider
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
                
                // Set green color for player health
                var fillImage = healthSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = Color.green;
                }
            }
            
            UpdateHealthText();
        }

        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamage);
        }

        private void OnPlayerTakeDamage(BossEventData data)
        {
            int damage = data.intValue;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }
            
            UpdateHealthText();
            
            // Check if player is defeated
            if (currentHealth <= 0)
            {
                OnPlayerDefeated();
            }
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
            // Handle player defeat
            Debug.Log("Player Defeated!");
            // This could trigger game over screen, respawn, etc.
        }

        public void HealPlayer(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }
            
            UpdateHealthText();
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamage);
        }
    }
}
