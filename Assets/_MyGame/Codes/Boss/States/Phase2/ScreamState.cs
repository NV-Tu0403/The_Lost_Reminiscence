using _MyGame.Codes.Boss.CoreSystem;
using _MyGame.Codes.Boss.States.Phase2;
using Code.Boss.States.Shared;
using DG.Tweening;
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
        private bool playerHitBoss = false;

        public override void Enter()
        {
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            playerHitBoss = false;
            
            Debug.Log("[Boss State] Entered ScreamState - Casting Scream skill");
            BossController.PlayAnimation("CastSkillA");
            
            
            BossEventSystem.Trigger(BossEventType.ScreamStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Scream" });
        }

        public override void Update()
        {
            if (isCasting) HandleCasting();
            else HandleSkillActive();
        }

        private void HandleCasting()
        {
            castTimer += Time.deltaTime;
            
            // Update skill cast progress for UI
            var progress = castTimer / Config.phase2.screamCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= Config.phase2.screamCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            
            BossEventSystem.Trigger(BossEventType.SkillInterrupted); // Ẩn UI cast bar
            Debug.Log("[Boss State] ScreamState - Scream activated with effects");
            
            // Apply scream effects
            ApplyScreamEffects();
            
            // Play scream sound
            if (Config.audioConfig.screamSound != null)
            {
                BossController.PlaySound(Config.audioConfig.screamSound, Config.audioConfig.sfxVolume);
            }
        }

        private void ApplyScreamEffects()
        {
            ApplyScreenShake();     
        }

        private void ApplyScreenShake()
        {
            var mainCam = Camera.main;
            if (mainCam == null) return;
            var config = BossController.Config.phase2;
            var originalPos = mainCam.transform.position;
            mainCam.transform.DOShakePosition(
                config.shakeDuration,
                config.shakeStrength,
                config.shakeVibrato,
                config.shakeRandomness,
                false,
                true)
                .OnComplete(() => mainCam.transform.position = originalPos);
            // Spawn shake effect prefab if assigned
            if (config.shakeEffectPrefab != null)
            {
                Object.Instantiate(config.shakeEffectPrefab, mainCam.transform.position, Quaternion.identity);
            }
        }
        

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            
            if (skillTimer >= Config.phase2.screamDuration)
            {
                EndScreamState();
            }
        }

        private void EndScreamState()
        {
            if (playerHitBoss)
            {
                BossController.TakeDamage(1);
                BossController.ChangeState(new AngryState());
            }
            else
            {
                BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(1));
                BossController.ChangeState(new SoulState());
            }
        }

        public override void Exit()
        {
            // Tắt hiệu ứng shake
            var mainCam = Camera.main;
            if (mainCam == null) return;
            // Đưa camera về vị trí ban đầu
            mainCam.transform.DOKill();
            // Nếu có hiệu ứng shakeEffectPrefab thì tìm và xóa
            var config = BossController.Config;
            if (config == null || config.phase2.shakeEffectPrefab == null) return;
            var shakeEffectName = config.phase2.shakeEffectPrefab.name;
            var shakeEffects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in shakeEffects)
            {
                if (obj.name.Contains(shakeEffectName))
                {
                    Object.Destroy(obj);
                }
            }
        }

        public override void OnTakeDamage()
        {
            if (isCasting && CanBeInterrupted())
            {
                BossEventSystem.Trigger(BossEventType.SkillInterrupted);
                BossController.ChangeState(new AngryState());
            }
            else if (skillActivated)
            {
                playerHitBoss = true;
            }
        }

        public override bool CanBeInterrupted() => true;
        public override bool CanTakeDamage() => isCasting;
    }
}
