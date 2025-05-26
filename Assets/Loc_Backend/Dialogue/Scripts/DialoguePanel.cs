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

        
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleContinueInput();
            }
        }
        
        public void StartDialogue(DialogueSo dialogueSo)
        {
            _currentDialogue = dialogueSo;
            _currentIndex = 0;
            ShowCurrentLine();
        }

        void ShowCurrentLine()
        {
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
            choicesPanel.gameObject.SetActive(false);
            _waitingForChoice = false;
            ClearChoices();

            if (choice.nextDialogueSo != null)
            {
                StartDialogue(choice.nextDialogueSo);
            }
            else if (choice.nextLineIndex >= 0)
            {
                _currentIndex = choice.nextLineIndex;
                ShowCurrentLine();
            }
            else
            {
                _currentIndex++;
                ShowCurrentLine();
            }
        }

        void ClearChoices()
        {
            foreach (var btn in spawnedChoiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedChoiceButtons.Clear();
        }

        public void HandleContinueInput()
        {
            if (_currentDialogue == null) return;
            if (_waitingForChoice) return;

            if (continueHint.gameObject.activeSelf)
            {
                typewriterEffect.StopBlink();
                continueHint.gameObject.SetActive(false);

                var line = _currentDialogue.lines[_currentIndex];
                if (line.nextDialogueSo != null)
                {
                    StartDialogue(line.nextDialogueSo);
                }
                else if (line.nextLineIndex >= 0)
                {
                    _currentIndex = line.nextLineIndex;
                    ShowCurrentLine();
                }
                else
                {
                    _currentIndex++;
                    ShowCurrentLine();
                }
            }
        }
    }
}