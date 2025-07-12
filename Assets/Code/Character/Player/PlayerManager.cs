using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DuckLe;

namespace DuckLe
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] public PlayerStats playerStats;

        [Header("UI In Local")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider manaBar;
        [SerializeField] private TMP_Text expText;

        [Header("UI In World")]
        [SerializeField] private Slider healthBarWorld;

        public float health { get; set; }
        public float mana { get; set; }
        public float coin { get; set; }

        public PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats is not assigned in PlayerManager!", this);
                return;
            }

            // Initialize player stats
            health = playerStats.health;
            mana = playerStats.mana;
            coin = playerStats.coin;

            // Initialize UI elements
            if (healthBarWorld != null)
            {
                healthBarWorld.maxValue = playerStats.health;
                healthBarWorld.value = health;
            }
            else
            {
                Debug.LogWarning("healthBarWorld is not assigned in Inspector!", this);
            }

            if (healthBar != null)
            {
                healthBar.maxValue = playerStats.health;
                healthBar.value = health;
            }
            else
            {
                Debug.LogWarning("PlayerHealthBar is not assigned in Inspector!", this);
            }

            if (manaBar != null)
            {
                manaBar.maxValue = playerStats.mana;
                manaBar.value = mana;
            }
            else
            {
                Debug.LogWarning("PlayerManaBar is not assigned in Inspector!", this);
            }

            if (expText != null)
            {
                expText.text = playerStats.exp.ToString();
            }
            else
            {
                Debug.LogWarning("ExpText is not assigned in Inspector!", this);
            }
        }

        private void Update()
        {
            // Update exp text if not assigned
            if (expText == null)
            {
                expText = GameObject.Find("CurrentExp")?.GetComponent<TMP_Text>();
                if (expText != null)
                {
                    expText.text = playerStats.exp.ToString();
                }
                else
                {
                    Debug.LogWarning("CurrentExp not found in scene!", this);
                }
            }
        }

        /// <summary>
        /// Apply damage to the player.
        /// </summary>
        /// <param name="amount">Damage amount</param>
        /// <param name="senderName">Name of the damage source</param>
        /// <param name="usableName">Name of the object used (e.g., weapon)</param>
        public void TakeDamage(float amount, string senderName, string usableName = "Default")
        {
            ApplyDamage(amount, senderName, usableName);
        }

        /// <summary>
        /// Calculate and apply damage to the player.
        /// </summary>
        public void ApplyDamage(float amount, string senderName, string usableName)
        {
            amount = Mathf.Clamp(amount, 0, playerStats.maxDamagePerHit);
            health -= amount;
            health = Mathf.Clamp(health, 0, playerStats.health);

            Debug.Log($"{name} took {amount} damage from {senderName} using {usableName}, health now: {health}");

            UpdateHealthUI();

            if (health <= 0)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        private void OnDeath()
        {
            Debug.Log($"{name} has died!");

            // Disable player controls
            _playerController.enabled = false;

            // Play death animation
            if (TryGetComponent<Animator>(out var anim))
            {
                anim.SetTrigger("Die");
            }

            // Start respawn coroutine
            StartCoroutine(RespawnAfterDelay(5f));
        }

        /// <summary>
        /// Respawn the player after a delay.
        /// </summary>
        private IEnumerator RespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Reset health
            health = playerStats.health;
            UpdateHealthUI();

            // Re-enable player
            //_playerController.enabled = true;

            //// Play respawn animation
            //if (TryGetComponent<Animator>(out var anim))
            //{
            //    anim.SetTrigger("Respawn");
            //}

            // TODO: Set player position to spawn point if needed
            // transform.position = GetSpawnPosition();
        }

        /// <summary>
        /// Update health UI.
        /// </summary>
        private void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.value = health;
            }
            if (healthBarWorld != null)
            {
                healthBarWorld.value = health;
            }
        }

        /// <summary>
        /// Update mana UI.
        /// </summary>
        private void UpdateManaUI()
        {
            if (manaBar != null)
            {
                manaBar.value = mana;
            }
        }

        /// <summary>
        /// Update experience UI and stats.
        /// </summary>
        private void UpdateExpUI()
        {
            if (expText != null)
            {
                expText.text = playerStats.exp.ToString();
            }
            else
            {
                Debug.LogWarning("ExpText is not assigned!", this);
            }

            // Optionally update exp to a local save system (replace PlayFab)
            // SaveExpLocally(playerStats.exp);
        }

        /// <summary>
        /// Heal the player.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(float amount)
        {
            health += amount;
            health = Mathf.Clamp(health, 0, playerStats.health);
            Debug.Log($"{name} healed for {amount}, health now: {health}");
            UpdateHealthUI();
        }

        /// <summary>
        /// Regenerate mana for the player.
        /// </summary>
        /// <param name="amount">Amount of mana to regenerate</param>
        public void RegenerateMana(float amount)
        {
            mana += amount;
            mana = Mathf.Clamp(mana, 0, playerStats.mana);
            Debug.Log($"{name} regenerated {amount} mana, mana now: {mana}");
            UpdateManaUI();
        }

        /// <summary>
        /// Add coins to the player.
        /// </summary>
        /// <param name="amount">Amount of coins to add</param>
        public void AddCoin(float amount)
        {
            coin += amount;
            playerStats.coin += amount;
            Debug.Log($"{name} gained {amount} coins, total: {playerStats.coin}");
            // Optionally save coins locally
            // SaveCoinsLocally(playerStats.coin);
        }

        /// <summary>
        /// Add experience to the player.
        /// </summary>
        /// <param name="amount">Amount of experience to add</param>
        public void AddExp(float amount)
        {
            playerStats.exp += amount;
            Debug.Log($"{name} gained {amount} exp, total: {playerStats.exp}");
            UpdateExpUI();
        }

        /// <summary>
        /// Get the player's stats.
        /// </summary>
        /// <returns>PlayerStats object</returns>
        public PlayerStats GetPlayerStats()
        {
            return playerStats;
        }
    }
}
