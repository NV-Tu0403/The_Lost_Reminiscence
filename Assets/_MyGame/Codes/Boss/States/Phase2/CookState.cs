using _MyGame.Codes.Boss.CoreSystem;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Cook State: Boss bị đánh bại (không rớt Memory Fragment)
    /// </summary>
    public class CookState : BossState
    {
        private float _cookTimer;
        private bool _defeatEmitted;
        
        public override void Enter()
        {
            _cookTimer = 0f;
            _defeatEmitted = false;
            //BossController.PlayAnimation("Cook");
            
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
            _cookTimer += Time.deltaTime;
            
            // Emit BossDefeated once after cook duration (no memory fragment drop)
            if (!_defeatEmitted && _cookTimer >= Config.phase2.cookStateDuration)
            {
                BossEventSystem.Trigger(BossEventType.BossDefeated);
                _defeatEmitted = true;
            }
            
            // Destroy boss shortly after to end the fight cleanly
            if (_cookTimer >= Config.phase2.cookStateDuration + 1f)
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
