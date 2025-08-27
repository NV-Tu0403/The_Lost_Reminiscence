using _MyGame.Codes.Boss.CoreSystem;
using UnityEngine;

namespace _MyGame.Codes.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Fear Zone State: Tạo vùng tối dưới chân người chơi
    /// </summary>
    public class FearZoneState : BossState
    {
        private float _castTimer;
        private float _skillTimer;
        private bool _isCasting = true;


        private GameObject _fearZoneCastEffect;
        private GameObject _fearZoneZoneEffect;
        private GameObject _fearZone;
        private Vector3 _fearZonePosition;

        private bool _playerInZone = false;
        private float _playerInZoneTime = 0f;
        private GameObject _playerFearEffect; // Effect bao quanh player

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered FearZoneState");
            BossController.PlayAnimation("CastSkillA");
            
            _castTimer = 0f;
            _skillTimer = 0f;
            _isCasting = true;
            BossEventSystem.Trigger(BossEventType.FearZoneCreated);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "VÙNG KHIẾP SỢ" });
            _fearZonePosition = BossController.Player.position;
            
            if (Config.phase2.fearZoneCastEffectPrefab != null)
            {
                _fearZoneCastEffect = Object.Instantiate(Config.phase2.fearZoneCastEffectPrefab, _fearZonePosition, Quaternion.identity);
            }
            
            // FMOD one-shot for fear zone cast
            BossController.PlayFMODOneShot(Config.fmodAudioConfig.fearZoneEvent);
        }

        public override void Update()
        {
            if (_isCasting) HandleCasting();
            else HandleSkillActive();
        }

        private void HandleCasting()
        {
            _castTimer += Time.deltaTime;
            
            // Update skill cast progress for UI
            var progress = _castTimer / Config.phase2.fearZoneCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (_castTimer >= Config.phase2.fearZoneCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            _isCasting = false;
            BossEventSystem.Trigger(BossEventType.SkillInterrupted);
            if (Config.phase2.fearZoneZoneEffectPrefab != null)
            {
                _fearZoneZoneEffect = Object.Instantiate(Config.phase2.fearZoneZoneEffectPrefab, _fearZonePosition, Quaternion.identity);
            }
            if (_fearZoneCastEffect != null)
            {
                Object.Destroy(_fearZoneCastEffect);
                _fearZoneCastEffect = null;
            }
            CreateFearZone();
        }
        
        private void CreateFearZone()
        {
            if (_fearZone != null)
            {
                Object.Destroy(_fearZone);
            }
            _fearZone = new GameObject("FearZone")
            {
                transform =
                {
                    position = _fearZonePosition
                }
            };
            var collider = _fearZone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = Config.phase2.fearZoneRadius;
        }


        private void HandleSkillActive()
        {
            _skillTimer += Time.deltaTime;
            CheckPlayerInZone();
            if (_skillTimer >= Config.phase2.fearZoneDuration) BossController.ChangeState(new ScreamState());
        }
        
        private void CheckPlayerInZone()
        {
            if (_fearZone == null) return;
            var distanceToPlayer = Vector3.Distance(_fearZone.transform.position, BossController.Player.position);
            var currentlyInZone = distanceToPlayer <= Config.phase2.fearZoneRadius;
            switch (currentlyInZone)
            {
                case true when !_playerInZone:
                    _playerInZone = true;
                    _playerInZoneTime = 0f;
                    ApplyFearEffects(true);
                    StartHeartbeatSound();
                    BossEventSystem.Trigger(BossEventType.PlayerEnteredFearZone);
                    break;
                case false when _playerInZone:
                    _playerInZone = false;
                    _playerInZoneTime = 0f;
                    ApplyFearEffects(false);
                    StopHeartbeatSound();
                    BossEventSystem.Trigger(BossEventType.PlayerExitedFearZone);
                    break;
            }
            if (_playerInZone)
            {
                _playerInZoneTime += Time.deltaTime;
            }
        }
        
        private void ApplyFearEffects(bool enable)
        {
            Debug.Log("[Boss State] Player " + (enable ? "entered" : "left") + " fear zone");
            
            if (enable)
            {
                // Spawn effect bao quanh player khi vào fear zone
                if (Config.phase2.fearZonePlayerEffectPrefab != null && _playerFearEffect == null)
                {
                    _playerFearEffect = Object.Instantiate(Config.phase2.fearZonePlayerEffectPrefab, 
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
                if (_playerFearEffect != null)
                {
                    Object.Destroy(_playerFearEffect);
                    _playerFearEffect = null;
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
            // Use FMOD looping instance via BossController helper
            BossController.StartHeartbeatLoop();
        }
        
        private void StopHeartbeatSound()
        {
            BossController.StopHeartbeatLoop();
        }
        
        public override void Exit()
        {
            StopHeartbeatSound();
            if (_playerInZone)
            {
                BossEventSystem.Trigger(BossEventType.PlayerExitedFearZone);
            }
            
            // Cleanup tốc độ player nếu vẫn đang bị giảm
            if (_playerInZone)
            {
                var playerController = BossController.Player.GetComponent<PlayerController_02>();
                if (playerController != null)
                {
                    playerController.RestoreMovementSpeed();
                }
            }
            
            // Cleanup tất cả effect ngay lập tức
            if (_playerFearEffect != null)
            {
                Object.DestroyImmediate(_playerFearEffect);
                _playerFearEffect = null;
            }
            
            if (_fearZoneCastEffect != null)
            {
                Object.DestroyImmediate(_fearZoneCastEffect);
                _fearZoneCastEffect = null;
            }
            
            if (_fearZoneZoneEffect != null)
            {
                Object.DestroyImmediate(_fearZoneZoneEffect);
                _fearZoneZoneEffect = null;
            }
            
            if (_fearZone != null)
            {
                Object.DestroyImmediate(_fearZone);
                _fearZone = null;
            }
            
            // Reset trạng thái
            _playerInZone = false;
            _playerInZoneTime = 0f;
            _isCasting = true;
            _castTimer = 0f;
            _skillTimer = 0f;
        }

        public override void OnTakeDamage() {}
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => false;
    }
}
