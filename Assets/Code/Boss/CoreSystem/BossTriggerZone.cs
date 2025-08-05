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
        
        private void Start()
        {
            // Đảm bảo collider là trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            else
            {
                Debug.LogError("[BossTriggerZone] No Collider component found! Please add a Collider and set it as Trigger.");
            }
            
            // Tạo BossGameManager nếu chưa có
            if (BossGameManager.Instance == null)
            {
                var gameManagerGO = new GameObject("BossGameManager");
                bossGameManager = gameManagerGO.AddComponent<BossGameManager>();
            }
            else
            {
                bossGameManager = BossGameManager.Instance;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            
            if (other.CompareTag(playerTag))
            {
                Debug.Log("[BossTriggerZone] Player entered boss area - Starting boss fight!");
                StartBossFight();
            }
        }
        
        private void StartBossFight()
        {
            hasTriggered = true;
            
            // Play trigger effect
            if (triggerEffectPrefab != null)
            {
                Instantiate(triggerEffectPrefab, transform.position, Quaternion.identity);
            }
            
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
            
            Vector3 spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
            Quaternion spawnRotation = bossSpawnPoint != null ? bossSpawnPoint.rotation : Quaternion.identity;
            
            // Play spawn effect
            if (bossSpawnEffectPrefab != null)
            {
                Instantiate(bossSpawnEffectPrefab, spawnPosition, spawnRotation);
            }
            
            // Spawn boss
            GameObject bossGO = Instantiate(bossPrefab, spawnPosition, spawnRotation);
            spawnedBoss = bossGO.GetComponent<BossController>();
            
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
            if (bossGameManager != null && spawnedBoss != null)
            {
                // Update BossGameManager references
                var bossControllerField = bossGameManager.GetType().GetField("bossController", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bossControllerField?.SetValue(bossGameManager, spawnedBoss);
                
                Debug.Log("[BossTriggerZone] Boss UI setup completed!");
            }
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
            
            Debug.Log("[BossTriggerZone] Trigger zone reset");
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
            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
                Gizmos.DrawLine(transform.position, bossSpawnPoint.position);
            }
        }
    }
}
