using UnityEngine;
using Code.Boss.States.Shared;

namespace Code.Boss
{
    /// <summary>
    /// Hành vi của Decoy (bóng ảo) - di chuyển chậm theo người chơi
    /// </summary>
    public class DecoyBehavior : MonoBehaviour
    {
        private BossController bossController;
        private bool isReal;
        private float moveSpeed;
        private Transform target;
        
        public bool IsReal => isReal;

        public void Initialize(BossController controller, bool real, float speed)
        {
            bossController = controller;
            isReal = real;
            moveSpeed = speed;
            target = controller.Player;
            
            // Visual differences between real and fake decoy could be added here
            if (!isReal)
            {
                // Make fake decoy slightly different (transparency, color, etc.)
                var renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = renderer.material;
                    var color = material.color;
                    color.a = 0.8f; // Slightly transparent
                    material.color = color;
                }
            }
        }

        private void Update()
        {
            if (target != null)
            {
                MoveTowardsTarget();
            }
        }

        private void MoveTowardsTarget()
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Rotate to face target
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                HandlePlayerContact();
            }
        }

        private void HandlePlayerContact()
        {
            if (isReal)
            {
                // Player hit real decoy - boss takes damage
                BossEventSystem.Trigger(BossEventType.RealDecoyHit);
                bossController.TakeDamage(1);
            }
            else
            {
                // Player hit fake decoy - player takes damage
                BossEventSystem.Trigger(BossEventType.FakeDecoyHit);
                BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(1));
                
                // Transition to Soul State
                bossController.ChangeState(new SoulState());
            }
            
            // Remove this decoy
            bossController.RemoveDecoy(gameObject);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Hành vi của Soul (dục hồn) - đuổi nhanh theo người chơi
    /// </summary>
    public class SoulBehavior : MonoBehaviour
    {
        private Transform target;
        private SoulConfig config;
        private float currentSpeed;
        
        public void Initialize(Transform playerTarget, SoulConfig soulConfig)
        {
            target = playerTarget;
            config = soulConfig;
            currentSpeed = config.soulMoveSpeed;
        }

        private void Update()
        {
            if (target != null)
            {
                FollowTarget();
            }
        }

        private void FollowTarget()
        {
            Vector3 direction = (target.position - transform.position).normalized;
            
            // Keep minimum distance from player
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > config.soulFollowDistance)
            {
                transform.position += direction * currentSpeed * Time.deltaTime;
            }
            
            // Rotate to face target
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Add floating/hovering effect
            AddFloatingEffect();
        }

        private void AddFloatingEffect()
        {
            float hover = Mathf.Sin(Time.time * 2f) * 0.5f;
            Vector3 pos = transform.position;
            pos.y += hover * Time.deltaTime;
            transform.position = pos;
        }

        private void OnDestroy()
        {
            // Soul destroyed - could trigger particle effects here
        }
    }

    /// <summary>
    /// Hành vi của Fear Zone - vùng tối gây hiệu ứng tâm lý
    /// </summary>
    public class FearZoneBehavior : MonoBehaviour
    {
        private float radius;
        private float blurIntensity;
        private bool isActive = false;
        
        public void Initialize(float zoneRadius, float visionBlur)
        {
            radius = zoneRadius;
            blurIntensity = visionBlur;
            isActive = true;
            
            CreateVisualEffect();
        }

        private void CreateVisualEffect()
        {
            // Create dark circle on ground
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.parent = transform;
            cylinder.transform.localPosition = Vector3.zero;
            cylinder.transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2);
            
            // Make it dark and transparent
            var renderer = cylinder.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0, 0, 0, 0.7f);
                material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material = material;
            }
            
            // Remove collider
            var collider = cylinder.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && isActive)
            {
                // Player entered fear zone
                ApplyFearEffects(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && isActive)
            {
                // Player left fear zone
                ApplyFearEffects(false);
            }
        }

        private void ApplyFearEffects(bool enable)
        {
            // This would integrate with camera/post-processing effects
            string effectName = enable ? "EnableFearEffect" : "DisableFearEffect";
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = effectName, floatValue = blurIntensity });
        }
    }

    /// <summary>
    /// Hành vi của Memory Fragment - mảnh ghép kí ức
    /// </summary>
    public class MemoryFragmentBehavior : MonoBehaviour
    {
        private bool collected = false;
        
        private void Start()
        {
            // Add visual effects for memory fragment
            AddGlowEffect();
        }

        private void AddGlowEffect()
        {
            // Add rotating and glowing effect
            var renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                // Create a simple cube as memory fragment
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = transform;
                cube.transform.localPosition = Vector3.zero;
                renderer = cube.GetComponent<Renderer>();
            }
            
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = Color.cyan;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.cyan * 0.5f);
                renderer.material = material;
            }
        }

        private void Update()
        {
            if (!collected)
            {
                // Rotate the fragment
                transform.Rotate(0, 90 * Time.deltaTime, 0);
                
                // Add floating effect
                float hover = Mathf.Sin(Time.time * 2f) * 0.2f;
                Vector3 pos = transform.position;
                pos.y += hover * Time.deltaTime;
                transform.position = pos;
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
            
            // Play collection effect and sound
            // Add particle effects here
            
            // Destroy after collection
            Destroy(gameObject, 0.5f);
        }
    }
}
