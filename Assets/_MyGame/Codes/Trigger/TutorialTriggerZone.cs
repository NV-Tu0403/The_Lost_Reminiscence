using _MyGame.Codes.Dialogue;
using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    // Hiển thị / ẩn panel tutorial khi player vào / ra vùng. Không disable zone.
    public class TutorialTriggerZone : TriggerZone
    {
        [Header("UI (Static Panel Mode)")] [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private bool showOnEnter = true;
        [SerializeField] private bool hideOnExit = true;
        [SerializeField] private bool showOnlyIfInactive = true; // tránh gọi SetActive(true) lặp lại
        
        [Header("Bubble Dialogue (Optional)")]
        [SerializeField] private bool useBubbleDialogue = false; // nếu true bỏ qua tutorialPanel và dùng dialogueId
        [SerializeField] private string dialogueId;              // Addressables key của DialogueNodeSo
        [SerializeField] private bool reShowOnReEnter = false;    // nếu player ra rồi vào lại có load lại bubble không

        [Header("Progression (optional)")]
        [SerializeField] private bool triggerProgressionOnFirstEnter = false; // chỉ trigger progression 1 lần
        [SerializeField] private bool unlockProcess = false; // có unlock trước khi trigger event không
        
        private bool firstEnterTriggered;
        private bool bubbleShownOnce;

        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            // Bubble dialogue mode
            if (useBubbleDialogue)
            {
                if (!bubbleShownOnce || reShowOnReEnter)
                {
                    if (!string.IsNullOrEmpty(dialogueId) && DialogueManager.Instance != null)
                    {
                        if (reShowOnReEnter && bubbleShownOnce)
                        {
                            // đảm bảo clear cũ trước khi show lại (tránh overlap tween)
                            DialogueManager.Instance.HideBubbleTutorial();
                        }
                        DialogueManager.Instance.ShowBubbleTutorial(dialogueId);
                        bubbleShownOnce = true;
                    }
                    else if (string.IsNullOrEmpty(dialogueId))
                    {
                        Debug.LogWarning($"[TutorialTriggerZone] dialogueId trống trong chế độ BubbleDialogue tại {name}");
                    }
                }
            }
            else // Static panel mode
            {
                if (showOnEnter && tutorialPanel != null)
                {
                    if (!showOnlyIfInactive || !tutorialPanel.activeSelf)
                        tutorialPanel.SetActive(true);
                }
            }

            // Progression tùy chọn (không disable zone)
            if (!triggerProgressionOnFirstEnter || firstEnterTriggered || string.IsNullOrEmpty(eventId)) return;
            if (TryExecuteProgression(unlockProcess, false))
            {
                firstEnterTriggered = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (useBubbleDialogue)
            {
                if (hideOnExit && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.HideBubbleTutorial();
                }
            }
            else if (hideOnExit && tutorialPanel != null)
            {
                if (tutorialPanel.activeSelf)
                    tutorialPanel.SetActive(false);
            }
        }
        
    }
}