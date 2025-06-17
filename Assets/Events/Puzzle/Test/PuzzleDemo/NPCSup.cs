using UnityEngine;

namespace Events.Puzzle.Test.PuzzleDemo
{
    public class NPCSup : MonoBehaviour
    {
        [Header("Câu hỏi và đáp án")]
        public string question;
        public string[] answers; 
        public int correctIndex; // chỉ số đáp án đúng

        private bool hasAnswered = false;
        [SerializeField] private DialogueSupUI dialogueUI;
        private FaController faController;
        private bool attractedToShield = false;
        private bool guiding = false;
        private Vector3 shieldTarget;

        private void Awake()
        {
            if (dialogueUI == null)
                dialogueUI = FindObjectOfType<DialogueSupUI>();
        }

        private void Start()
        {
            if (dialogueUI == null)
                dialogueUI = FindObjectOfType<DialogueSupUI>();
            faController = FindObjectOfType<FaController>();
        }

        private void OnTriggerStay(Collider other)
        {
            // Phá hủy nếu guiding đang bật và đang ở trong shieldObject
            if (faController != null && faController.IsShieldActive() && faController.IsGuiding() && other.gameObject == faController.shieldObject)
            {
                Debug.Log("NPCSup bị phá hủy bởi shield khi guiding! (OnTriggerStay)");
                Destroy(gameObject);
                return;
            }

            if (!hasAnswered && other.GetComponent<TestController>() != null && Input.GetKeyDown(KeyCode.E))
            {
                ShowQuestion();
            }
        }

        private void Update()
        {
            if (faController != null && faController.IsShieldActive())
            {
                attractedToShield = true;
                shieldTarget = faController.GetShieldPosition();
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
        }

        public void ShowQuestion()
        {
            if (dialogueUI != null)
                dialogueUI.Show(this);
        }

        public void OnAnswered(bool isCorrect)
        {
            hasAnswered = isCorrect;
        }

        public void ResetState()
        {
            hasAnswered = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"NPCSup OnTriggerEnter: {other.gameObject.name}, ShieldActive={faController?.IsShieldActive()}, Guiding={faController?.IsGuiding()}");
            // Chỉ phá hủy khi guiding đang bật và va vào shieldObject
            if (faController != null && faController.IsShieldActive() && faController.IsGuiding() && other.gameObject == faController.shieldObject)
            {
                Debug.Log("NPCSup bị phá hủy bởi shield khi guiding!");
                Destroy(gameObject);
            }
        }
    }
}