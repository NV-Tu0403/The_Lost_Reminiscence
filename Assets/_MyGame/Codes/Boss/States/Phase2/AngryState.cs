using Code.Boss;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Angry State: Boss di chuyển xoay quanh trung tâm NavMesh
    /// </summary>
    public class AngryState : BossState
    {
        private float stateTimer;
        private float currentAngle;
        private Vector3 centerPosition;
        private const bool CanTransition = true;

        public override void Enter()
        {
            stateTimer = 0f;
            centerPosition = BossController.NavMeshCenter != null ? 
                           BossController.NavMeshCenter.position : 
                           BossController.transform.position;
            
            Debug.Log("[Boss State] Entered AngryState - Boss di chuyển xoay quanh trung tâm NavMesh");
            // Animation: set move direction 
            var tangent = new Vector3(-Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle));
            BossController.SetMoveDirection(tangent.x, tangent.z);
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
            
            if (stateTimer >= Config.phase2.angryMoveDuration && CanTransition)
            {
                BossController.ChangeState(new FearZoneState());
            }
        }

        private void MoveInCircle()
        {
            // Guard against zero/near-zero radius
            var radius = Mathf.Max(0.1f, Config.phase2.circleRadius);
            currentAngle += (Config.phase2.angryMoveSpeed / radius) * Time.deltaTime;
            
            var targetPosition = centerPosition + new Vector3(
                Mathf.Cos(currentAngle) * radius,
                0,
                Mathf.Sin(currentAngle) * radius
            );
            // Cập nhật hướng di chuyển cho animation
            var tangent = new Vector3(-Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle));
            BossController.SetMoveDirection(tangent.x, tangent.z);
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.SetDestination(targetPosition);
            }
        }
        
        public override void Exit()
        {
        }

        public override void OnTakeDamage() { }
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => false;
    }
}
