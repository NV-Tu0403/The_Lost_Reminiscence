using _MyGame.Codes.Boss.CoreSystem;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Shared
{
    /// <summary>
    /// Phase Change State: Chuyển đổi từ Phase 1 sang Phase 2
    /// </summary>
    public class PhaseChangeState : BossState
    {
        private float _transitionTimer;
        private bool _transitionCompleted = false;

        public override void Enter()
        {
            _transitionTimer = 0f;
            _transitionCompleted = false;
            
            Debug.Log("[Boss State] Entered PhaseChangeState - Chuyển đổi từ Phase 1 sang Phase 2");
            
            // Spawn follow effect that sticks to the boss until death
            BossController.SpawnPhaseChangeFollowEffect();
            
            // Clear any remaining decoys and souls
            BossController.ClearDecoys();
            BossController.SoulManager.DestroyAllSouls();
        }

        public override void Update()
        {
            _transitionTimer += Time.deltaTime;

            if (!(_transitionTimer >= 2f) || _transitionCompleted) return; // 2 second transition
            _transitionCompleted = true;
            BossController.ChangeToPhase(2);
        }

        public override void Exit() { }

        public override void OnTakeDamage() { }

        public override bool CanBeInterrupted() => false;
    }
}
