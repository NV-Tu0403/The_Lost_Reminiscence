using _MyGame.Codes.Boss.CoreSystem;
using Code.Boss;
using Code.Boss.States.Phase2;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase2
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
        private GameObject playerFearEffect; // Effect bao quanh player

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
            
            if (enable)
            {
                // Spawn effect bao quanh player khi vào fear zone
                if (Config.phase2.fearZonePlayerEffectPrefab != null && playerFearEffect == null)
                {
                    playerFearEffect = Object.Instantiate(Config.phase2.fearZonePlayerEffectPrefab, 
                        BossController.Player.position, 
                        Quaternion.identity, 
                        BossController.Player);
                }
                
                // Giảm tốc độ di chuyển của player
                var playerController = BossController.Player.GetComponent<PlayerController_02>();
                if (playerController != null)
                {
                    playerController.ReduceMovementSpeed(0.3f); // Giảm 70% tốc độ
                }
            }
            else
            {
                // Destroy effect khi player rời khỏi fear zone
                if (playerFearEffect != null)
                {
                    Object.Destroy(playerFearEffect);
                    playerFearEffect = null;
                }
                
                // Khôi phục tốc độ di chuyển của player
                var playerController = BossController.Player.GetComponent<PlayerController_02>();
                if (playerController != null)
                {
                    playerController.RestoreMovementSpeed();
                }
            }
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
            StopHeartbeatSound();
            
            // Cleanup tốc độ player nếu vẫn đang bị giảm
            if (playerInZone)
            {
                var playerController = BossController.Player.GetComponent<PlayerController_02>();
                if (playerController != null)
                {
                    playerController.RestoreMovementSpeed();
                }
            }
            
            // Cleanup tất cả effect ngay lập tức
            if (playerFearEffect != null)
            {
                Object.DestroyImmediate(playerFearEffect);
                playerFearEffect = null;
            }
            
            if (fearZoneCastEffect != null)
            {
                Object.DestroyImmediate(fearZoneCastEffect);
                fearZoneCastEffect = null;
            }
            
            if (fearZoneZoneEffect != null)
            {
                Object.DestroyImmediate(fearZoneZoneEffect);
                fearZoneZoneEffect = null;
            }
            
            if (fearZone != null)
            {
                Object.DestroyImmediate(fearZone);
                fearZone = null;
            }
            
            // Reset trạng thái
            playerInZone = false;
            playerInZoneTime = 0f;
            isCasting = true;
            castTimer = 0f;
            skillTimer = 0f;
        }

        public override void OnTakeDamage() {}
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => false;
    }
}
