using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Loc_Backend.Dialogue.Scripts
{
    public class DialoguePanel : MonoBehaviour
    {
        public Image leftAvatar;
        public Image rightAvatar;
        public TextMeshProUGUI leftName;
        public TextMeshProUGUI rightName;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI continueHint;
        public TypewriterEffect typewriterEffect;

        private DialogueSO _currentDialogue;
        private int _currentIndex = 0;
        private Coroutine _blinkCoroutine;

        public void StartDialogue(DialogueSO dialogueSo)
        {
            _currentDialogue = dialogueSo;
            _currentIndex = 0;
            ShowCurrentLine();
        }

        public void ShowCurrentLine()
        {
            if (_currentDialogue == null || _currentIndex >= _currentDialogue.lines.Length)
            {
                gameObject.SetActive(false);
                return;
            }

            var line = _currentDialogue.lines[_currentIndex];

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

            continueHint.gameObject.SetActive(false); // Ẩn hint trước khi chạy typewriter

            string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("Dialogue", line.localizationKey);

            // Chạy typewriter, xong thì hiện hint và bắt đầu hiệu ứng nhấp nháy
            typewriterEffect.StartTypewriter(localizedText, OnTypewriterComplete);
        }

        private void OnTypewriterComplete()
        {
            continueHint.gameObject.SetActive(true);
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = StartCoroutine(BlinkContinueHint());
        }

        IEnumerator BlinkContinueHint()
        {
            while (true)
            {
                continueHint.alpha = 1f;
                yield return new WaitForSeconds(0.5f);
                continueHint.alpha = 0.2f;
                yield return new WaitForSeconds(0.5f);
            }
        }

        void Update()
        {
            if (_currentDialogue == null) return;
            // Chỉ cho phép tiếp tục khi hint đang hiện (tức là typewriter đã xong)
            if (continueHint.gameObject.activeSelf && Input.GetMouseButtonDown(0))
            {
                if (_blinkCoroutine != null)
                    StopCoroutine(_blinkCoroutine);
                continueHint.gameObject.SetActive(false);
                _currentIndex++;
                ShowCurrentLine();
            }
        }
    }
}