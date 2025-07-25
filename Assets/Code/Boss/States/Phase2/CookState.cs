using UnityEngine;

namespace Code.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Cook State: Boss bị đánh bại và rớt mảnh ghép
    /// </summary>
    public class CookState : BossState
    {
        private float cookTimer;
        private bool memoryFragmentDropped = false;

        public override void Enter()
        {
            cookTimer = 0f;
            memoryFragmentDropped = false;
            
            Debug.Log("[Boss State] Entered CookState - Boss bị đánh bại và đang biến mất");
            
            // Ẩn UI máu boss và UI cast skill khi boss bị đánh bại
            Debug.Log("[CookState] Hiding boss health UI and cast skill UI");
            BossEventSystem.Trigger(BossEventType.SkillInterrupted); // Ẩn UI cast skill
            BossEventSystem.Trigger(BossEventType.BossDefeated); // Có thể trigger event để ẩn UI máu boss
            
            // Stop all movement
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.SetDestination(bossController.transform.position);
                bossController.NavAgent.enabled = false;
            }
            
            // Clear all souls
            bossController.SoulManager.DestroyAllSouls();
        }

        public override void Update()
        {
            cookTimer += Time.deltaTime;
            
            // Gradually fade boss
            FadeBoss();
            
            if (cookTimer >= config.phase2.cookStateDuration && !memoryFragmentDropped)
            {
                DropMemoryFragment();
                memoryFragmentDropped = true;
            }
            
            if (cookTimer >= config.phase2.cookStateDuration + 1f)
            {
                CompleteBossDefeat();
            }
        }

        private void FadeBoss()
        {
            Debug.Log("[FadeBoss] Fading out boss over time");
        }

        private void DropMemoryFragment()
        {
            // Create memory fragment at boss position
            GameObject memoryFragment = new GameObject("MemoryFragment");
            memoryFragment.transform.position = bossController.transform.position;
            
            // Add memory fragment behavior
            var fragmentBehavior = memoryFragment.AddComponent<MemoryFragmentBehavior>();
            
            BossEventSystem.Trigger(BossEventType.BossDefeated, new BossEventData(memoryFragment));
        }

        private void CompleteBossDefeat()
        {
            // Boss completely defeated
            bossController.gameObject.SetActive(false);
            // Or destroy the boss GameObject
            // Object.Destroy(bossController.gameObject);
        }

        public override void Exit()
        {
            // No special cleanup needed - boss is defeated
        }

        public override void OnTakeDamage()
        {
            // Boss cannot take damage when cooking/defeated
        }

        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
