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
            
            // Play phase change sound
            if (config.audioConfig.phaseChangeSound != null)
            {
                bossController.PlaySound(config.audioConfig.phaseChangeSound, config.audioConfig.sfxVolume);
            }
            
            // Clear any remaining decoys and souls
            bossController.ClearDecoys();
            bossController.SoulManager.DestroyAllSouls();
        }

        public override void Update()
        {
            transitionTimer += Time.deltaTime;
            
            if (transitionTimer >= 2f && !transitionCompleted) // 2 second transition
            {
                transitionCompleted = true;
                bossController.ChangeToPhase(2);
            }
        }

        public override void Exit()
        {
            // Cleanup handled in Enter()
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
