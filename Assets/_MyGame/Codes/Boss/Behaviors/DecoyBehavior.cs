using _MyGame.Codes.Boss.CoreSystem;
using _MyGame.Codes.Boss.States.Shared;
using UnityEngine;

namespace Code.Boss
{
   /// <summary>
    /// Hành vi của Decoy (bóng ảo) - di chuyển chậm theo người chơi
    /// </summary>
    public class DecoyBehavior : MonoBehaviour
    {
        private BossController bossController;
        private float moveSpeed;
        private Transform target;

        private bool IsReal { get; set; }

        public void Initialize(BossController controller, bool real, float speed)
        {
            bossController = controller;
            IsReal = real;
            moveSpeed = speed;
            target = controller.Player;
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
            var direction = (target.position - transform.position).normalized;
            transform.position += direction * (moveSpeed * Time.deltaTime);
            
            // Rotate to face target 
            if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);
        }

        private void HandlePlayerContact()
        {
            if (IsReal)
            {
                BossEventSystem.Trigger(BossEventType.RealDecoyHit);
                bossController.ClearDecoys();
                bossController.gameObject.SetActive(true);
                bossController.TakeDamageFromDecoy(1);
            }
            else
            {
                BossEventSystem.Trigger(BossEventType.FakeDecoyHit);
                BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(1));
                bossController.ChangeState(new SoulState());
            }
            
            // Remove this decoy
            bossController.RemoveDecoy(gameObject);
            Destroy(gameObject);
        }

        // Xử lý va chạm với Bullet
        private void OnTriggerEnter(Collider other)
        {
            if (!enabled) return;
            if (other.CompareTag("Bullet"))
            {
                HandlePlayerContact();
            }
        }
    }
}