using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Loc_Backend.Dialogue.Scripts
{
    public class DialoguePanel : MonoBehaviour
    {
        [Header("Dialogue Panel")] public Image leftAvatar;
        public Image rightAvatar;
        public TextMeshProUGUI leftName;
        public TextMeshProUGUI rightName;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI continueHint;
        public TypewriterEffect typewriterEffect;

        [Header("Branching UI")] public Transform choicesPanel;
        public GameObject buttonChoicePrefab;

        private readonly List<GameObject> spawnedChoiceButtons = new();
        private DialogueSo _currentDialogue;
        private int _currentIndex;
        private bool _waitingForChoice;

        private bool IsEndOfDialogue(DialogueLineData line)
        {
            return (line.nextLineIndex == -1 && !line.hasChoices && line.nextDialogueSo == null) ||
                   _currentIndex >= _currentDialogue.lines.Length - 1;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) HandleContinueInput();
        }

        public void HandleContinueInput()
        {
            if (_currentDialogue == null || _waitingForChoice) return;
            if (!continueHint.gameObject.activeSelf) return;

            typewriterEffect.StopBlink();
            continueHint.gameObject.SetActive(false);

            var line = _currentDialogue.lines[_currentIndex];

            // Nếu chuyển sang DialogueSO khác
            if (line.nextDialogueSo != null)
            {
                StartDialogue(line.nextDialogueSo);
                return;
            }

            // Nếu chuyển sang dòng chỉ định
            if (line.nextLineIndex >= 0)
            {
                _currentIndex = line.nextLineIndex;
                ShowCurrentLine();
                return;
            }

            // Nếu là dòng kết thúc
            if (IsEndOfDialogue(line))
            {
                Debug.Log("[Dialogue] End of dialogue!");

                gameObject.SetActive(false);
                return;
            }

            // Mặc định chuyển sang dòng kế tiếp
            _currentIndex++;
            ShowCurrentLine();
        }

        public void StartDialogue(DialogueSo dialogueSo)
        {
            _currentDialogue = dialogueSo;
            _currentIndex = 0;
            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            //Debug
            Debug.Log($"Showing line {_currentIndex} of {_currentDialogue.name}");

            choicesPanel.gameObject.SetActive(false);
            ClearChoices();

            if (_currentDialogue == null || _currentIndex >= _currentDialogue.lines.Length)
            {
                gameObject.SetActive(false);
                return;
            }

            var line = _currentDialogue.lines[_currentIndex];
            ShowSpeaker(line);
            ShowDialogueLine(line);
        }

        private void ShowSpeaker(DialogueLineData line)
        {
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
        }

        private void ShowDialogueLine(DialogueLineData line)
        {
            continueHint.gameObject.SetActive(false);
            _waitingForChoice = false;
            var localizedText =
                LocalizationSettings.StringDatabase.GetLocalizedString("Dialogue", line.localizationKey);

            typewriterEffect.StartTypewriter(localizedText, () => OnTypewriterComplete(line));
        }

        private void OnTypewriterComplete(DialogueLineData line)
        {
            if (line.hasChoices && line.choices != null && line.choices.Length > 0)
            {
                ShowChoices(line.choices);
            }
            else
            {
                continueHint.gameObject.SetActive(true);
                typewriterEffect.StartBlink(continueHint);
            }
        }

        private void ShowChoices(DialogueChoice[] choices)
        {
            ClearChoices();
            choicesPanel.gameObject.SetActive(true);
            _waitingForChoice = true;

            foreach (var choice in choices)
            {
                var btnObj = Instantiate(buttonChoicePrefab, choicesPanel);
                spawnedChoiceButtons.Add(btnObj);

                var btn = btnObj.GetComponent<Button>();
                var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                var localizedChoice =
                    LocalizationSettings.StringDatabase.GetLocalizedString("Dialogue", choice.localizationKey);
                txt.text = localizedChoice;

                btn.onClick.AddListener(() => { OnChoiceSelected(choice); });
            }
        }

        private void OnChoiceSelected(DialogueChoice choice)
        {
            Debug.Log(
                $"[Dialogue] Choice selected: key={choice.localizationKey}, nextSO={choice.nextDialogueSo}, nextIdx={choice.nextLineIndex}");

            choicesPanel.gameObject.SetActive(false);
            _waitingForChoice = false;
            ClearChoices();

            // Nếu có nextDialogueSo, chuyển sang SO mới
            if (choice.nextDialogueSo != null)
            {
                StartDialogue(choice.nextDialogueSo);
                return;
            }

            // Nếu nextLineIndex hợp lệ (>= 0), nhảy tới dòng chỉ định
            if (choice.nextLineIndex >= 0)
            {
                _currentIndex = choice.nextLineIndex;
                ShowCurrentLine();
                return;
            }

            // Nếu nextLineIndex == -1 hoặc giá trị không hợp lệ, kết thúc hội thoại (hoặc xử lý đặc biệt)
            Debug.Log("[Dialogue] End of dialogue by choice!");
            gameObject.SetActive(false);
        }

        private void ClearChoices()
        {
            foreach (var btn in spawnedChoiceButtons)
                if (btn != null)
                    Destroy(btn);

            spawnedChoiceButtons.Clear();
        }
    }
}