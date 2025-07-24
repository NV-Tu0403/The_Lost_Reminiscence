using UnityEngine;

namespace Code.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Scream State: Phát âm thanh chê trách và hiệu ứng
    /// </summary>
    public class ScreamState : BossState
    {
        private float castTimer;
        private float skillTimer;
        private bool isCasting = true;
        private bool skillActivated = false;
        private bool playerAttacked = false;
        private bool playerHitBoss = false;

        public override void Enter()
        {
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            playerAttacked = false;
            playerHitBoss = false;
            
            Debug.Log("[Boss State] Entered ScreamState - Casting Scream skill");
            BossEventSystem.Trigger(BossEventType.ScreamStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted);
        }

        public override void Update()
        {
            if (isCasting)
            {
                HandleCasting();
            }
            else
            {
                HandleSkillActive();
            }
        }

        private void HandleCasting()
        {
            castTimer += Time.deltaTime;
            
            // Update skill cast progress for UI
            float progress = castTimer / config.phase2.screamCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            
            if (castTimer >= config.phase2.screamCastTime)
            {
                ActivateSkill();
            }
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            
            Debug.Log("[Boss State] ScreamState - Scream activated with effects");
            
            // Apply scream effects
            ApplyScreamEffects();
            
            // Play scream sound
            if (config.audioConfig.screamSound != null)
            {
                bossController.PlaySound(config.audioConfig.screamSound, config.audioConfig.sfxVolume);
            }
        }

        private void ApplyScreamEffects()
        {
            // Apply screen shake
            ApplyScreenShake();
            
            // Apply vision shrink
            ApplyVisionShrink();
        }

        private void ApplyScreenShake()
        {
            // This would integrate with a camera shake system
            // For now, trigger event for external system to handle
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "ScreenShake", floatValue = config.phase2.screenShakeIntensity });
        }

        private void ApplyVisionShrink()
        {
            // This would integrate with a camera/UI system
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "VisionShrink", floatValue = config.phase2.visionShrinkAmount });
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            
            if (skillTimer >= config.phase2.screamDuration)
            {
                EndScreamState();
            }
        }

        private void EndScreamState()
        {
            if (playerHitBoss)
            {
                // Player hit boss successfully - boss takes damage
                bossController.TakeDamage(1);
                bossController.ChangeState(new AngryState());
            }
            else
            {
                // Player missed or didn't attack - player takes damage and spawn soul
                BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(1));
                bossController.ChangeState(new Code.Boss.States.Shared.SoulState());
            }
        }

        public override void Exit()
        {
            // Remove scream effects
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "RemoveEffects" });
        }

        public override void OnTakeDamage()
        {
            if (isCasting && CanBeInterrupted())
            {
                BossEventSystem.Trigger(BossEventType.SkillInterrupted);
                bossController.ChangeState(new AngryState());
            }
            else if (skillActivated)
            {
                // Player hit boss during scream
                playerHitBoss = true;
                playerAttacked = true;
            }
        }

        public override bool CanBeInterrupted()
        {
            return isCasting;
        }

        // Method to be called when player attacks (hit or miss)
        public void OnPlayerAttack(bool hitBoss)
        {
            if (skillActivated)
            {
                playerAttacked = true;
                playerHitBoss = hitBoss;
            }
        }
    }
}
