using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Code.Boss
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

        public void Initialize(int playerMaxHealth)
        {
            maxHealth = playerMaxHealth;
            currentHealth = playerMaxHealth;
            
            SetupUI();
            RegisterEvents();
        }

        private void SetupUI()
        {
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
            // Reload Scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
