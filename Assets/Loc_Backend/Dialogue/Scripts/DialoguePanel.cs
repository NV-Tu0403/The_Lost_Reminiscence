using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Loc_Backend.Dialogue.Scripts
{
    public class DialoguePanel : MonoBehaviour
    {
        [Header("Dialogue Panel")]
        public Image leftAvatar;
        public Image rightAvatar;
        public TextMeshProUGUI leftName;
        public TextMeshProUGUI rightName;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI continueHint;
        public TypewriterEffect typewriterEffect;

        [Header("Branching UI")]
        public Transform choicesPanel;
        public GameObject buttonChoicePrefab;

        private List<GameObject> spawnedChoiceButtons = new List<GameObject>();
        private DialogueSo _currentDialogue;
        private int _currentIndex = 0;
        private bool _waitingForChoice = false;
        private bool IsEndOfDialogue(DialogueLineData line)
        {
            return 
                (line.nextLineIndex == -1 && !line.hasChoices && line.nextDialogueSo == null)
                || (_currentIndex >= _currentDialogue.lines.Length - 1);
        }
        
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleContinueInput();
            }
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

        void ShowCurrentLine()
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

        void ShowSpeaker(DialogueLineData line)
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

        void ShowDialogueLine(DialogueLineData line)
        {
            continueHint.gameObject.SetActive(false);
            _waitingForChoice = false;
            string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("Dialogue", line.localizationKey);

            typewriterEffect.StartTypewriter(localizedText, () => OnTypewriterComplete(line));
        }

        void OnTypewriterComplete(DialogueLineData line)
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

        void ShowChoices(DialogueChoice[] choices)
        {
            ClearChoices();
            choicesPanel.gameObject.SetActive(true);
            _waitingForChoice = true;

            foreach (var choice in choices)
            {
                GameObject btnObj = Instantiate(buttonChoicePrefab, choicesPanel);
                spawnedChoiceButtons.Add(btnObj);

                var btn = btnObj.GetComponent<Button>();
                var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                string localizedChoice = LocalizationSettings.StringDatabase
                    .GetLocalizedString("Dialogue", choice.localizationKey);
                txt.text = localizedChoice;

                btn.onClick.AddListener(() =>
                {
                    OnChoiceSelected(choice);
                });
            }
        }

        void OnChoiceSelected(DialogueChoice choice)
        {
            Debug.Log($"[Dialogue] Choice selected: key={choice.localizationKey}, nextSO={choice.nextDialogueSo}, nextIdx={choice.nextLineIndex}");
            
            choicesPanel.gameObject.SetActive(false);
            _waitingForChoice = false;
            ClearChoices();

            if (choice.nextDialogueSo != null)
            {
                StartDialogue(choice.nextDialogueSo);
                return;
            }
            if (choice.nextLineIndex >= 0)
            {
                _currentIndex = choice.nextLineIndex;
                ShowCurrentLine();
                return;
            }

            var line = _currentDialogue.lines[_currentIndex];
            if (IsEndOfDialogue(line))
            {
                Debug.Log("[Dialogue] End of dialogue!");
                
                gameObject.SetActive(false);
                return;
            }

            _currentIndex++;
            ShowCurrentLine();
        }

        void ClearChoices()
        {
            foreach (var btn in spawnedChoiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedChoiceButtons.Clear();
        }
    }
}