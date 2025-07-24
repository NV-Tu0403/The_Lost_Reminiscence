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
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            
            Debug.Log("[Boss State] Entered DecoyState - Casting Decoy skill");
            BossEventSystem.Trigger(BossEventType.DecoyStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted);
            
            // Play decoy spawn sound
            if (config.audioConfig.decoySpawnSound != null)
            {
                bossController.PlaySound(config.audioConfig.decoySpawnSound, config.audioConfig.sfxVolume);
            }
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
            float progress = castTimer / config.phase1.decoyCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            
            if (castTimer >= config.phase1.decoyCastTime)
            {
                ActivateSkill();
            }
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            
            SpawnDecoys();
            Debug.Log("[Boss State] DecoyState - Skill activated, decoys spawned");
        }

        private void SpawnDecoys()
        {
            Vector3 spawnCenter = bossController.Player.position;
            float spawnRadius = config.phase1.decoySpawnRadius;
            
            // Spawn real decoy (this is actually the boss)
            Vector3 realPos = GetRandomSpawnPosition(spawnCenter, spawnRadius);
            realDecoy = CreateDecoy(realPos, true);
            
            // Spawn fake decoy
            Vector3 fakePos = GetRandomSpawnPosition(spawnCenter, spawnRadius);
            fakeDecoy = CreateDecoy(fakePos, false);
            
            // Hide original boss
            bossController.gameObject.SetActive(false);
        }

        private Vector3 GetRandomSpawnPosition(Vector3 center, float radius)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * radius;
            return center + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        private GameObject CreateDecoy(Vector3 position, bool isReal)
        {
            // Create decoy GameObject (assuming we have a prefab)
            GameObject decoy = new GameObject(isReal ? "RealDecoy" : "FakeDecoy");
            decoy.transform.position = position;
            
            // Add decoy behavior
            var decoyBehavior = decoy.AddComponent<DecoyBehavior>();
            decoyBehavior.Initialize(bossController, isReal, config.phase1.decoyMoveSpeed);
            
            bossController.AddDecoy(decoy);
            return decoy;
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            
            if (skillTimer >= config.phase1.decoyDuration)
            {
                EndDecoyState();
            }
        }

        private void EndDecoyState()
        {
            // Clean up decoys
            bossController.ClearDecoys();
            
            // Show original boss
            bossController.gameObject.SetActive(true);
            
            // Return to idle
            bossController.ChangeState(new IdleState());
        }

        public override void Exit()
        {
            // Ensure cleanup
            if (skillActivated)
            {
                bossController.ClearDecoys();
                bossController.gameObject.SetActive(true);
            }
        }

        public override void OnTakeDamage()
        {
            if (isCasting && CanBeInterrupted())
            {
                // Skill interrupted
                BossEventSystem.Trigger(BossEventType.SkillInterrupted);
                bossController.ChangeState(new IdleState());
            }
        }

        public override bool CanBeInterrupted()
        {
            return isCasting; // Can only be interrupted during casting
        }
    }
}
