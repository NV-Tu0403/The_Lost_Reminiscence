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
        private bool skillActivated = false;
        private GameObject realDecoy;
        private GameObject fakeDecoy;

        public override void Enter()
        {
            Debug.Log("[Boss State] Entered DecoyState - Spawn 2 bóng ảo (1 thật 1 giả) truy đuổi người chơi");
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            BossEventSystem.Trigger(BossEventType.DecoyStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Decoy" });
            
            // Play decoy spawn sound
            if (config.audioConfig.decoySpawnSound != null)
            {
                bossController.PlaySound(config.audioConfig.decoySpawnSound, config.audioConfig.sfxVolume);
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
            var progress = castTimer / config.phase1.decoyCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= config.phase1.decoyCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            BossEventSystem.Trigger(BossEventType.SkillInterrupted);
            SpawnDecoys();
        }

        private void SpawnDecoys()
        {
            var spawnCenter = bossController.Player.position;
            var spawnRadius = config.phase1.decoySpawnRadius;
            
            // Spawn real decoy (this is actually the boss)
            var realPos = GetRandomSpawnPosition(spawnCenter, spawnRadius);
            realDecoy = CreateDecoy(realPos, true);
            
            // Spawn fake decoy
            var fakePos = GetRandomSpawnPosition(spawnCenter, spawnRadius);
            fakeDecoy = CreateDecoy(fakePos, false);
            
            // Hide original boss
            bossController.gameObject.SetActive(false);
        }

        private static Vector3 GetRandomSpawnPosition(Vector3 center, float radius)
        {
            var randomCircle = Random.insideUnitCircle.normalized * radius;
            return center + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        private GameObject CreateDecoy(Vector3 position, bool isReal)
        {
            // Check if decoy prefab is assigned
            if (config.phase1.decoyPrefab == null)
            {
                Debug.LogError("[DecoyState] Decoy prefab is not assigned in BossConfig! Please assign a decoy prefab in Phase1Config.");
                return null;
            }
            
            // Instantiate decoy from prefab
            var decoy = Object.Instantiate(config.phase1.decoyPrefab, position, Quaternion.identity);
            decoy.name = isReal ? "RealDecoy" : "FakeDecoy";
            
            // TODO: Delete after
            // Ensure decoy has a collider for attacks 
            Collider decoyCollider = decoy.GetComponent<Collider>();
            if (decoyCollider == null)
            {
                // Add a default collider if none exists
                decoyCollider = decoy.AddComponent<CapsuleCollider>();
                if (decoyCollider is CapsuleCollider capsule)
                {
                    capsule.height = 2f;
                    capsule.radius = 0.5f;
                    capsule.center = new Vector3(0, 1, 0);
                }
                Debug.Log($"[DecoyState] Added Collider to {decoy.name}");
            }
            
            // Ensure collider is not a trigger for attack detection
            if (decoyCollider != null) decoyCollider.isTrigger = false;

            // Add decoy behavior
            var decoyBehavior = decoy.GetComponent<DecoyBehavior>();
            if (decoyBehavior == null)
                decoyBehavior = decoy.AddComponent<DecoyBehavior>();
                
            decoyBehavior.Initialize(bossController, isReal, config.phase1.decoyMoveSpeed);
            
            bossController.AddDecoy(decoy);
            return decoy;
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            if (skillTimer >= config.phase1.decoyDuration) EndDecoyState();
        }

        private void EndDecoyState()
        {
            bossController.ClearDecoys();                               // Clean up decoys
            bossController.gameObject.SetActive(true);                  // Show original boss
            bossController.ChangeState(new IdleState());                // Return to idle
        }

        public override void Exit()
        {
            if (!skillActivated) return;
            bossController.ClearDecoys();
            bossController.gameObject.SetActive(true);
        }

        public override void OnTakeDamage() {}
        
        public override bool CanBeInterrupted()
        {
            return isCasting; 
        }
    }
}
