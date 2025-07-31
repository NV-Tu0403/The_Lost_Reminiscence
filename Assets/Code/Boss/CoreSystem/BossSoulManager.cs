using System.Collections.Generic;
using UnityEngine;

namespace Code.Boss
{
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

        public void SpawnSoul(Vector3 position)
        {
            if (activeSouls.Count >= MaxSouls) return;
            var soulConfig = bossController.Config.soulConfig;
            if (soulConfig.soulPrefab == null) return;
            
            var soul = Object.Instantiate(soulConfig.soulPrefab, position, Quaternion.identity);
            var soulBehavior = soul.GetComponent<SoulBehavior>();
            
            if (soulBehavior == null)
                soulBehavior = soul.AddComponent<SoulBehavior>();
            soulBehavior.Initialize(bossController.Player, soulConfig);
            activeSouls.Add(soul);
        }

        public void DestroyAllSouls()
        {
            foreach (var soul in activeSouls)
            {
                if (soul != null)
                {
                    BossEventSystem.Trigger(BossEventType.SoulDestroyed, new BossEventData(soul));
                    Object.Destroy(soul);
                }
            }
            activeSouls.Clear();
        }

        private void OnFaSkillUsed(BossEventData data)
        {
            // Assuming Fa's radar skill destroys all souls
            if (data.stringValue == "KnowledgeLight")
            {
                DestroyAllSouls();
            }
        }
    }
}
