using _MyGame.Codes.Boss.CoreSystem;
using _MyGame.Codes.Musical;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase1
{
    /// <summary>
    /// Phase 1 - Lure State: Boss tiến lại gần người chơi rồi rút lui
    /// </summary>
    public class LureState : BossState
    {
        private BossFinalAudio _audio;

        
        private enum LurePhase { Approaching, Retreating, Completed }
        private LurePhase _currentPhase = LurePhase.Approaching;
        private Vector3 _originalPosition;
        private Vector3 _targetPosition;
        private float _stateTimer;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered LureState - Boss tiến lại gần người chơi rồi rút lui");
            // Animation: set move direction toward player
            var dir = (BossController.Player.position - BossController.transform.position).normalized;
            BossController.SetMoveDirection(dir.x, dir.z);
            
            _originalPosition = BossController.transform.position;
            _currentPhase = LurePhase.Approaching;
            _stateTimer = 0f;
            
            BossEventSystem.Trigger(BossEventType.LureStarted);
            CalculateTargetPosition();
        }

        public override void Update()
        {
            _stateTimer += Time.deltaTime;

            switch (_currentPhase)
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
            _targetPosition = BossController.Player.position - directionToPlayer * Config.phase1.lureDistance;
        }

        private void HandleApproaching()
        {
            MoveTowards(_targetPosition, Config.phase1.lureApproachSpeed);
            var dir = (_targetPosition - BossController.transform.position).normalized;
            BossController.SetMoveDirection(dir.x, dir.z);
            
            var distanceToTarget = Vector3.Distance(BossController.transform.position, _targetPosition);
            if (distanceToTarget < 0.5f || _stateTimer > Config.phase1.lureDuration * 0.6f)
            {
                _currentPhase = LurePhase.Retreating;
            }
        }

        private void HandleRetreating()
        {
            MoveTowards(_originalPosition, Config.phase1.lureRetreatSpeed);
            var dir = (_originalPosition - BossController.transform.position).normalized;
            BossController.SetMoveDirection(dir.x, dir.z);
            
            var distanceToOriginal = Vector3.Distance(BossController.transform.position, _originalPosition);
            if (distanceToOriginal < 0.5f || _stateTimer > Config.phase1.lureDuration)
            {
                _currentPhase = LurePhase.Completed;
            }
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            if (BossController.NavAgent == null) return;
            BossController.NavAgent.speed = speed;
            BossController.NavAgent.SetDestination(target);
        }

        public override void Exit() { }
        public override void OnTakeDamage() { }
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => false;
    }
}
