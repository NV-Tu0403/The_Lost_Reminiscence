using UnityEngine;

namespace Code.Boss.States.Phase1
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
            idleTimer = 0f;
            Debug.Log("[Boss State] Entered IdleState - Boss đứng yên tại chỗ");
            
            // Stop movement
            if (bossController.NavAgent != null)
            {
                bossController.NavAgent.SetDestination(bossController.transform.position);
            }
        }

        public override void Update()
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= config.phase1.idleDuration && canTransition)
            {
                TransitionToNextState();
            }
        }

        private void TransitionToNextState()
        {
            if (config.phase1.enableRandomStates)
            {
                BossState nextState = GetRandomNextState();
                bossController.ChangeState(nextState);
            }
            else
            {
                // Default sequence: Idle -> Lure -> Mock -> Decoy
                bossController.ChangeState(new LureState());
            }
        }

        private BossState GetRandomNextState()
        {
            float[] weights = config.phase1.stateWeights;
            float totalWeight = weights[1] + weights[2] + weights[3]; // Exclude Idle weight
            float randomValue = Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            
            currentWeight += weights[1]; // Lure
            if (randomValue <= currentWeight)
                return new LureState();
                
            currentWeight += weights[2]; // Mock
            if (randomValue <= currentWeight)
                return new MockState();
                
            return new DecoyState(); // Decoy
        }

        public override void Exit()
        {
            // No special cleanup needed
        }

        public override void OnTakeDamage()
        {
            // Boss cannot take damage in Idle state (only in Decoy state)
        }

        public override bool CanBeInterrupted()
        {
            return false; // Idle state cannot be interrupted
        }
    }
}
