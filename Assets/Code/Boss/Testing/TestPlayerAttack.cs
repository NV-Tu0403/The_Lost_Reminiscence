using UnityEngine;
using Code.Boss;

namespace Code.Boss.Testing
{
    /// <summary>
    /// Test controller để giả lập player movement và tương tác với Boss
    /// </summary>
    public class TestPlayerAttack : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private float bulletLifetime = 5f;
        [SerializeField] private float attackCooldown = 0.5f;
        
        [Header("UI Testing")]
        [SerializeField] private bool showDebugUI = true;
        
        private float lastAttackTime;
        private BossManager bossManager;
        private Camera playerCamera;

        private void Start()
        {
            bossManager = BossManager.Instance;
            playerCamera = Camera.main;
            
            // Nếu không có firePoint được gán, tạo một cái tự động
            if (firePoint == null)
            {
                GameObject firePointGO = new GameObject("FirePoint");
                firePointGO.transform.SetParent(transform);
                firePointGO.transform.localPosition = Vector3.forward * 1f;
                firePoint = firePointGO.transform;
            }
        }

        private void Update()
        {
            HandleAttackInput();
        }

        private void HandleAttackInput()
        {
            // Space or Left Mouse Button to attack
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && CanAttack())
            {
                PerformAttack();
            }
        }
        
        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            
            // Bắn đạn theo hướng camera nhìn (người chơi tự aim)
            Vector3 shootDirection = GetShootDirection();
            FireBullet(shootDirection);
        }

        private Vector3 GetShootDirection()
        {
            // Bắn theo hướng camera - người chơi tự aim
            if (playerCamera != null)
            {
                return playerCamera.transform.forward;
            }
            else
            {
                // Fallback: bắn theo hướng transform forward
                return transform.forward;
            }
        }

        private void FireBullet(Vector3 direction)
        {
            GameObject bullet;
            
            if (bulletPrefab != null)
            {
                // Sử dụng prefab nếu có
                bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            }
            else
            {
                // Tạo bullet đơn giản nếu không có prefab
                bullet = CreateSimpleBullet();
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Add bullet behavior
            var bulletScript = bullet.GetComponent<TestBullet>();
            if (bulletScript == null)
            {
                bulletScript = bullet.AddComponent<TestBullet>();
            }
            
            bulletScript.Initialize(direction, bulletSpeed, bulletLifetime);
        }

        private GameObject CreateSimpleBullet()
        {
            // Tạo bullet đơn giản bằng primitive
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "TestBullet";
            bullet.transform.localScale = Vector3.one * 0.2f;
            
            // Thêm Rigidbody cho physics
            var rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            
            // Màu vàng để dễ thấy
            var renderer = bullet.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
            
            return bullet;
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== PLAYER TEST CONTROLS ===");
            GUILayout.Label("Space/LMB: Shoot Bullet");
            GUILayout.Label("Q: Fa Radar Skill");
            GUILayout.Label("E: Fa Second Skill");
            GUILayout.Label("R: Fa Reveal Skill");
            GUILayout.Space(10);
            GUILayout.Label($"Position: {transform.position}");
            GUILayout.Label($"Can Attack: {CanAttack()}");
            GUILayout.Label($"Fire Direction: {GetShootDirection()}");
            GUILayout.EndArea();
        }

        private void OnDrawGizmosSelected()
        {
            if (firePoint != null)
            {
                // Vẽ fire point và hướng bắn
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(firePoint.position, 0.2f);
                
                Gizmos.color = Color.yellow;
                Vector3 shootDir = GetShootDirection();
                Gizmos.DrawRay(firePoint.position, shootDir * 5f);
            }
        }
    }
}
