using Code.Puzzle.LightTree;
using DuckLe;
using UnityEngine;

namespace Script.Puzzle.LightTree
{
    public class SupController : MonoBehaviour
    {
        [Header("Câu hỏi và đáp án")]
        public string question;
        public string[] answers; 
        public int correctIndex; // chỉ số đáp án đúng
        [SerializeField] private UISupDialogue uiSupDialogue;
        [SerializeField] private Code.Puzzle.LightTree.FaController faController;
        [SerializeField] private PlayerSpirit playerSpirit;
       
        
        private bool attractedToShield = false;
        private bool guiding = false;
        private bool hasAnswered = false; 
        private Vector3 shieldTarget;
        

        private void Update()
        {
            UpdateShieldState();
            MoveSup();
        }

  
        // Cập nhật trạng thái lá chắn, kiểm tra xem có đang dẫn lối hay không
        private void UpdateShieldState()
        {
            if (faController != null && faController.IsShieldActive())
            {
                attractedToShield = true;
                shieldTarget = faController.GetShieldPosition();
                guiding = faController.IsGuiding();
            }
            else
            {
                attractedToShield = false;
                guiding = false;
            }
        }

        // Di chuyển NPC về phía tâm lá chắn nếu đang dẫn lối
        private void MoveSup()
        {
            var direction = (shieldTarget - transform.position).normalized;
            var moveSpeed = guiding ? faController.attractSpeed * 2f : faController.attractSpeed;
            if (attractedToShield)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }

        // Hàm này sẽ được gọi khi người chơi muốn tương tác với NPC
        private void ShowQuestion()
        {
            if (uiSupDialogue != null)
                uiSupDialogue.Show(this);
        }

        // Hàm này sẽ được gọi khi người chơi trả lời câu hỏi
        public void OnAnswered(bool isCorrect)
        {
            hasAnswered = isCorrect;
            if (!isCorrect && playerSpirit != null)
            {
                playerSpirit.ReduceSpirit(5);
            }
        }

        // Hàm này sẽ được gọi khi NPC được reset, ví dụ khi người chơi chết hoặc bước này được reset
        public void ResetState()
        {
            hasAnswered = false;
        }
        
        // Hàm này sẽ được gọi khi NPC bị phá hủy
        private void OnTriggerStay(Collider other)
        {
            // Phá hủy nếu guiding đang bật và đang ở trong shieldObject
            if (faController != null && faController.IsShieldActive() && faController.IsGuiding() && other.gameObject == faController.shieldObject)
            {
                Debug.Log("NPCSup bị phá hủy bởi shield khi guiding! (OnTriggerStay)");
                Destroy(gameObject);
                return;
            }

            // Đổi từ TestController sang PlayerController
            if (!hasAnswered && other.GetComponent<PlayerController>() != null)
            {
                ShowQuestion();
            }
        }

        
        // Hàm này sẽ được gọi khi NPC bị phá hủy
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"NPCSup OnTriggerEnter: {other.gameObject.name}, ShieldActive={faController?.IsShieldActive()}, Guiding={faController?.IsGuiding()}");
            // Chỉ phá hủy khi guiding đang bật và va vào shieldObject
            if (faController == null || !faController.IsShieldActive() || !faController.IsGuiding() ||
                other.gameObject != faController.shieldObject) return;
            Debug.Log("NPCSup bị phá hủy bởi shield khi guiding!");
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Debug.Log($"[SupController] {gameObject.name} OnDestroy called");
        }
    }
}