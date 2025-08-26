using Code.Boss;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase1
{
    /// <summary>
    /// Phase 1 - Idle State: Boss đứng yên tại chỗ
    /// </summary>
    public class IdleState : BossState
    {
        private float idleTimer;
        private bool canTransition = true;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered IdleState - Boss đứng yên tại chỗ");
            BossController.PlayAnimation("Idle");
            
            idleTimer = 0f;
            // Stop movement
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.SetDestination(BossController.transform.position);
            }
        }

        public override void Update()
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= Config.phase1.idleDuration && canTransition)
            {
                BossController.ChangeState(new LureState());
            }
        }


        public override void Exit() {}

        public override void OnTakeDamage() {}
        
        public override bool CanBeInterrupted() => false;
    }
}
