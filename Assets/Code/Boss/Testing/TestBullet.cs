using UnityEngine;
using Code.Boss;

namespace Code.Boss.Testing
{
    /// <summary>
    /// Script cho đạn test - di chuyển thẳng và detect collision với boss/decoy
    /// </summary>
    public class TestBullet : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private float timeAlive;
        private Rigidbody rb;

        public void Initialize(Vector3 shootDirection, float bulletSpeed, float bulletLifetime)
        {
            direction = shootDirection.normalized;
            speed = bulletSpeed;
            lifetime = bulletLifetime;
            timeAlive = 0f;
            
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Sử dụng Rigidbody để di chuyển
                rb.linearVelocity = direction * speed;
                
                // Đảm bảo continuous collision detection để không miss
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            
            // Đảm bảo bullet có collider và isTrigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void Update()
        {
            timeAlive += Time.deltaTime;
            
            // Manual collision check as fallback
            CheckManualCollision();
            
            // Tự hủy sau lifetime
            if (timeAlive >= lifetime)
            {
                DestroyBullet();
                return;
            }
            
            // Nếu không có Rigidbody, di chuyển bằng transform
            if (rb == null)
            {
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
            }
        }

        private void CheckManualCollision()
        {
            // Manual collision check as backup when Unity's physics fails
            float checkRadius = 1f; // Radius to check around bullet
            
            // Check for boss
            var boss = FindObjectOfType<BossController>();
            if (boss != null && boss.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, boss.transform.position);
                if (distance <= checkRadius)
                {
                    HandleCollision(boss.GetComponent<Collider>());
                    return;
                }
            }
            
            // Check for decoys
            var decoys = FindObjectsOfType<DecoyBehavior>();
            foreach (var decoy in decoys)
            {
                if (decoy.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, decoy.transform.position);
                    if (distance <= checkRadius)
                    {
                        HandleCollision(decoy.GetComponent<Collider>());
                        return;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.collider);
        }

        private void HandleCollision(Collider hitCollider)
        {
            // Check for boss
            var boss = hitCollider.GetComponent<BossController>();
            if (boss != null)
            {
                var bossManager = BossManager.Instance;
                if (bossManager != null)
                {
                    bossManager.PlayerAttackBoss();
                }
                DestroyBullet();
                return;
            }
            
            // Check for decoy
            var decoy = hitCollider.GetComponent<DecoyBehavior>();
            if (decoy != null)
            {
                decoy.OnAttacked();
                DestroyBullet();
                return;
            }
            
            // Check for soul
            var soul = hitCollider.GetComponent<SoulBehavior>();
            if (soul != null)
            {
                DestroyBullet();
                return;
            }
            
            // Check for ground/obstacles
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DestroyBullet();
            }
        }

        private void DestroyBullet()
        {
            // Có thể thêm hiệu ứng nổ ở đây
            CreateHitEffect();
            Destroy(gameObject);
        }

        private void CreateHitEffect()
        {
            // Tạo hiệu ứng đơn giản khi đạn nổ
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "BulletHitEffect";
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector3.one * 0.5f;
            
            // Màu đỏ cho hiệu ứng
            var renderer = effect.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
            
            // Remove collider để không ảnh hưởng gameplay
            var collider = effect.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            
            // Tự hủy sau 0.2 giây
            Destroy(effect, 0.2f);
        }
    }
}
