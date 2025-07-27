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
            var weights = config.phase1.stateWeights;
            var totalWeight = weights[1] + weights[2] + weights[3]; // Exclude Idle weight
            var randomValue = Random.Range(0f, totalWeight);
            var currentWeight = 0f;
            
            currentWeight += weights[1]; // Lure
            if (randomValue <= currentWeight)
                return new LureState();
                
            currentWeight += weights[2]; // Mock
            if (randomValue <= currentWeight)
                return new MockState();
                
            return new DecoyState(); // Decoy
        }

        public override void Exit() {}

        public override void OnTakeDamage() {}
        
        public override bool CanBeInterrupted()
        {
            return false; 
        }
    }
}
