using Code.Puzzle.LightTree;
using DuckLe;
using UnityEngine;

namespace Script.Puzzle.LightTree
{
    public class IdController : MonoBehaviour
    {
        private PlayerController _playerController;
        
        [SerializeField] private Code.Puzzle.LightTree.FaController faController;
        [SerializeField] private PlayerSpirit playerSpirit;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float timeBetweenHits = 1f; // thời gian giữa các lần trừ máu
        
        private float _hitCooldown = 0f;
        private bool _attractedToShield = false;
        private bool _guiding = false;
        private Vector3 _shieldTarget;


        private void Update()
        {
            UpdateShieldState();
            MoveId();
            UpdateHitCooldown();
        }
        
        // Thiết lập đối tượng TestController để Id có thể theo dõi
        public void SetChaseTarget(PlayerController player)
        {
            _playerController = player;
        }

        // Reset trạng thái Id, gọi khi Id bị phá hủy hoặc reset
        public void ResetChase()
        {
            _playerController = null;
            _hitCooldown = 0f;
        }
        

        // Cập nhật trạng thái lá chắn, kiểm tra xem có đang dẫn lối hay không
        private void UpdateShieldState()
        {
            if (faController != null && faController.IsShieldActive())
            {
                _attractedToShield = true;
                _shieldTarget = faController.GetShieldPosition();
                // Nếu đang dẫn lối thì lao nhanh vào tâm lá chắn
                _guiding = faController.IsGuiding();
            }
            else
            {
                _attractedToShield = false;
                _guiding = false;
            }
        }

        // Di chuyển Id về phía người chơi hoặc tâm lá chắn nếu đang dẫn lối
        private void MoveId()
        {
            if (_attractedToShield)
            {
                float moveSpeed = _guiding ? faController.attractSpeed * 2f : faController.attractSpeed;
                Vector3 direction = (_shieldTarget - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            else if (_playerController != null)
            {
                Vector3 direction = (_playerController.transform.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
        }

        // Cập nhật cooldown giữa các lần trừ máu
        private void UpdateHitCooldown()
        {
            // Giảm cooldown mỗi frame
            if (_hitCooldown > 0f)
                _hitCooldown -= Time.deltaTime;
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
            if (_playerController != null && other.gameObject == _playerController.gameObject)
            {
                if (_hitCooldown <= 0f)
                {
                    Debug.Log("ID triggered by player (OnTriggerStay)");
                    if (playerSpirit != null)
                        playerSpirit.ReduceSpirit(1);
                    _hitCooldown = timeBetweenHits;
                }
            }
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

        private void OnDestroy()
        {
            Debug.Log($"[IdController] {gameObject.name} OnDestroy called");
        }
    }
}