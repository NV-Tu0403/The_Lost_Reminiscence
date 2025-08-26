using System.Collections.Generic;
using _MyGame.Codes.Boss.States.Phase1;
using _MyGame.Codes.Boss.UI;
using Code.Boss;
using Tu_Develop.Import.Scripts;
using UnityEngine;
using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Dialogue; 
using _MyGame.Codes.Boss.States.Phase2; 
using _MyGame.Codes.Boss.States.Shared;

namespace _MyGame.Codes.Boss.CoreSystem
{
    /// <summary>
    /// Manager tổng thể cho Boss system - quản lý game state, restart, và tích hợp với Fa Agent
    /// </summary>
    public class BossGameManager : MonoBehaviour
    {
        [Header("Boss Setup")]
        [SerializeField] private BossController bossController;
        [SerializeField] private BossTriggerZone bossTriggerZone;
        
        [Header("Fa Agent Reference")]
        [SerializeField] private FaAgent faAgent;
        
        [Header("UI References")]
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private PlayerHealthBar playerHealthBar;
        [SerializeField] private BossHealthBar bossHealthBar;
        [SerializeField] private BossSkillCastBar bossSkillCastBar;
        [SerializeField] private BossPhasePanel bossPhasePanel; 
        
        [Header("Dialogue Hints (Addressable IDs)")]
        [Tooltip("Addressable ID for Decoy hint bubble")]
        [SerializeField] private string decoyHintDialogueId;
        [Tooltip("Addressable ID for Soul hint bubble")]
        [SerializeField] private string soulHintDialogueId;
        [Tooltip("Addressable ID for Fear Zone hint bubble")]
        [SerializeField] private string fearZoneHintDialogueId;
        [Tooltip("Addressable ID for Scream hint bubble")]
        [SerializeField] private string screamHintDialogueId;
        
        public static BossGameManager Instance { get; private set; }
        
        private bool isGameOver = false;
        
        // Events for external systems
        public System.Action<int> OnBossPhaseChanged;
        public System.Action OnBossDefeated;
        public System.Action<int> OnPlayerHealthChanged;
        public System.Action OnBossFightStarted;

        // Track current player health
        private int currentPlayerHealth = 3; 
        // Track states that already showed a hint (show once per state)
        private readonly HashSet<string> shownHintStates = new HashSet<string>();

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
            HideAllUI(); // Ẩn UI ban đầu
        }

        private void InitializeBossSystem()
        {
            // Setup boss trigger zone if not assigned
            if (bossTriggerZone == null) bossTriggerZone = FindFirstObjectByType<BossTriggerZone>();
            
            // Setup boss controller if not assigned (có thể null ban đầu vì chưa spawn)
            if (bossController == null) bossController = FindFirstObjectByType<BossController>();
            
            // Setup FaAgent if not assigned
            if (faAgent == null) faAgent = FindFirstObjectByType<FaAgent>();
            if (faAgent == null)
            {
                Debug.LogWarning("FaAgent not found! Boss system will work without Fa integration.");
            }
            
            // Setup UI components
            if (playerHealthBar == null) playerHealthBar = FindFirstObjectByType<PlayerHealthBar>();
            if (bossHealthBar == null) bossHealthBar = FindFirstObjectByType<BossHealthBar>();
            if (bossSkillCastBar == null) bossSkillCastBar = FindFirstObjectByType<BossSkillCastBar>();
            if (bossPhasePanel == null) bossPhasePanel = FindFirstObjectByType<BossPhasePanel>(); // find phase panel if not assigned
        }

