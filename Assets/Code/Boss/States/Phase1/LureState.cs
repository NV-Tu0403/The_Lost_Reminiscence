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
            Debug.Log("[Boss State] Entered LureState - Boss tiến lại gần người chơi rồi rút lui");
            originalPosition = BossController.transform.position;
            currentPhase = LurePhase.Approaching;
            stateTimer = 0f;
            
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
                // Transition to next state
                case LurePhase.Completed:
                    BossController.ChangeState(new MockState());
                    break;
            }
        }

        private void CalculateTargetPosition()
        {
            var directionToPlayer = (BossController.Player.position - BossController.transform.position).normalized;
            targetPosition = BossController.Player.position - directionToPlayer * Config.phase1.lureDistance;
        }

        private void HandleApproaching()
        {
            MoveTowards(targetPosition, Config.phase1.lureApproachSpeed);
            
            var distanceToTarget = Vector3.Distance(BossController.transform.position, targetPosition);
            if (distanceToTarget < 0.5f || stateTimer > Config.phase1.lureDuration * 0.6f)
            {
                currentPhase = LurePhase.Retreating;
            }
        }

        private void HandleRetreating()
        {
            MoveTowards(originalPosition, Config.phase1.lureRetreatSpeed);
            
            var distanceToOriginal = Vector3.Distance(BossController.transform.position, originalPosition);
            if (distanceToOriginal < 0.5f || stateTimer > Config.phase1.lureDuration)
            {
                currentPhase = LurePhase.Completed;
            }
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.speed = speed;
                BossController.NavAgent.SetDestination(target);
            }
        }

        public override void Exit()
        {
            // Reset movement speed
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.speed = Config.moveSpeed;
            }
        }

        public override void OnTakeDamage() { }
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => false;
    }
}
