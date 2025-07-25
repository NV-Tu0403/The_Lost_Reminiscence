using UnityEngine;

namespace Code.Boss.States.Phase1
{
    /// <summary>
    /// Phase 1 - Lure State: Boss tiến lại gần người chơi rồi rút lui
    /// </summary>
    public class LureState : BossState
    {
        private enum LurePhase { Approaching, Retreating, Completed }
        private LurePhase currentPhase = LurePhase.Approaching;
        private Vector3 originalPosition;
        private Vector3 targetPosition;
        private float stateTimer;

        public override void Enter()
        {
            originalPosition = bossController.transform.position;
            currentPhase = LurePhase.Approaching;
            stateTimer = 0f;
            
            Debug.Log("[Boss State] Entered LureState - Boss tiến lại gần người chơi rồi rút lui");
            BossEventSystem.Trigger(BossEventType.LureStarted);
            
            CalculateTargetPosition();
        }

        public override void Update()
        {
            stateTimer += Time.deltaTime;
            
            switch (currentPhase)
            {
                case LurePhase.Approaching:
                    HandleApproaching();
                    break;
                case LurePhase.Retreating:
                    HandleRetreating();
                    break;
                case LurePhase.Completed:
                    // Transition to next state
                    bossController.ChangeState(new MockState());
                    break;
            }
        }

        private void CalculateTargetPosition()
        {
            Vector3 directionToPlayer = (bossController.Player.position - bossController.transform.position).normalized;
            targetPosition = bossController.Player.position - directionToPlayer * config.phase1.lureDistance;
        }

        private void HandleApproaching()
        {
            MoveTowards(targetPosition, config.phase1.lureApproachSpeed);
            
            float distanceToTarget = Vector3.Distance(bossController.transform.position, targetPosition);
            if (distanceToTarget < 0.5f || stateTimer > config.phase1.lureDuration * 0.6f)
            {
                currentPhase = LurePhase.Retreating;
            }
        }

        private void HandleRetreating()
        {
            MoveTowards(originalPosition, config.phase1.lureRetreatSpeed);
            
            float distanceToOriginal = Vector3.Distance(bossController.transform.position, originalPosition);
            if (distanceToOriginal < 0.5f || stateTimer > config.phase1.lureDuration)
            {
                currentPhase = LurePhase.Completed;
            }
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.speed = speed;
                bossController.NavAgent.SetDestination(target);
            }
        }

        public override void Exit()
        {
            // Reset movement speed
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.speed = config.moveSpeed;
            }
        }

        public override void OnTakeDamage()
        {
            // Boss bất khả xâm phạm trong Phase 1 - chỉ có thể damage qua decoys
            Debug.Log("[LureState] Boss is invulnerable in Phase 1! Can only damage through decoys.");
        }

        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
