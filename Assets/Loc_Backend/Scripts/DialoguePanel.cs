using System;
using System.Collections;
using Loc_Backend.Dialogue.Scripts.SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loc_Backend.Scripts
{
    public class DialoguePanel : MonoBehaviour
    {
        public Image leftAvatar, rightAvatar;
        public TextMeshProUGUI leftName, rightName, dialogueText;
        public Transform choicesPanel;
        public Button choiceButtonPrefab;
        public Button nextButton;

        private Action onDialogueEnd;

        public void ShowDialogue(DialogueNodeSO node, Action onEnd)
        {
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;
            ShowNode(node);
        }

        void ShowNode(DialogueNodeSO node)
        {
            ShowSpeaker(node);
            ClearChoices();
            StopAllCoroutines();
            StartCoroutine(ShowDialogueTextCoroutine(node));
        }

        private IEnumerator ShowDialogueTextCoroutine(DialogueNodeSO node)
        {
            string fullText = "";
            node.dialogueText.StringChanged += (localizedText) => fullText = localizedText;
            node.dialogueText.RefreshString();
            while (string.IsNullOrEmpty(fullText)) yield return null;
            dialogueText.text = "";
            for (int i = 0; i < fullText.Length; i++)
            {
                dialogueText.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(0.02f);
            }
            // Đảm bảo text đầy đủ
            dialogueText.text = fullText;
            // Hiện next hoặc choices sau khi chạy xong
            if (node.choices != null && node.choices.Length > 0)
            {
                ShowChoices(node);
            }
            else
            {
                ShowNextButton(node);
            }
        }

        private void ShowNextButton(DialogueNodeSO node)
        {
            choicesPanel.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);

            nextButton.onClick.RemoveAllListeners();
            if (node.nextNode != null)
                nextButton.onClick.AddListener(() => ShowNode(node.nextNode));
            else
                nextButton.onClick.AddListener(EndDialogue);
        }

        private void ShowChoices(DialogueNodeSO node)
        {
            choicesPanel.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);

            foreach (var choice in node.choices)
            {
                CreateChoiceButton(choice);
            }
        }

        private void CreateChoiceButton(DialogueChoiceData choice)
        {
            var btn = Instantiate(choiceButtonPrefab, choicesPanel);
            var capturedChoice = choice;
            // Lấy text từ LocalizedString
            capturedChoice.choiceText.StringChanged += (localizedText) =>
            {
                if (btn != null && btn.gameObject != null)
                {
                    var textComp = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                        textComp.text = localizedText;
                }
            };
            capturedChoice.choiceText.RefreshString();

            btn.onClick.AddListener(() => {
                if (btn != null && btn.gameObject != null)
                {
                    OnChoiceSelected(capturedChoice);
                }
            });
        }

        private void ClearChoices()
        {
            foreach (Transform child in choicesPanel)
            {
                var btn = child.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
                Destroy(child.gameObject);
            }
        }

        private void ShowSpeaker(DialogueNodeSO node)
        {
            if (node.isLeftSpeaker)
            {
                leftAvatar.gameObject.SetActive(true);
                leftAvatar.sprite = node.speakerAvatar;
                leftName.text = node.speakerName;
                rightAvatar.gameObject.SetActive(false);
                rightName.text = "";
            }
            else
            {
                rightAvatar.gameObject.SetActive(true);
                rightAvatar.sprite = node.speakerAvatar;
                rightName.text = node.speakerName;
                leftAvatar.gameObject.SetActive(false);
                leftName.text = "";
            }
        }

        void OnChoiceSelected(DialogueChoiceData choice)
        {
            ShowNode(choice.nextNode);
        }

        void EndDialogue()
        {
            gameObject.SetActive(false);
            onDialogueEnd?.Invoke();
        }
    }
}

