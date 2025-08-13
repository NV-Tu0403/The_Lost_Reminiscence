using Code.Puzzle.LightTree;
using UnityEngine;

namespace _MyGame.Codes.Puzzle.LightTree
{
    public class IdController : MonoBehaviour
    {
        private PlayerController_02 playerController;
        
        [SerializeField] private FaController faController;
        [SerializeField] private PlayerSpirit playerSpirit;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float timeBetweenHits = 1f; // thời gian giữa các lần trừ máu
        
        private float hitCooldown;
        private bool attractedToShield;
        private bool guiding;
        private Vector3 shieldTarget;


        private void Update()
        {
            UpdateShieldState();
            MoveId();
            UpdateHitCooldown();
        }
        
        // Thiết lập đối tượng TestController để Id có thể theo dõi
        public void SetChaseTarget(PlayerController_02 player)
        {
            playerController = player;
        }

        // Reset trạng thái Id, gọi khi Id bị phá hủy hoặc reset
        public void ResetChase()
        {
            playerController = null;
            hitCooldown = 0f;
        }
        

        // Cập nhật trạng thái lá chắn, kiểm tra xem có đang dẫn lối hay không
        private void UpdateShieldState()
        {
            if (faController != null && faController.IsShieldActive())
            {
                attractedToShield = true;
                shieldTarget = faController.GetShieldPosition();
                // Nếu đang dẫn lối thì lao nhanh vào tâm lá chắn
                guiding = faController.IsGuiding();
            }
            else
            {
                attractedToShield = false;
                guiding = false;
            }
        }

        // Di chuyển Id về phía người chơi hoặc tâm lá chắn nếu đang dẫn lối
        private void MoveId()
        {
            var direction = (shieldTarget - transform.position).normalized;
            if (attractedToShield)
            {
                var moveSpeed = guiding ? faController.attractSpeed * 2f : faController.attractSpeed;
                transform.position += direction * (moveSpeed * Time.deltaTime);
            }
            else if (playerController != null)
            {
                direction = (playerController.transform.position - transform.position).normalized;
                transform.position += direction * (speed * Time.deltaTime);
            }
        }

        // Cập nhật cooldown giữa các lần trừ máu
        private void UpdateHitCooldown()
        {
            // Giảm cooldown mỗi frame
            if (hitCooldown > 0f)
                hitCooldown -= Time.deltaTime;
        }

        // Xử lý va chạm với người chơi hoặc lá chắn
        private void OnTriggerStay(Collider other)
        {
            // Phá hủy nếu guiding đang bật và đang ở trong shieldObject
            if (faController != null && faController.IsShieldActive() && faController.IsGuiding() && other.gameObject == faController.shieldObject)
            {
                Debug.Log("Id bị phá hủy bởi shield khi guiding! (OnTriggerStay)");
                Destroy(gameObject);
                return;
            }

            // Kiểm tra nếu va chạm với nguời chơi thi trừ máu
            if (playerController == null || other.gameObject != playerController.gameObject) return;
            if (!(hitCooldown <= 0f)) return;
            Debug.Log("ID triggered by player (OnTriggerStay)");
            if (playerSpirit != null)
                playerSpirit.ReduceSpirit(1);
            hitCooldown = timeBetweenHits;
        }

        // Xử lý va chạm với lá chắn
        private void OnTriggerEnter(Collider other)
        {
            // Chỉ phá hủy khi guiding đang bật và va vào shieldObject
            if (faController != null && faController.IsShieldActive() && faController.IsGuiding() && other.gameObject == faController.shieldObject)
            {
                Destroy(gameObject);
            }
        }
    }
}