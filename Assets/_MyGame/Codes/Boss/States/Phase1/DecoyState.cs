using _MyGame.Codes.Boss.CoreSystem;
using _MyGame.Codes.Boss.States.Phase1;
using UnityEngine;

namespace Code.Boss.States.Phase1
{
    /// <summary>
    /// Phase 1 - Decoy State: Spawn 2 bóng ảo (1 thật 1 giả) truy đuổi người chơi
    /// </summary>
    public class DecoyState : BossState
    {
        private float castTimer;
        private float skillTimer;
        private bool isCasting = true;
        private bool skillActivated;
        
        private GameObject realDecoy;
        private GameObject fakeDecoy;
        private GameObject decoyEffect1;
        private GameObject decoyEffect2;
        private bool decoyEffectSpawned;
        private GameObject realDecoyRevealEffectInstance;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered DecoyState - Spawn 2 bóng ảo (1 thật 1 giả) truy đuổi người chơi");
            BossController.PlayAnimation("CastSkillA");
            
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            BossEventSystem.Trigger(BossEventType.DecoyStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Decoy" });
            // Play decoy spawn sound (FMOD)
            BossController.PlayFMODOneShot(Config.fmodAudioConfig.decoySpawnEvent);
            // Đăng ký lắng nghe sự kiện GuideSignal
            BossEventSystem.Subscribe(BossEventType.FaSkillUsed, OnFaSkillUsed);
        }

        public override void Update()
        {
            if (isCasting) HandleCasting();
            else HandleSkillActive();
        }

        private void HandleCasting()
        {
            castTimer += Time.deltaTime;
            // Spawn decoy effect at the beginning of casting
            if (!decoyEffectSpawned)
            {
                SpawnDecoyCastEffects();
                decoyEffectSpawned = true;
            }
            // Update skill cast progress for UI
            var progress = castTimer / Config.phase1.decoyCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= Config.phase1.decoyCastTime) ActivateSkill();
        }

        private void SpawnDecoyCastEffects()
        {
            var spawnPoint1 = GameObject.Find("DecoySpawnPoint1");
            var spawnPoint2 = GameObject.Find("DecoySpawnPoint2");
            var point1 = spawnPoint1 != null ? spawnPoint1.transform.position : BossController.transform.position + Vector3.left * 2f;
            var point2 = spawnPoint2 != null ? spawnPoint2.transform.position : BossController.transform.position + Vector3.right * 2f;
            
            if (Config.phase1.decoySpawnEffectPrefab == null) return;
            
            var effectRotation = Quaternion.Euler(0, 90, 0);
            decoyEffect1 = Object.Instantiate(Config.phase1.decoySpawnEffectPrefab, point1, effectRotation);
            decoyEffect2 = Object.Instantiate(Config.phase1.decoySpawnEffectPrefab, point2, effectRotation);
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            BossEventSystem.Trigger(BossEventType.SkillInterrupted);
            SpawnDecoys();
            DisableDecoyEffects();
        }

        private void SpawnDecoys()
        {
            // Tìm 2 spawn point trong scene
            var spawnPoint1 = GameObject.Find("DecoySpawnPoint1");
            var spawnPoint2 = GameObject.Find("DecoySpawnPoint2");
            var point1 = spawnPoint1 != null ? spawnPoint1.transform.position : 
                BossController.transform.position + Vector3.left * 2f;
            var point2 = spawnPoint2 != null ? spawnPoint2.transform.position : 
                BossController.transform.position + Vector3.right * 2f;

            // Random vị trí real/fake decoy
            var realAtPoint1 = Random.value > 0.5f;
            if (realAtPoint1)
            {
                realDecoy = CreateDecoy(point1, true);
                fakeDecoy = CreateDecoy(point2, false);
            }
            else
            {
                realDecoy = CreateDecoy(point2, true);
                fakeDecoy = CreateDecoy(point1, false);
            }
            BossController.gameObject.SetActive(false);
        }
        
        private GameObject CreateDecoy(Vector3 position, bool isReal)
        {
            // Check if decoy prefab is assigned
            if (Config.phase1.decoyPrefab == null)
            {
                Debug.LogError("[DecoyState] Decoy prefab is not assigned in BossConfig! Please assign a decoy prefab in Phase1Config.");
                return null;
            }
            
            // Instantiate decoy from prefab
            var decoy = Object.Instantiate(Config.phase1.decoyPrefab, position, Quaternion.identity);
            decoy.name = isReal ? "RealDecoy" : "FakeDecoy";
            
            // Add decoy behavior
            var decoyBehavior = decoy.GetComponent<DecoyBehavior>();
            if (decoyBehavior == null)
                decoyBehavior = decoy.AddComponent<DecoyBehavior>();
                
            decoyBehavior.Initialize(BossController, isReal, Config.phase1.decoyMoveSpeed);
            
            BossController.AddDecoy(decoy);
            return decoy;
        }
        
        private void DisableDecoyEffects()
        {
            if (!decoyEffectSpawned) return;
            // Chờ 2s
            if (decoyEffect1 != null) 
                Object.Destroy(decoyEffect1, 2f);
            if (decoyEffect2 != null) 
                Object.Destroy(decoyEffect2, 2f);
            // Clear the decoy effects
            decoyEffectSpawned = false;
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            if (skillTimer >= Config.phase1.decoyDuration) EndDecoyState();
        }

        private void EndDecoyState()
        {
            BossController.ClearDecoys();
            BossController.gameObject.SetActive(true);
            BossController.ChangeState(new IdleState());
        }

        public override void Exit()
        {
            if (!skillActivated) return;
            BossController.ClearDecoys();
            BossController.gameObject.SetActive(true);
            BossEventSystem.Unsubscribe(BossEventType.FaSkillUsed, OnFaSkillUsed);
            // Xóa hiệu ứng nếu có
            if (realDecoyRevealEffectInstance == null) return;
            Object.Destroy(realDecoyRevealEffectInstance);
            realDecoyRevealEffectInstance = null;
            //
            // // Reset movement speed
            // if (BossController.NavAgent != null)
            // {
            //     BossController.NavAgent.speed = Config.moveSpeed;
            // }
            // BossController.ResetMoveDirection();
        }

        public override void OnTakeDamage() {}
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => isCasting;

        private void OnFaSkillUsed(BossEventData data)
        {
            if (data != null && data.stringValue == "GuideSignal")
            {
                RevealRealDecoy();
            }
        }

        private void RevealRealDecoy()
        {
            if (realDecoy == null) return;
            // Spawn hiệu ứng prefab nếu có cấu hình
            if (Config.phase1.realDecoyRevealEffectPrefab != null && realDecoyRevealEffectInstance == null)
            {
                realDecoyRevealEffectInstance = Object.Instantiate(
                    Config.phase1.realDecoyRevealEffectPrefab,
                    realDecoy.transform.position,
                    Quaternion.identity,
                    realDecoy.transform
                );
            }
            Debug.Log("[DecoyState] Real decoy revealed by GuideSignal!");
        }
    }
}
