using _MyGame.Codes.Boss.CoreSystem;
using _MyGame.Codes.Musical;
using Code.Boss;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase1
{
    /// <summary>
    /// Phase 1 - Idle State: Boss đứng yên tại chỗ
    /// </summary>
    public class IdleState : BossState
    {
        private float _idleTimer;
        private const bool CanTransition = true;
        
        public override void Enter()
        {
            //Debug.Log("[Boss State] Entered IdleState - Boss đứng yên tại chỗ");
            BossController.PlayAnimation("Idle");
            
            _idleTimer = 0f;
            // Stop movement
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.SetDestination(BossController.transform.position);
            }
        }

        public override void Update()
        {
            _idleTimer += Time.deltaTime;
            
            if (_idleTimer >= Config.phase1.idleDuration && CanTransition)
            {
                BossController.ChangeState(new LureState());
            }
        }
        
        public override void Exit() {}

        public override void OnTakeDamage() {}
        
        public override bool CanBeInterrupted() => false;
    }
}
