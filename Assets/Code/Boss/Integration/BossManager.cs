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
        [SerializeField] private BossConfig bossConfig;
        
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
            if (bossController == null)
            {
                bossController = FindObjectOfType<BossController>();
            }
            
            if (bossController == null)
            {
                Debug.LogError("BossController not found! Please assign it in the inspector.");
                return;
            }
            
            // Register for boss events
            RegisterBossEvents();
            
            // Register for Fa integration events
            RegisterFaIntegrationEvents();
            
            Debug.Log("Boss System initialized successfully!");
        }

       
        private void RegisterBossEvents()
        {
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChanged);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeatedEvent);
            BossEventSystem.Subscribe(BossEventType.PlayerTakeDamage, OnPlayerTakeDamageEvent);
            BossEventSystem.Subscribe(BossEventType.BossSpawned, OnBossSpawned);
        }

        private void RegisterFaIntegrationEvents()
        {
            // Listen for Fa skill requests
            BossEventSystem.Subscribe(BossEventType.RequestRadarSkill, OnRequestRadarSkill);
            BossEventSystem.Subscribe(BossEventType.RequestOtherSkill, OnRequestOtherSkill);
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
            // Request Fa to use Radar skill to destroy souls
            Debug.Log("Requesting Fa to use Radar skill to destroy souls");
            
            // This will be handled by Fa's system
            // For now, we can simulate the request
            RequestFaSkill("Radar");
        }

        private void OnRequestOtherSkill(BossEventData data)
        {
            var skillName = data.stringValue ?? "Unknown";
            Debug.Log($"Requesting Fa to use skill: {skillName}");
            
            RequestFaSkill(skillName);
        }

        private void RequestFaSkill(string skillName)
        {
            // This method will be called by Fa's system when skill is used
            // For now, we can simulate skill usage for testing
            
            // In actual implementation, this would communicate with Fa's skill system
            // Example: FaSkillManager.Instance.RequestSkill(skillName, OnFaSkillComplete);
        }

        // This method should be called by Fa's system when a skill is completed
        public void OnFaSkillCompleted(string skillName, bool success)
        {
            if (success)
            {
                BossEventSystem.Trigger(BossEventType.FaSkillUsed, 
                    new BossEventData { stringValue = skillName });
                
                Debug.Log($"Fa successfully used skill: {skillName}");
            }
            else
            {
                Debug.Log($"Fa failed to use skill: {skillName}");
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
