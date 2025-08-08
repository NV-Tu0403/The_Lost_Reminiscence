using UnityEngine;

namespace Code.Boss.States.Shared
{
    /// <summary>
    /// Phase Change State: Chuyển đổi từ Phase 1 sang Phase 2
    /// </summary>
    public class PhaseChangeState : BossState
    {
        private float transitionTimer;
        private bool transitionCompleted = false;

        public override void Enter()
        {
            transitionTimer = 0f;
            transitionCompleted = false;
            
            Debug.Log("[Boss State] Entered PhaseChangeState - Chuyển đổi từ Phase 1 sang Phase 2");
            //BossController.PlayAnimation("PhaseChange");
            
            // Play phase change sound
            if (Config.audioConfig.phaseChangeSound != null)
            {
                BossController.PlaySound(Config.audioConfig.phaseChangeSound, Config.audioConfig.sfxVolume);
            }
            
            // Clear any remaining decoys and souls
            BossController.ClearDecoys();
            BossController.SoulManager.DestroyAllSouls();
        }

        public override void Update()
        {
            transitionTimer += Time.deltaTime;

            if (!(transitionTimer >= 2f) || transitionCompleted) return; // 2 second transition
            transitionCompleted = true;
            BossController.ChangeToPhase(2);
        }

        public override void Exit()
        {
            // BossController.ResetMoveDirection();
            // BossController.NavAgent.speed = Config.moveSpeed;
        }

        public override void OnTakeDamage()
        {
            // Cannot take damage during phase transition
        }

        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
