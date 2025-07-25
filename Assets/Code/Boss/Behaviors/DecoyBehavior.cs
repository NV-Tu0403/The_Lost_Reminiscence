using Code.Boss.States.Shared;
using UnityEngine;

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
        

        // Method này sẽ được gọi từ PlayerTestController thông qua BossManager
        public void OnAttacked()
        {
            HandlePlayerContact();
        }

        private void HandlePlayerContact()
        {
            if (isReal)
            {
                // Player hit real decoy - boss takes damage and end decoy state
                BossEventSystem.Trigger(BossEventType.RealDecoyHit);
                
                // End DecoyState immediately when real decoy is hit
                Debug.Log("[DecoyBehavior] Real decoy hit! Ending DecoyState...");
                
                // Clean up all decoys and return boss to normal
                bossController.ClearDecoys();
                bossController.gameObject.SetActive(true);
                
                // Damage boss AFTER cleanup - use special method to bypass CanTakeDamage() check
                Debug.Log("[DecoyBehavior] Calling TakeDamageFromDecoy to bypass invulnerability");
                bossController.TakeDamageFromDecoy(1);
                
                // Don't force state change - let BossController handle phase transition naturally
            }
            else
            {
                // Player hit fake decoy - player takes damage
                BossEventSystem.Trigger(BossEventType.FakeDecoyHit);
                BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(1));
                
                // Transition to Soul State
                bossController.ChangeState(new Code.Boss.States.Shared.SoulState());
            }
            
            // Remove this decoy
            bossController.RemoveDecoy(gameObject);
            Destroy(gameObject);
        }
    }
}