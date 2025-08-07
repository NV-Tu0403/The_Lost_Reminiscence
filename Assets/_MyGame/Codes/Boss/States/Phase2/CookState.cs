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
            BossController.PlayAnimation("Cook"); // Play death/cook animation
            
            BossEventSystem.Trigger(BossEventType.SkillInterrupted); 
            BossEventSystem.Trigger(BossEventType.BossDefeated); 
            
            // Stop all movement
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.SetDestination(BossController.transform.position);
                BossController.NavAgent.enabled = false;
            }
            
            // Clear all souls
            BossController.SoulManager.DestroyAllSouls();
        }

        public override void Update()
        {
            cookTimer += Time.deltaTime;
            if (cookTimer >= Config.phase2.cookStateDuration && !memoryFragmentDropped)
            {
                DropMemoryFragment();
                memoryFragmentDropped = true;
            }
            
            if (cookTimer >= Config.phase2.cookStateDuration + 1f)
            {
                CompleteBossDefeat();
            }
        }
        
        private void DropMemoryFragment()
        {
            // Spawn memory fragment prefab từ BossConfig tại vị trí boss
            if (Config.memoryFragmentPrefab != null)
            {
                var memoryFragment = Object.Instantiate(Config.memoryFragmentPrefab, 
                    BossController.transform.position, 
                    Quaternion.identity);
                
                BossEventSystem.Trigger(BossEventType.BossDefeated, new BossEventData(memoryFragment));
            }
            else
            {
                Debug.LogWarning("[CookState] Memory Fragment Prefab not assigned in BossConfig!");
                BossEventSystem.Trigger(BossEventType.BossDefeated);
            }
        }

        private void CompleteBossDefeat()
        {
            Object.Destroy(BossController.gameObject);
        }

        public override void Exit() { }

        public override void OnTakeDamage() { }

        public override bool CanTakeDamage() => false;

        public override bool CanBeInterrupted() => false;

    }
}
