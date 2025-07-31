using System.Collections.Generic;
using UnityEngine;

namespace Code.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Fear Zone State: Tạo vùng tối dưới chân người chơi
    /// </summary>
    public class FearZoneState : BossState
    {
        private float castTimer;
        private float skillTimer;
        private bool isCasting = true;


        private GameObject fearZoneCastEffect;
        private GameObject fearZoneZoneEffect;
        private GameObject fearZone;
        private Vector3 fearZonePosition;

        private bool playerInZone = false;
        private float playerInZoneTime = 0f;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered FearZoneState");
            BossController.PlayAnimation("CastSkillA");
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            BossEventSystem.Trigger(BossEventType.FearZoneCreated);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Fear Zone" });
            fearZonePosition = BossController.Player.position;
            
            if (Config.phase2.fearZoneCastEffectPrefab != null)
            {
                fearZoneCastEffect = Object.Instantiate(Config.phase2.fearZoneCastEffectPrefab, fearZonePosition, Quaternion.identity);
            }
            if (Config.audioConfig.fearZoneSound != null)
            {
                BossController.PlaySound(Config.audioConfig.fearZoneSound, Config.audioConfig.ambientVolume);
            }
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
            var progress = castTimer / Config.phase2.fearZoneCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= Config.phase2.fearZoneCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            isCasting = false;
            BossEventSystem.Trigger(BossEventType.SkillInterrupted);
            if (Config.phase2.fearZoneZoneEffectPrefab != null)
            {
                fearZoneZoneEffect = Object.Instantiate(Config.phase2.fearZoneZoneEffectPrefab, fearZonePosition, Quaternion.identity);
            }
            if (fearZoneCastEffect != null)
            {
                Object.Destroy(fearZoneCastEffect);
                fearZoneCastEffect = null;
            }
            CreateFearZone();
        }
        
        private void CreateFearZone()
        {
            if (fearZone != null)
            {
                Object.Destroy(fearZone);
            }
            fearZone = new GameObject("FearZone")
            {
                transform =
                {
                    position = fearZonePosition
                }
            };
            var collider = fearZone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = Config.phase2.fearZoneRadius;
        }


        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            CheckPlayerInZone();
            if (skillTimer >= Config.phase2.fearZoneDuration) BossController.ChangeState(new ScreamState());
        }
        
        private void CheckPlayerInZone()
        {
            if (fearZone == null) return;
            var distanceToPlayer = Vector3.Distance(fearZone.transform.position, BossController.Player.position);
            var currentlyInZone = distanceToPlayer <= Config.phase2.fearZoneRadius;
            switch (currentlyInZone)
            {
                case true when !playerInZone:
                    playerInZone = true;
                    playerInZoneTime = 0f;
                    ApplyFearEffects(true);
                    StartHeartbeatSound();
                    break;
                case false when playerInZone:
                    playerInZone = false;
                    playerInZoneTime = 0f;
                    ApplyFearEffects(false);
                    StopHeartbeatSound();
                    break;
            }
            if (playerInZone)
            {
                playerInZoneTime += Time.deltaTime;
            }
        }
        
        private void ApplyFearEffects(bool enable)
        {
            Debug.Log("[Boss State] Player " + (enable ? "entered" : "left") + " fear zone");
        }
        
        private void StartHeartbeatSound()
        {
            if (Config.audioConfig.heartbeatSound == null) return;
            BossController.AudioSource.clip = Config.audioConfig.heartbeatSound;
            BossController.AudioSource.loop = true;
            BossController.AudioSource.Play();
        }
        
        private void StopHeartbeatSound()
        {
            BossController.AudioSource.Stop();
            BossController.AudioSource.loop = false;
        }
        
        public override void Exit()
        {
            if (fearZone != null) Object.Destroy(fearZone);
            if (fearZoneZoneEffect != null) Object.Destroy(fearZoneZoneEffect);
            if (fearZoneCastEffect != null) Object.Destroy(fearZoneCastEffect);
        }

        public override void OnTakeDamage() {}
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => false;
    }
}
