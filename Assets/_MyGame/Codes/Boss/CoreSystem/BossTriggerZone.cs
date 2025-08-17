using System.Linq;
using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Trigger zone để bắt đầu boss fight khi player đi vào
    /// </summary>
    public class BossTriggerZone : MonoBehaviour
    {
        [Header("Boss Fight Setup")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private BossConfig bossConfig;
        
        [Header("Trigger Settings")]
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private string playerTag = "Player";
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject triggerEffectPrefab;
        [SerializeField] private GameObject bossSpawnEffectPrefab;
        
        private bool hasTriggered = false;
        private BossController spawnedBoss;
        private BossGameManager bossGameManager;
        private Rift_Controller riftController;
        
        private void Start()
        {
            // Đảm bảo collider là trigger
            var component = GetComponent<Collider>();
            if (component != null)
            {
                component.isTrigger = true;
            }
            else
            {
                Debug.LogError("[BossTriggerZone] No Collider component found! Please add a Collider and set it as Trigger.");
            }
            
            // Tạo BossGameManager nếu chưa có
            if (BossGameManager.Instance == null)
            {
                var gameManagerGo = new GameObject("BossGameManager");
                bossGameManager = gameManagerGo.AddComponent<BossGameManager>();
            }
            else
            {
                bossGameManager = BossGameManager.Instance;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;

            if (!other.CompareTag(playerTag)) return;
            Debug.Log("[BossTriggerZone] Player entered boss area - Starting boss fight!");
            StartBossFight();
        }
        
        private void StartBossFight()
        {
            hasTriggered = true;
            
            // Play trigger effect
            if (triggerEffectPrefab != null)
            {
                Instantiate(triggerEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Spawn effects
            if (riftController == null) riftController = FindFirstObjectByType<Rift_Controller>();
            riftController.F_ToggleRift(true);
                
            // Spawn boss
            SpawnBoss();
            
            // Setup UI
            SetupBossUI();
            
            // Trigger boss fight started event
            BossEventSystem.Trigger(BossEventType.BossFightStarted);
        }
        
        private void SpawnBoss()
        {
            if (bossPrefab == null)
            {
                Debug.LogError("[BossTriggerZone] Boss prefab not assigned!");
                return;
            }
            
            var spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
            var spawnRotation = bossSpawnPoint != null ? bossSpawnPoint.rotation : Quaternion.identity;
            
            // Play spawn effect và destroy sau 3 giây
            if (bossSpawnEffectPrefab != null)
            {
                var spawnEffect = Instantiate(bossSpawnEffectPrefab, spawnPosition, spawnRotation);
                Destroy(spawnEffect, 3f); // Tự động destroy effect sau 3 giây
            }
            
            // Spawn boss
            var bossGo = Instantiate(bossPrefab, spawnPosition, spawnRotation);
            spawnedBoss = bossGo.GetComponent<BossController>();
            
            if (spawnedBoss == null)
            {
                Debug.LogError("[BossTriggerZone] Spawned boss doesn't have BossController component!");
                return;
            }
            
            // Assign config if available
            if (bossConfig != null)
            {
                // Gán config thông qua reflection hoặc public method
                var configField = spawnedBoss.GetType().GetField("bossConfig", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                configField?.SetValue(spawnedBoss, bossConfig);
            }
            
            Debug.Log("[BossTriggerZone] Boss spawned successfully!");
        }
        
        private void SetupBossUI()
        {
            if (bossGameManager == null || spawnedBoss == null) return;
            // Update BossGameManager references
            var bossControllerField = bossGameManager.GetType().GetField("bossController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bossControllerField?.SetValue(bossGameManager, spawnedBoss);
                
            Debug.Log("[BossTriggerZone] Boss UI setup completed!");
        }
        
        /// <summary>
        /// Reset trigger zone (dùng khi restart game)
        /// </summary>
        public void ResetTrigger()
        {
            hasTriggered = false;
            
            // Destroy spawned boss if exists
            if (spawnedBoss != null)
            {
                Destroy(spawnedBoss.gameObject);
                spawnedBoss = null;
            }
            
            // Cleanup all boss-related objects in scene
            CleanupBossEffects();
            
            Debug.Log("[BossTriggerZone] Trigger zone reset");
        }
        
        /// <summary>
        /// Cleanup tất cả effects, souls, decoys còn sót lại từ boss fight trước
        /// </summary>
        private static void CleanupBossEffects()
        {
            // Cleanup souls bằng cách tìm SoulBehavior component
            var soulBehaviors = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<object>()
                .Where(mb => mb.GetType().Name == "SoulBehavior");
            foreach (var soul in soulBehaviors)
            {
                if (soul is MonoBehaviour mb)
                    Destroy(mb.gameObject);
            }
            
            // Cleanup decoys bằng cách tìm DecoyBehavior component
            var decoyBehaviors = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<object>()
                .Where(mb => mb.GetType().Name == "DecoyBehavior");
            foreach (var decoy in decoyBehaviors)
            {
                if (decoy is MonoBehaviour mb)
                    Destroy(mb.gameObject);
            }
            
            // Cleanup effects (tìm theo tên chứa "Effect" hoặc "VFX")
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Effect") || obj.name.Contains("VFX") || 
                    obj.name.Contains("FearZone") || obj.name.Contains("Shake"))
                {
                    // Kiểm tra không phải UI hoặc player
                    if (!obj.GetComponent<Canvas>() && !obj.CompareTag("Player"))
                    {
                        Destroy(obj);
                    }
                }
            }
            
            // Stop all coroutines trong scene
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in allMonoBehaviours)
            {
                if (mb != null && mb.gameObject != null)
                {
                    mb.StopAllCoroutines();
                }
            }
            
            Debug.Log("[BossTriggerZone] Boss effects cleanup completed");
        }
        
        /// <summary>
        /// Lấy reference tới boss đã spawn
        /// </summary>
        public BossController GetSpawnedBoss() => spawnedBoss;
        
        /// <summary>
        /// Kiểm tra trigger zone đã được kích hoạt chưa
        /// </summary>
        public bool HasTriggered() => hasTriggered;
        
        private void OnDrawGizmosSelected()
        {
            // Vẽ trigger zone trong Scene view
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
            
            // Vẽ boss spawn point
            if (bossSpawnPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
            Gizmos.DrawLine(transform.position, bossSpawnPoint.position);
        }
    }
}
