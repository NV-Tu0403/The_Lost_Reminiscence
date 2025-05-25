using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Loc_Backend.Dialogue.Scripts
{
    public class DialoguePanel : MonoBehaviour
    {
        [Header("Typewriter Effect")]
        public TypewriterEffect typewriterEffect;
        
        [Header("UI Elements")]
        public Image leftAvatar;
        public Image rightAvatar;
        public TextMeshProUGUI leftName;
        public TextMeshProUGUI rightName;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI continueHint;

        private DialogueSO currentDialogue;
        private int currentIndex = 0;

        public void StartDialogue(DialogueSO dialogueSo)
        {
            currentDialogue = dialogueSo;
            currentIndex = 0;
            ShowCurrentLine();
        }

        public void ShowCurrentLine()
        {
            // Nếu không có đoạn hội thoại hoặc đã hết các dòng, ẩn panel
            if (currentDialogue == null || currentIndex >= currentDialogue.lines.Length)
            {
                gameObject.SetActive(false);
                return;
            }

            // Hiển thị dòng hiện tại
            var line = currentDialogue.lines[currentIndex];
            
            if (line.isLeftSpeaker)
            {
                leftAvatar.gameObject.SetActive(true);
                leftAvatar.sprite = line.speakerAvatar;
                leftName.text = line.speakerName;

                rightAvatar.gameObject.SetActive(false);
                rightName.text = "";
            }
            else
            {
                rightAvatar.gameObject.SetActive(true);
                rightAvatar.sprite = line.speakerAvatar;
                rightName.text = line.speakerName;

                leftAvatar.gameObject.SetActive(false);
                leftName.text = "";
            }

            // Lấy text từ Localization theo key
            string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("Dialogue", line.localizationKey);
            typewriterEffect.StartTypewriter(localizedText);
            
            continueHint.gameObject.SetActive(true);
        }

        // Cập nhật mỗi frame để kiểm tra sự kiện nhấn chuột ( Cái này là Lộc đang set khi người chơi nhấn chuột thì sẽ chuyển sang dòng tiếp theo )
        void Update()
        {
            if (currentDialogue == null) return;
            if (Input.GetMouseButtonDown(0))
            {
                currentIndex++;
                ShowCurrentLine();
            }
        }
    }
}