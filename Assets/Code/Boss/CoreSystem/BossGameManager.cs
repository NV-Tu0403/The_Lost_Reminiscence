using System;
using Tu_Develop.Import.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Boss
{
    /// <summary>
    /// Manager tổng thể cho Boss system - quản lý game state, restart, và tích hợp với Fa Agent
    /// </summary>
    public class BossGameManager : MonoBehaviour
    {
        [Header("Boss Setup")]
        [SerializeField] private BossController bossController;
        
        [Header("Fa Agent Reference")]
        [SerializeField] private FaAgent faAgent;
        
        [Header("UI References")]
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private PlayerHealthBar playerHealthBar;
        
        public static BossGameManager Instance { get; private set; }
        
        private bool isGameOver = false;
        
        // Events for external systems
        public System.Action<int> OnBossPhaseChanged;
        public System.Action OnBossDefeated;
        public System.Action<int> OnPlayerHealthChanged;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeBossSystem();
            RegisterBossEvents();
            
            if (gameOverUI != null)
                gameOverUI.SetActive(false);
        }

        private void InitializeBossSystem()
        {
            // Setup boss controller if not assigned
            if (bossController == null) bossController = FindFirstObjectByType<BossController>();
            if (bossController == null)
            {
                Debug.LogError("BossController not found! Please assign it in the inspector.");
                return;
            }
            
            // Setup FaAgent if not assigned
            if (faAgent == null) faAgent = FindFirstObjectByType<FaAgent>();
            if (faAgent == null)
            {
                Debug.LogWarning("FaAgent not found! Boss system will work without Fa integration.");
            }
            
            // Setup PlayerHealthBar if not assigned
            if (playerHealthBar == null) playerHealthBar = FindFirstObjectByType<PlayerHealthBar>();
            if (playerHealthBar == null)
            {
                Debug.LogWarning("PlayerHealthBar not found! Please assign it in the inspector.");
            }
        }

        private void RegisterBossEvents()
        {
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChangedEvent);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Subscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Subscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
            BossEventSystem.Subscribe(BossEventType.BossSpawned, OnBossSpawned);
            BossEventSystem.Subscribe(BossEventType.RequestRadarSkill, OnRequestRadarSkill);
            BossEventSystem.Subscribe(BossEventType.RequestOtherSkill, OnRequestOtherSkill);
        }
        
        #region Boss Events
        private void OnPhaseChangedEvent(BossEventData data)
        {
            var newPhase = data.intValue;
            OnBossPhaseChanged?.Invoke(newPhase);
            Debug.Log($"[BossGameManager] Boss entered Phase {newPhase}");
        }

        private void OnBossDefeatedEvent(BossEventData data)
        {
            OnBossDefeated?.Invoke();
            Debug.Log("[BossGameManager] Boss has been defeated!");
        }

        private void OnPlayerTakeDamageEvent(BossEventData data)
        {
            var damage = data.intValue;
            OnPlayerHealthChanged?.Invoke(damage);
        }

        private void OnPlayerDefeatedEvent(BossEventData data)
        {
            if (isGameOver) return;
            
            isGameOver = true;
            Debug.Log("[BossGameManager] Player defeated - Game Over");
            
            // Show Game Over UI
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
            
            // Pause game
            Time.timeScale = 0f;
        }

        private void OnBossSpawned(BossEventData data)
        {
            Debug.Log("[BossGameManager] Boss has spawned and is ready for battle!");
        }
        #endregion

        #region Fa Agent Integration
        private void OnRequestRadarSkill(BossEventData data)
        {
            Debug.Log("[BossGameManager] Requesting Fa to use Radar skill to destroy souls");
            if (faAgent != null)
            {
                faAgent.UseGuideSignal();
            }
            else
            {
                Debug.LogWarning("FaAgent reference missing, cannot use Radar skill.");
            }
        }

        private void OnRequestOtherSkill(BossEventData data)
        {
            var skillName = data.stringValue ?? "Unknown";
            Debug.Log($"[BossGameManager] Requesting Fa to use skill: {skillName}");
            if (faAgent != null)
            {
                switch (skillName)
                {
                    case "GuideSignal":
                        faAgent.UseGuideSignal();
                        BossEventSystem.Trigger(BossEventType.FaSkillUsed, new BossEventData("GuideSignal"));
                        break;
                    case "KnowledgeLight":
                        faAgent.UseKnowledgeLight();
                        BossEventSystem.Trigger(BossEventType.FaSkillUsed, new BossEventData("KnowledgeLight"));
                        break;
                    case "ProtectiveAura":
                        faAgent.UseProtectiveAura();
                        BossEventSystem.Trigger(BossEventType.FaSkillUsed, new BossEventData("ProtectiveAura"));
                        break;
                    default:
                        Debug.LogWarning($"Skill '{skillName}' is not recognized by FaAgent.");
                        break;
                }
            }
            else
            {
                Debug.LogWarning("FaAgent reference missing, cannot use skill.");
            }
        }
        #endregion

        #region Game State Management
        public void RestartGame()
        {
            Debug.Log("[BossGameManager] Restarting game...");
            
            // Reset time scale
            Time.timeScale = 1f;
            isGameOver = false;
            
            // Hide Game Over UI
            if (gameOverUI != null)
                gameOverUI.SetActive(false);
            
            // Reset Boss System
            ResetBossSystem();
            
            // Reset Player Health
            ResetPlayerHealth();
        }
        
        private void ResetBossSystem()
        {
            if (bossController != null)
            {
                bossController.ResetBoss();
                Debug.Log("[BossGameManager] Boss system reset");
            }
        }
        
        private void ResetPlayerHealth()
        {
            // Trigger event to reset player health
            BossEventSystem.Trigger(BossEventType.PlayerHealthReset, new BossEventData(3));
            Debug.Log("[BossGameManager] Player health reset");
        }
        
        public void ReloadScene()
        {
            Debug.Log("[BossGameManager] Reloading scene...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void QuitGame()
        {
            Debug.Log("[BossGameManager] Quitting game...");
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        #endregion

        #region Public API
        /// <summary>
        /// Lấy reference tới BossController
        /// </summary>
        public BossController GetBossController() => bossController;
        
        /// <summary>
        /// Lấy reference tới FaAgent
        /// </summary>
        public FaAgent GetFaAgent() => faAgent;
        
        /// <summary>
        /// Kiểm tra game có đang ở trạng thái Game Over không
        /// </summary>
        public bool IsGameOver() => isGameOver;
        #endregion

        private void OnDestroy()
        {
            // Cleanup events
            BossEventSystem.Unsubscribe(BossEventType.PhaseChanged, OnPhaseChangedEvent);
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Unsubscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Unsubscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
            BossEventSystem.Unsubscribe(BossEventType.BossSpawned, OnBossSpawned);
            BossEventSystem.Unsubscribe(BossEventType.RequestRadarSkill, OnRequestRadarSkill);
            BossEventSystem.Unsubscribe(BossEventType.RequestOtherSkill, OnRequestOtherSkill);
        }
    }
}
