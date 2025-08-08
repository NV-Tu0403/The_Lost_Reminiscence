using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Hành vi của Memory Fragment - mảnh ghép kí ức
    /// </summary>
    public class MemoryFragmentBehavior : MonoBehaviour
    {
        [Header("Float Animation")]
        [SerializeField] private float floatHeight = 1.5f;
        [SerializeField] private float floatSpeed = 2f;
        
        [Header("Rotation Animation")]
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        
        private bool collected = false;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool hasReachedTarget = false;
        private float floatProgress = 0f;
        private GameObject spawnedEffect;

        private void Start()
        {
            startPosition = transform.position;
            targetPosition = startPosition + Vector3.up * floatHeight;
            
            // Spawn effect nếu có trong BossConfig
            SpawnEffect();
        }

        private void Update()
        {
            if (!collected)
            {
                if (!hasReachedTarget)
                {
                    // Bay lên đến vị trí target
                    floatProgress += Time.deltaTime * floatSpeed;
                    transform.position = Vector3.Lerp(startPosition, targetPosition, floatProgress);
                    
                    if (floatProgress >= 1f)
                    {
                        hasReachedTarget = true;
                        //transform.position = targetPosition;
                    }
                }
                
                // Xoay vòng liên tục
                transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            }
        }

        private void SpawnEffect()
        {
            // Tìm BossConfig để lấy effect prefab
            var bossController = FindFirstObjectByType<BossController>();
            if (bossController != null && bossController.Config.memoryFragmentEffectPrefab != null)
            {
                spawnedEffect = Instantiate(bossController.Config.memoryFragmentEffectPrefab, 
                    transform.position, Quaternion.identity, transform);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !collected)
            {
                CollectFragment();
            }
        }

        private void CollectFragment()
        {
            collected = true;
            
            // Trigger collection event
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "MemoryFragmentCollected", gameObject = gameObject });
            
            // TODO:
            // Play collection effect and sound
            // Add particle effects here
            // Run Timeline here
            
            // Destroy after collection
            Destroy(gameObject, 0.5f);
        }
    }
}
