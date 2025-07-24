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
            mockTimer = 0f;
            Debug.Log("[Boss State] Entered MockState - Boss phát tư thế vặn vẹo và tiếng cười méo mó");
            BossEventSystem.Trigger(BossEventType.MockStarted);
            
            // Play mock laugh sound
            if (config.audioConfig.mockLaughSound != null)
            {
                bossController.PlaySound(config.audioConfig.mockLaughSound, config.audioConfig.sfxVolume);
            }
            
            // Stop movement
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.SetDestination(bossController.transform.position);
            }
        }

        public override void Update()
        {
            mockTimer += Time.deltaTime;
            
            if (mockTimer >= config.phase1.mockDuration)
            {
                bossController.ChangeState(new DecoyState());
            }
        }

        public override void Exit()
        {
            // No special cleanup needed
        }

        public override void OnTakeDamage()
        {
            // Boss cannot take damage in Mock state
        }

        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
