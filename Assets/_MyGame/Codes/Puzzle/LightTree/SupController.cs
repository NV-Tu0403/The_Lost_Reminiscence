using Code.Puzzle.LightTree;
using DuckLe;
using UnityEngine;
using UnityEngine.Localization;

namespace Script.Puzzle.LightTree
{
    public class SupController : MonoBehaviour
    {
        //[Header("Dialogue")]
        //public string questionDialogueId; // ID của DialogueNodeSO cho câu hỏi
        
        [Header("Localization")]
        public LocalizedString questionLocalized; // Key localization cho câu hỏi
        public LocalizedString[] answersLocalized; // Keys localization cho các đáp án
        
        [Header("Legacy - Sẽ xóa sau")]
        [SerializeField] public string question;
        [SerializeField] public string[] answers; 
        [SerializeField] public int correctIndex; // chỉ số đáp án đúng
        [SerializeField] private UISupDialogue uiSupDialogue;
        [SerializeField] private FaController faController;
        [SerializeField] private PlayerSpirit playerSpirit;
       
        
        private bool attractedToShield;
        private bool guiding;
        private bool hasAnswered; 
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
                transform.position += direction * (moveSpeed * Time.deltaTime);
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
            if (!hasAnswered && other.GetComponent<PlayerController_02>() != null)
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