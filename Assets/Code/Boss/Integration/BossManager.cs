using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Manager tổng thể cho Boss system - điểm tích hợp chính
    /// </summary>
    public class BossManager : MonoBehaviour
    {
        [Header("Boss Setup")]
        [SerializeField] private BossController bossController;
        
        [Header("Fa Agent Reference")]
        [SerializeField] private Tu_Develop.Import.Scripts.FaAgent faAgent;

        public static BossManager Instance { get; private set; }
        
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
            if (faAgent == null) faAgent = FindFirstObjectByType<Tu_Develop.Import.Scripts.FaAgent>();
            if (faAgent == null)
            {
                Debug.LogError("FaAgent not found! Please assign it in the inspector.");
            }
            
            // Register for boss events
            RegisterBossEvents();
        }

       
        private void RegisterBossEvents()
        {
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChanged);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Subscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Subscribe(BossEventType.BossSpawned, OnBossSpawned);
        }
        
        private void OnPhaseChanged(BossEventData data)
        {
            var newPhase = data.intValue;
            OnBossPhaseChanged?.Invoke(newPhase);
            
            Debug.Log($"Boss entered Phase {newPhase}");
        }

        private void OnBossDefeatedEvent(BossEventData data)
        {
            OnBossDefeated?.Invoke();
            Debug.Log("Boss has been defeated!");
        }

        private void OnPlayerTakeDamageEvent(BossEventData data)
        {
            var damage = data.intValue;
            OnPlayerHealthChanged?.Invoke(damage);
        }

        private void OnBossSpawned(BossEventData data)
        {
            Debug.Log("Boss has spawned and is ready for battle!");
        }

        private void OnRequestRadarSkill(BossEventData data)
        {
            Debug.Log("Requesting Fa to use Radar skill to destroy souls");
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
            Debug.Log($"Requesting Fa to use skill: {skillName}");
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

        private void OnDestroy()
        {
            // Cleanup events
            BossEventSystem.Unsubscribe(BossEventType.PhaseChanged, OnPhaseChanged);
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Unsubscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Unsubscribe(BossEventType.BossSpawned, OnBossSpawned);
            BossEventSystem.Unsubscribe(BossEventType.RequestRadarSkill, OnRequestRadarSkill);
            BossEventSystem.Unsubscribe(BossEventType.RequestOtherSkill, OnRequestOtherSkill);
        }
    }
}
