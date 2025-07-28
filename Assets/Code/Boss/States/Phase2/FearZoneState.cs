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
        private bool skillActivated = false;
        private GameObject fearZone;
        private Vector3 fearZonePosition;
        private bool playerInZone = false;
        private float playerInZoneTime = 0f;

        public override void Enter()
        {
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            playerInZone = false;
            playerInZoneTime = 0f;
            
            Debug.Log("[Boss State] Entered FearZoneState - Casting Fear Zone");
            BossEventSystem.Trigger(BossEventType.FearZoneCreated);
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "Fear Zone" });
            
            // Record player position for fear zone
            fearZonePosition = bossController.Player.position;
            
            // Play fear zone sound
            if (config.audioConfig.fearZoneSound != null)
            {
                bossController.PlaySound(config.audioConfig.fearZoneSound, config.audioConfig.ambientVolume);
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
            var progress = castTimer / config.phase2.fearZoneCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= config.phase2.fearZoneCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            
            // Trigger event để ẩn UI cast bar khi skill hoàn thành
            BossEventSystem.Trigger(BossEventType.SkillInterrupted); // Ẩn UI cast bar
            CreateFearZone();
            Debug.Log("[Boss State] FearZoneState - Fear zone activated");
        }

        private void CreateFearZone()
        {
            // Create fear zone GameObject
            fearZone = new GameObject("FearZone")
            {
                transform =
                {
                    position = fearZonePosition
                }
            };

            // Add fear zone behavior
            var fearZoneBehavior = fearZone.AddComponent<FearZoneBehavior>();
            fearZoneBehavior.Initialize(config.phase2.fearZoneRadius, config.phase2.visionBlurIntensity);
            
            // Add trigger collider
            var collider = fearZone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = config.phase2.fearZoneRadius;
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            // Check if player is in fear zone
            CheckPlayerInZone();
            if (skillTimer >= config.phase2.fearZoneDuration) EndFearZoneState();
        }

        private void CheckPlayerInZone()
        {
            if (fearZone == null) return;
            
            var distanceToPlayer = Vector3.Distance(fearZonePosition, bossController.Player.position);
            var currentlyInZone = distanceToPlayer <= config.phase2.fearZoneRadius;
            
            if (currentlyInZone)
            {
                if (!playerInZone)
                {
                    playerInZone = true;
                    playerInZoneTime = 0f;
                    StartHeartbeatSound();
                }
                
                playerInZoneTime += Time.deltaTime;
                
                if (playerInZoneTime >= config.phase2.fearZoneActivationTime)
                {
                    ApplyFearEffects();
                }
            }
            else
            {
                if (!playerInZone) return;
                playerInZone = false;
                playerInZoneTime = 0f;
                StopHeartbeatSound();
            }
        }

        private void StartHeartbeatSound()
        {
            if (config.audioConfig.heartbeatSound != null)
            {
                bossController.AudioSource.clip = config.audioConfig.heartbeatSound;
                bossController.AudioSource.loop = true;
                bossController.AudioSource.Play();
            }
        }

        private void StopHeartbeatSound()
        {
            bossController.AudioSource.Stop();
            bossController.AudioSource.loop = false;
        }

        private void ApplyFearEffects()
        {
            // Apply vision blur and other fear effects
            // This would integrate with a camera effects system
            BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(0)); // Psychological damage
        }

        private void EndFearZoneState()
        {
            if (fearZone != null)  Object.Destroy(fearZone);
            
            StopHeartbeatSound();
            bossController.ChangeState(new ScreamState());
        }

        public override void Exit()
        { 
            if (fearZone != null) Object.Destroy(fearZone);
            StopHeartbeatSound();
        }

        public override void OnTakeDamage() {}
        
        public override bool CanBeInterrupted()
        {
            return false;
        }
    }
}
