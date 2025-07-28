using UnityEngine;

namespace Code.Boss.States.Phase1
{
    /// <summary>
    /// Phase 1 - Mock State: Boss phát tư thế vặn vẹo và tiếng cười méo mó
    /// </summary>
    public class MockState : BossState
    {
        private float mockTimer;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered MockState - Boss phát tư thế vặn vẹo và tiếng cười méo mó");

            // Set boss animation to mock
            // BossController.PlayAnimation("Mock");
            
            mockTimer = 0f;
            BossEventSystem.Trigger(BossEventType.MockStarted);
            
            // Play mock laugh sound
            if (Config.audioConfig.mockLaughSound != null)
            {
                BossController.PlaySound(Config.audioConfig.mockLaughSound, Config.audioConfig.sfxVolume);
            }
            
            // Stop movement
            if (BossController.NavAgent != null)
            {
                BossController.NavAgent.SetDestination(BossController.transform.position);
            }
        }

        public override void Update()
        {
            mockTimer += Time.deltaTime;
            
            if (mockTimer >= Config.phase1.mockDuration)
            {
                BossController.ChangeState(new DecoyState());
            }
        }

        public override void Exit() { }

        public override void OnTakeDamage() { }

        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
