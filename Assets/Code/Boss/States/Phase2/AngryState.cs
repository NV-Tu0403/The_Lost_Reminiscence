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
            centerPosition = bossController.NavMeshCenter != null ? 
                           bossController.NavMeshCenter.position : 
                           bossController.transform.position;
            
            Debug.Log("[Boss State] Entered AngryState - Boss di chuyển xoay quanh trung tâm NavMesh");
            
            // Set movement speed for angry state
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.speed = config.phase2.angryMoveSpeed;
            }
        }

        public override void Update()
        {
            stateTimer += Time.deltaTime;
            
            // Move in circle around center
            MoveInCircle();
            
            if (stateTimer >= config.phase2.angryMoveDuration && canTransition)
            {
                bossController.ChangeState(new FearZoneState());
            }
        }

        private void MoveInCircle()
        {
            var radius = config.phase2.circleRadius;
            currentAngle += (config.phase2.angryMoveSpeed / radius) * Time.deltaTime;
            
            var targetPosition = centerPosition + new Vector3(
                Mathf.Cos(currentAngle) * radius,
                0,
                Mathf.Sin(currentAngle) * radius
            );
            
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.SetDestination(targetPosition);
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

        public override void OnTakeDamage() { }

        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
