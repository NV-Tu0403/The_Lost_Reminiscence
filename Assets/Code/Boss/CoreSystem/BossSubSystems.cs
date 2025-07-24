using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Quản lý hệ thống máu của Boss
    /// </summary>
    public class BossHealthSystem
    {
        private int maxHealthPerPhase;
        private int currentHealth;
        private int currentPhase = 1;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealthPerPhase => maxHealthPerPhase;
        public int CurrentPhase => currentPhase;
        public float HealthPercentage => (float)currentHealth / maxHealthPerPhase;
        
        public event Action<int, int> OnHealthChanged; // current, max
        public event Action OnPhaseHealthDepleted;

        public BossHealthSystem(int maxHealthPerPhase)
        {
            this.maxHealthPerPhase = maxHealthPerPhase;
            currentHealth = maxHealthPerPhase;
        }

        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealthPerPhase);
            
            if (currentHealth <= 0)
            {
                OnPhaseHealthDepleted?.Invoke();
            }
        }

        public void ResetPhaseHealth()
        {
            currentHealth = maxHealthPerPhase;
            currentPhase++;
            OnHealthChanged?.Invoke(currentHealth, maxHealthPerPhase);
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealthPerPhase, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealthPerPhase);
        }
    }

    /// <summary>
    /// Quản lý Soul entities
    /// </summary>
    public class BossSoulManager
    {
        private BossController bossController;
        private List<GameObject> activeSouls = new List<GameObject>();
        
        public int ActiveSoulCount => activeSouls.Count;
        public int MaxSouls => bossController.Config.soulConfig.maxSouls;

        public BossSoulManager(BossController controller)
        {
            bossController = controller;
            
            // Listen for Fa skill usage to destroy souls
            BossEventSystem.Subscribe(BossEventType.FaSkillUsed, OnFaSkillUsed);
        }

        public void SpawnSoul()
        {
            if (activeSouls.Count >= MaxSouls) return;
            
            var soulConfig = bossController.Config.soulConfig;
            if (soulConfig.soulPrefab == null) return;
            
            // Find spawn position around boss
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject soul = GameObject.Instantiate(soulConfig.soulPrefab, spawnPos, Quaternion.identity);
            
            // Setup soul behavior
            var soulBehavior = soul.GetComponent<SoulBehavior>();
            if (soulBehavior == null)
                soulBehavior = soul.AddComponent<SoulBehavior>();
            
            soulBehavior.Initialize(bossController.Player, soulConfig);
            
            activeSouls.Add(soul);
            BossEventSystem.Trigger(BossEventType.SoulSpawned, new BossEventData(soul));
        }

        private Vector3 GetRandomSpawnPosition()
        {
            var config = bossController.Config.soulConfig;
            Vector3 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 spawnPos = bossController.transform.position + 
                              new Vector3(randomDirection.x, 0, randomDirection.y) * config.soulSpawnRadius;
            return spawnPos;
        }

        public void DestroyAllSouls()
        {
            foreach (var soul in activeSouls)
            {
                if (soul != null)
                {
                    BossEventSystem.Trigger(BossEventType.SoulDestroyed, new BossEventData(soul));
                    GameObject.Destroy(soul);
                }
            }
            activeSouls.Clear();
        }

        private void OnFaSkillUsed(BossEventData data)
        {
            // Assuming Fa's radar skill destroys all souls
            if (data.stringValue == "Radar")
            {
                DestroyAllSouls();
            }
        }

        public void CleanupDestroyed()
        {
            activeSouls.RemoveAll(soul => soul == null);
        }
    }

    /// <summary>
    /// Quản lý UI của Boss
    /// </summary>
    public class BossUIManager
    {
        private BossController bossController;
        private BossHealthBar bossHealthBar;
        private BossSkillCastBar skillCastBar;
        
        public BossUIManager(BossController controller)
        {
            bossController = controller;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Tạo UI elements sẽ được implement trong các class riêng
            CreateBossHealthBar();
            CreateSkillCastBar();
        }

        private void CreateBossHealthBar()
        {
            // This will be implemented with actual UI components
            var uiConfig = bossController.Config.uiConfig;
            // TODO: Create actual UI GameObject and setup
        }

        private void CreateSkillCastBar()
        {
            // This will be implemented with actual UI components
            var uiConfig = bossController.Config.uiConfig;
            // TODO: Create actual UI GameObject and setup
        }
    }
}
