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
            // BossController.PlayAnimation("Soul");
            BossEventSystem.Trigger(BossEventType.SoulStateStarted);
            
            // Trigger skill cast với skill name để UI hiển thị
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Soul Teleport" });
            
            // Calculate teleport position
            CalculateTeleportPosition();
            
            // Play soul spawn sound
            if (Config.audioConfig.soulSpawnSound != null)
            {
                BossController.PlaySound(Config.audioConfig.soulSpawnSound, Config.audioConfig.sfxVolume);
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
            float progress = castTimer / Config.phase1.soulStateCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            
            if (castTimer >= Config.phase1.soulStateCastTime)
            {
                ExecuteTeleportAndSpawnSoul();
            }
        }

        private void CalculateTeleportPosition()
        {
            // Teleport to a random position away from player
            Vector3 playerPos = BossController.Player.position;
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float teleportDistance = Config.soulConfig.soulSpawnRadius * 0.8f;
            
            teleportPosition = playerPos + new Vector3(randomDirection.x, 0, randomDirection.y) * teleportDistance;
        }

        private void ExecuteTeleportAndSpawnSoul()
        {
            isCasting = false;
            
            // Teleport boss
            BossController.transform.position = teleportPosition;
            Debug.Log("[Boss State] SoulState - Boss teleported and soul activated");
            
            // Spawn soul if under limit
            if (BossController.SoulManager.ActiveSoulCount < BossController.SoulManager.MaxSouls)
            {
                BossController.SoulManager.SpawnSoul();
            }
        }

        private void TransitionToNextState()
        {
            // Transition based on current phase
            if (BossController.CurrentPhase == 1)
            {
                BossController.ChangeState(new Phase1.IdleState());
            }
            else // Phase 2
            {
                BossController.ChangeState(new Phase2.AngryState());
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
