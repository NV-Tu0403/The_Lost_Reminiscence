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
            if (Config.audioConfig.decoySpawnSound != null)
            {
                BossController.PlaySound(Config.audioConfig.decoySpawnSound, Config.audioConfig.sfxVolume);
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
            var progress = castTimer / Config.phase1.decoyCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= Config.phase1.decoyCastTime) ActivateSkill();
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
            var spawnCenter = BossController.Player.position;
            var spawnRadius = Config.phase1.decoySpawnRadius;
            
            // Spawn real decoy (this is actually the boss)
            var realPos = GetRandomSpawnPosition(spawnCenter, spawnRadius);
            realDecoy = CreateDecoy(realPos, true);
            
            // Spawn fake decoy
            var fakePos = GetRandomSpawnPosition(spawnCenter, spawnRadius);
            fakeDecoy = CreateDecoy(fakePos, false);
            
            // Hide original boss
            BossController.gameObject.SetActive(false);
        }

        private static Vector3 GetRandomSpawnPosition(Vector3 center, float radius)
        {
            var randomCircle = Random.insideUnitCircle.normalized * radius;
            return center + new Vector3(randomCircle.x, 0, randomCircle.y);
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
            
            // TODO: Delete after
            // Ensure decoy has a collider for attacks 
            var decoyCollider = decoy.GetComponent<Collider>();
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
            
            // Ensure collider is a trigger for bullet detection
            if (decoyCollider != null) decoyCollider.isTrigger = true;

            // Add decoy behavior
            var decoyBehavior = decoy.GetComponent<DecoyBehavior>();
            if (decoyBehavior == null)
                decoyBehavior = decoy.AddComponent<DecoyBehavior>();
                
            decoyBehavior.Initialize(BossController, isReal, Config.phase1.decoyMoveSpeed);
            
            BossController.AddDecoy(decoy);
            return decoy;
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            if (skillTimer >= Config.phase1.decoyDuration) EndDecoyState();
        }

        private void EndDecoyState()
        {
            BossController.ClearDecoys();                               // Clean up decoys
            BossController.gameObject.SetActive(true);                  // Show original boss
            BossController.ChangeState(new IdleState());                // Return to idle
        }

        public override void Exit()
        {
            if (!skillActivated) return;
            BossController.ClearDecoys();
            BossController.gameObject.SetActive(true);
        }

        public override void OnTakeDamage() {}
        public override bool CanTakeDamage() => false;
        public override bool CanBeInterrupted() => isCasting;
    }
}
