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
        
        private void DestroyBullet()
        {
            Destroy(gameObject);
        }
    }
}
