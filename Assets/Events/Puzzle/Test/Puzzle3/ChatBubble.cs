using TMPro;
using UnityEngine;

namespace Events.Puzzle.Test.Puzzle3
{
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float duration = 3f;

        private Transform cameraTransform;
        private float timer;

        void Start()
        {
            cameraTransform = Camera.main.transform;
            timer = duration;
        }

        void Update()
        {
            // Xoay về camera
            transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

            // Tự hủy sau thời gian
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void SetMessage(string message, float showDuration = 3f)
        {
            messageText.text = message;
            duration = showDuration;
            timer = showDuration;
        }
    }
}