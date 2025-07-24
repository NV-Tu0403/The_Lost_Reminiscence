using UnityEngine;

namespace Code.Boss.States.Shared
{
    /// <summary>
    /// Soul State: Teleport và spawn soul (dùng cho cả Phase 1 và Phase 2)
    /// </summary>
    public class SoulState : BossState
    {
        private float castTimer;
        private bool isCasting = true;
        private Vector3 teleportPosition;

        public override void Enter()
        {
            castTimer = 0f;
            isCasting = true;
            
            Debug.Log("[Boss State] Entered SoulState - Teleporting and spawning soul");
            BossEventSystem.Trigger(BossEventType.SoulStateStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted);
            
            // Calculate teleport position
            CalculateTeleportPosition();
            
            // Play soul spawn sound
            if (config.audioConfig.soulSpawnSound != null)
            {
                bossController.PlaySound(config.audioConfig.soulSpawnSound, config.audioConfig.sfxVolume);
            }
        }

        public override void Update()
        {
            if (isCasting)
            {
                HandleCasting();
            }
            else
            {
                // Immediately transition after teleport and soul spawn
                TransitionToNextState();
            }
        }

        private void HandleCasting()
        {
            castTimer += Time.deltaTime;
            
            // Update skill cast progress for UI
            float progress = castTimer / config.phase1.soulStateCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            
            if (castTimer >= config.phase1.soulStateCastTime)
            {
                ExecuteTeleportAndSpawnSoul();
            }
        }

        private void CalculateTeleportPosition()
        {
            // Teleport to a random position away from player
            Vector3 playerPos = bossController.Player.position;
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float teleportDistance = config.soulConfig.soulSpawnRadius * 0.8f;
            
            teleportPosition = playerPos + new Vector3(randomDirection.x, 0, randomDirection.y) * teleportDistance;
        }

        private void ExecuteTeleportAndSpawnSoul()
        {
            isCasting = false;
            
            // Teleport boss
            bossController.transform.position = teleportPosition;
            Debug.Log("[Boss State] SoulState - Boss teleported and soul activated");
            
            // Spawn soul if under limit
            if (bossController.SoulManager.ActiveSoulCount < bossController.SoulManager.MaxSouls)
            {
                bossController.SoulManager.SpawnSoul();
            }
        }

        private void TransitionToNextState()
        {
            // Transition based on current phase
            if (bossController.CurrentPhase == 1)
            {
                bossController.ChangeState(new Code.Boss.States.Phase1.IdleState());
            }
            else // Phase 2
            {
                bossController.ChangeState(new Code.Boss.States.Phase2.AngryState());
            }
        }

        public override void Exit()
        {
            // No special cleanup needed
        }

        public override void OnTakeDamage()
        {
            if (isCasting && CanBeInterrupted())
            {
                BossEventSystem.Trigger(BossEventType.SkillInterrupted);
                TransitionToNextState();
            }
        }

        public override bool CanBeInterrupted()
        {
            return isCasting;
        }
    }
}
