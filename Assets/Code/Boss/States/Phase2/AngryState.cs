using UnityEngine;

namespace Code.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Angry State: Boss di chuyển xoay quanh trung tâm NavMesh
    /// </summary>
    public class AngryState : BossState
    {
        private float stateTimer;
        private float currentAngle = 0f;
        private Vector3 centerPosition;
        private bool canTransition = true;

        public override void Enter()
        {
            stateTimer = 0f;
            centerPosition = BossController.NavMeshCenter != null ? 
                           BossController.NavMeshCenter.position : 
                           BossController.transform.position;
            
            Debug.Log("[Boss State] Entered AngryState - Boss di chuyển xoay quanh trung tâm NavMesh");
            // animation
           // BossController.PlayAnimation("Angry");
            
            // Set movement speed for angry state
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.speed = Config.phase2.angryMoveSpeed;
            }
        }

        public override void Update()
        {
            stateTimer += Time.deltaTime;
            
            // Move in circle around center
            MoveInCircle();
            
            if (stateTimer >= Config.phase2.angryMoveDuration && canTransition)
            {
                BossController.ChangeState(new FearZoneState());
            }
        }

        private void MoveInCircle()
        {
            var radius = Config.phase2.circleRadius;
            currentAngle += (Config.phase2.angryMoveSpeed / radius) * Time.deltaTime;
            
            var targetPosition = centerPosition + new Vector3(
                Mathf.Cos(currentAngle) * radius,
                0,
                Mathf.Sin(currentAngle) * radius
            );
            
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.SetDestination(targetPosition);
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
