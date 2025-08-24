using _MyGame.Codes.Boss.CoreSystem;
using Code.Boss;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Cook State: Boss bị đánh bại (không rớt Memory Fragment)
    /// </summary>
    public class CookState : BossState
    {
        private float cookTimer;
        private bool defeatEmitted;
        
        public override void Enter()
        {
            cookTimer = 0f;
            defeatEmitted = false;
            BossController.PlayAnimation("Cook");
            
            BossEventSystem.Trigger(BossEventType.SkillInterrupted);
            
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
            
            // Emit BossDefeated once after cook duration (no memory fragment drop)
            if (!defeatEmitted && cookTimer >= Config.phase2.cookStateDuration)
            {
                BossEventSystem.Trigger(BossEventType.BossDefeated);
                defeatEmitted = true;
            }
            
            // Destroy boss shortly after to end the fight cleanly
            if (cookTimer >= Config.phase2.cookStateDuration + 1f)
            {
                CompleteBossDefeat();
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
