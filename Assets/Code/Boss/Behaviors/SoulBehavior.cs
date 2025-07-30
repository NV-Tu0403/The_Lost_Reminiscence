using UnityEngine;

namespace Code.Boss
{
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
}