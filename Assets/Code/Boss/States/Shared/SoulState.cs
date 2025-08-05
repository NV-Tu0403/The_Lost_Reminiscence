using UnityEngine;

namespace Code.Boss.States.Shared
{
    /// <summary>
    /// Soul State: Teleport và spawn soul (dùng cho cả Phase 1 và Phase 2)
    /// </summary>
    public class SoulState : BossState
    {
        private float castTimer;
        private float skillTimer;
        private bool isCasting = true;
        
        
        private Vector3 teleportPosition;
        private Vector3 soulSpawnPosition;
        private GameObject soulSpawnEffect;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered SoulState - Teleporting and spawning soul");
            BossController.PlayAnimation("CastSkillB");
            
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            BossEventSystem.Trigger(BossEventType.SoulStateStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Soul" });

            // Lấy vị trí spawn từ GameObject trong scene
            var spawnPoint = GameObject.Find("SoulSpawnPoint");
            soulSpawnPosition = spawnPoint != null
                ? spawnPoint.transform.position
                : BossController.transform.position + Vector3.forward * 3f;

            // Nếu có prefab hiệu ứng spawn soul, instantiate nó
            if (Config.soulConfig.soulSpawnEffectPrefab != null)
            {
                var effectRotation = Quaternion.Euler(0, 90, 0);
                soulSpawnEffect = Object.Instantiate(Config.soulConfig.soulSpawnEffectPrefab, soulSpawnPosition,
                    effectRotation);
            }

            // Play soul spawn sound
            if (Config.audioConfig.soulSpawnSound != null)
            {
                BossController.PlaySound(Config.audioConfig.soulSpawnSound, Config.audioConfig.sfxVolume);
            }
        }

        public override void Update()
        {
            if (isCasting) HandleCasting();
            else HandleSkillActive();
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;

            // Check if skill is active for a certain duration
            if (!(skillTimer >= Config.phase1.soulStateCastTime)) return;
            Debug.Log("[Boss State] SoulState - Skill duration ended, transitioning to next state");
            TransitionToNextState();
        }

        private void HandleCasting()
        {
            castTimer += Time.deltaTime;
            // Update skill cast progress for UI
            var progress = castTimer / Config.phase1.soulStateCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= Config.phase1.soulStateCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            isCasting = false;
            Debug.Log("[Boss State] SoulState - Soul spawned at point");
            BossEventSystem.Trigger(BossEventType.SkillInterrupted);
            if (BossController.SoulManager.ActiveSoulCount < BossController.SoulManager.MaxSouls)
            {
                BossController.SoulManager.SpawnSoul(soulSpawnPosition);
            }

            // Xóa hiệu ứng spawn nếu có
            if (soulSpawnEffect == null) return;
            Object.Destroy(soulSpawnEffect);
            soulSpawnEffect = null;
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
            // // Reset movement speed
            // if (BossController.NavAgent != null)
            // {
            //     BossController.NavAgent.speed = Config.moveSpeed;
            // }
            // BossController.ResetMoveDirection();
        }
        public override void OnTakeDamage() { }
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => isCasting;
    }
}
