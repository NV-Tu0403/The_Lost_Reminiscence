using UnityEngine;

namespace Events.Puzzle.Test.PuzzleDemo
{
    public class Id : MonoBehaviour
    {
        private TestController _testController;
        private FaController faController;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float timeBetweenHits = 1f; // thời gian giữa các lần trừ máu
        private float hitCooldown = 0f;
        private bool attractedToShield = false;
        private bool guiding = false;
        private Vector3 shieldTarget;

        public void SetChaseTarget(TestController testController)
        {
            _testController = testController;
        }

        public void ResetChase()
        {
            _testController = null;
            hitCooldown = 0f;
        }

        // Di chuyen duoi Player
        private void Start()
        {
            faController = FindObjectOfType<FaController>();
        }

        private void Update()
        {
            if (faController != null && faController.IsShieldActive())
            {
                attractedToShield = true;
                shieldTarget = faController.GetShieldPosition();
                // Nếu đang dẫn lối thì lao nhanh vào tâm lá chắn
                if (faController.IsGuiding())
                {
                    guiding = true;
                }
            }
            else
            {
                attractedToShield = false;
                guiding = false;
            }

            if (attractedToShield)
            {
                float moveSpeed = guiding ? faController.attractSpeed * 2f : faController.attractSpeed;
                Vector3 direction = (shieldTarget - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            else if (_testController != null)
            {
                Vector3 direction = (_testController.transform.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
            // Giảm cooldown mỗi frame
            if (hitCooldown > 0f)
                hitCooldown -= Time.deltaTime;
        }

        private void OnTriggerStay(Collider other)
        {
            // Phá hủy nếu guiding đang bật và đang ở trong shieldObject
            if (faController != null && faController.IsShieldActive() && faController.IsGuiding() && other.gameObject == faController.shieldObject)
            {
                Debug.Log("Id bị phá hủy bởi shield khi guiding! (OnTriggerStay)");
                Destroy(gameObject);
                return;
            }

            if (_testController != null && other.gameObject == _testController.gameObject)
            {
                if (hitCooldown <= 0f)
                {
                    Debug.Log("ID triggered by player (OnTriggerStay)");
                    Puzzle3.Instance.ReduceSpirit(1);
                    hitCooldown = timeBetweenHits;
                }
            }
        }

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