using UnityEngine;
using UnityEngine.Playables;

namespace Code.Timelines
{
    public class DialogueController : MonoBehaviour
    {
        public PlayableDirector timeline;
        public bool isDialogueRunning = false;

        private void Start()
        {
            timeline.Play();
        }

        public void StartDialogue()
        {
            isDialogueRunning = true;
            timeline.Pause();
            Debug.Log("Dialogue started");
            // Gọi sự kiện chạy DialogueManager để bắt đầu hội thoại
        }

        public void StopDialogue()
        {
            isDialogueRunning = false;
            timeline.time = 8f; // Nhảy đến animation LookAround
            timeline.Play();

            Debug.Log("Dialogue stopped");
        }

        private void Update()
        {
            if (isDialogueRunning && Input.GetKeyDown(KeyCode.Space))
            {
                StopDialogue();
            }
        }
    }
}