        private void RegisterBossEvents()
        {
            BossEventSystem.Subscribe(BossEventType.BossFightStarted, OnBossFightStartedEvent);
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChangedEvent);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Subscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Subscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
            BossEventSystem.Subscribe(BossEventType.BossSpawned, OnBossSpawned);
            BossEventSystem.Subscribe(BossEventType.RequestRadarSkill, OnRequestRadarSkill);
            BossEventSystem.Subscribe(BossEventType.RequestOtherSkill, OnRequestOtherSkill);
            // Dialogue hints driven by StateChanged for auto-hide and per-state show-once
            BossEventSystem.Subscribe(BossEventType.StateChanged, OnStateChangedEvent);
            // Auto-hide hint when cast finishes
            BossEventSystem.Subscribe(BossEventType.SkillInterrupted, OnSkillInterruptedEvent);
        }

        private void HideAllUI()
        {
            if (playerHealthBar != null) playerHealthBar.gameObject.SetActive(false);
            if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);
            if (bossSkillCastBar != null) bossSkillCastBar.gameObject.SetActive(false);
            if (bossPhasePanel != null) bossPhasePanel.gameObject.SetActive(false); // hide phase panel root object
            if (gameOverUI != null) gameOverUI.SetActive(false);
        }

        private void ShowBossUI()
        {
            if (playerHealthBar != null) playerHealthBar.gameObject.SetActive(true);
            if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(true);
            if (bossSkillCastBar != null) bossSkillCastBar.gameObject.SetActive(true);
            if (bossPhasePanel != null) bossPhasePanel.gameObject.SetActive(true); // ensure phase panel enabled (it will self-hide panelRoot)
            
            // Initialize UI với boss controller mới spawn
            if (bossController == null) return;
            // Reset current health khi bắt đầu boss fight
            currentPlayerHealth = 3;
                
            if (playerHealthBar != null) playerHealthBar.Initialize(3, bossController.Config); 
            if (bossHealthBar != null) bossHealthBar.Initialize(bossController);
            if (bossSkillCastBar != null) bossSkillCastBar.Initialize(bossController);
            if (bossPhasePanel != null) bossPhasePanel.Initialize(bossController);
        }
        
        #region Boss Events
        private void OnBossFightStartedEvent(BossEventData data)
        {
            // Update boss controller reference từ trigger zone
            if (bossTriggerZone != null)
            {
                bossController = bossTriggerZone.GetSpawnedBoss();
            }
            
            // Show boss UI
            ShowBossUI();
            
            // Show Phase immediately to avoid missing early PhaseChanged event
            if (bossPhasePanel != null && bossController != null)
            {
                bossPhasePanel.ShowPhaseNow(bossController.CurrentPhase);
            }
            
            OnBossFightStarted?.Invoke();
            Debug.Log("[BossGameManager] Boss fight started - UI activated!");
            
            // Reset one-time hint tracking on new fight
            shownHintStates.Clear();
        }

        private void OnPhaseChangedEvent(BossEventData data)
        {
            var newPhase = data.intValue;
            OnBossPhaseChanged?.Invoke(newPhase);
            Debug.Log($"[BossGameManager] Boss entered Phase {newPhase}");
        }

        private void OnBossDefeatedEvent(BossEventData data)
        {
            // Hide any hint bubble when fight ends
            HideHintBubble();
            
            OnBossDefeated?.Invoke();
            Debug.Log("[BossGameManager] Boss has been defeated!");
            
            // Start defeat timeline via EventBus; when it finishes, end the session
            var cfg = bossController != null ? bossController.Config : null;
            if (cfg != null && !string.IsNullOrEmpty(cfg.bossDefeatTimelineId))
            {
                var evt = new BaseEventData
                {
                    eventId = cfg.bossDefeatTimelineId,
                    OnFinish = () =>
                    {
                        // Call end-session hook after the timeline completes
                        CoreEvent.Instance.TriggerEndSession();
                    }
                };
                EventBus.Publish("StartTimeline", evt);
            }
            else
            {
                // If no timeline configured, end the session immediately
                CoreEvent.Instance.TriggerEndSession();
            }
        }

        private void OnPlayerTakeDamageEvent(BossEventData data)
        {
            if (isGameOver) return; // Ignore damage after game over
            var damage = data.intValue;
            
            // Store previous health, then subtract
            var prevHealth = currentPlayerHealth;
            currentPlayerHealth = Mathf.Max(0, currentPlayerHealth - damage);
            
            // Pass current health (không phải damage) cho UI
            OnPlayerHealthChanged?.Invoke(currentPlayerHealth);
            
            // Trigger PlayerDefeated only on transition from >0 to 0
            if (prevHealth > 0 && currentPlayerHealth == 0)
            {
                BossEventSystem.Trigger(BossEventType.PlayerDefeated);
            }
        }

        private void OnPlayerDefeatedEvent(BossEventData data)
        {
            if (isGameOver) return;
            
            isGameOver = true;
            Debug.Log("[BossGameManager] Player defeated - Game Over");
            
            // Hide any hint bubble on game over
            HideHintBubble();
            
            // Show Game Over UI
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
            
            // Pause game
            Time.timeScale = 0f;
        }

        private static void OnBossSpawned(BossEventData data)
        {
            Debug.Log("[BossGameManager] Boss has spawned and is ready for battle!");
        }
        #endregion

        #region Dialogue Hint Handlers
        private void OnStateChangedEvent(BossEventData data)
        {
            var newStateName = data?.stringValue;
            var id = ResolveDialogueIdByStateName(newStateName);
            var dm = DialogueManager.Instance;

            // If new state is not a hint state -> defer hide to let current typing finish (avoid cutting off)
            if (string.IsNullOrEmpty(id))
            {
                if (dm != null)
                {
                    dm.HideBubbleTutorialDeferred();
                }
                else
                {
                    HideHintBubble();
                }
                return;
            }

            // New state has a hint -> show immediately, but only once per state per fight
            if (string.IsNullOrEmpty(newStateName) || shownHintStates.Contains(newStateName)) return;
            // Clear any existing bubble instantly to make room for new hint
            HideHintBubble();
            ShowHintBubble(id);
            shownHintStates.Add(newStateName);
        }

        private void OnSkillInterruptedEvent(BossEventData data)
        {
            // Casting finished -> defer hide bubble until typewriter completes
            var dm = DialogueManager.Instance;
            if (dm != null)
            {
                dm.HideBubbleTutorialDeferred();
            }
            else
            {
                HideHintBubble();
            }
        }

        private string ResolveDialogueIdByStateName(string stateName)
        {
            switch (stateName)
            {
                case nameof(DecoyState):
                    return decoyHintDialogueId;
                case nameof(SoulState):
                    return soulHintDialogueId;
                case nameof(FearZoneState):
                    return fearZoneHintDialogueId;
                case nameof(ScreamState):
                    return screamHintDialogueId;
                default:
                    return null;
            }
        }

        private void ShowHintBubble(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId)) return;
            var dm = DialogueManager.Instance;
            if (dm == null)
            {
                Debug.LogWarning("[BossGameManager] DialogueManager not found, cannot show hint bubble.");
                return;
            }
            dm.ShowBubbleTutorial(dialogueId);
        }

        private void HideHintBubble()
        {
            var dm = DialogueManager.Instance;
            if (dm == null) return;
            dm.HideBubbleTutorial();
        }
        #endregion

        #region Fa Agent Integration
        private void OnRequestRadarSkill(BossEventData data)
        {
            //Debug.Log("[BossGameManager] Requesting Fa to use Radar skill to destroy souls");
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
            //Debug.Log($"[BossGameManager] Requesting Fa to use skill: {skillName}");
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
            
            // Hide all UI
            HideAllUI();
            
            // Hide any active dialogue hint
            HideHintBubble();
            
            // Reset boss system first (destroy current boss)
            ResetBossSystem();
            
            // Reset trigger zone
            ResetTriggerZone();
            
            // Reset Player Health
            ResetPlayerHealth();
            
            // Reset one-time hint tracking on restart
            shownHintStates.Clear();
        }
        
        private void ResetBossSystem()
        {
            if (bossController == null) return;
            Destroy(bossController.gameObject);
            bossController = null;
            Debug.Log("[BossGameManager] Boss destroyed for restart");
        }
        
        private void ResetTriggerZone()
        {
            if (bossTriggerZone == null) return;
            bossTriggerZone.ResetTrigger();
            Debug.Log("[BossGameManager] Trigger zone reset");
        }
        
        private void ResetPlayerHealth()
        {
            // Reset current health tracking
            currentPlayerHealth = 3; 
            
            // Trigger event to reset player health UI
            BossEventSystem.Trigger(BossEventType.PlayerHealthReset, new BossEventData(3)); // Sửa từ 6 về 3
            Debug.Log("[BossGameManager] Player health reset");
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
            BossEventSystem.Unsubscribe(BossEventType.BossFightStarted, OnBossFightStartedEvent);
            BossEventSystem.Unsubscribe(BossEventType.PhaseChanged, OnPhaseChangedEvent);
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Unsubscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Unsubscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
            BossEventSystem.Unsubscribe(BossEventType.BossSpawned, OnBossSpawned);
            BossEventSystem.Unsubscribe(BossEventType.RequestRadarSkill, OnRequestRadarSkill);
            BossEventSystem.Unsubscribe(BossEventType.RequestOtherSkill, OnRequestOtherSkill);
            BossEventSystem.Unsubscribe(BossEventType.StateChanged, OnStateChangedEvent);
            BossEventSystem.Unsubscribe(BossEventType.SkillInterrupted, OnSkillInterruptedEvent);
        }
    }
}
