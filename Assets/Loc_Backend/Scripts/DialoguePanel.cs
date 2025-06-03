using System;
using Loc_Backend.Dialogue.Scripts.SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loc_Backend.Dialogue.Scripts.UI
{
    public class DialoguePanel : MonoBehaviour
    {
        public Image leftAvatar, rightAvatar;
        public TextMeshProUGUI leftName, rightName, dialogueText;
        public Transform choicesPanel;
        public Button choiceButtonPrefab;
        public Button nextButton;

        private Action onDialogueEnd;
        private DialogueNodeSO currentNode;

        public void ShowDialogue(DialogueNodeSO node, Action onEnd)
        {
            Debug.Log($"[DialoguePanel] ShowDialogue: {node?.name}");
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;
            currentNode = null; // Reset current node
            nextButton.onClick.RemoveAllListeners(); // Reset listeners
            ShowNode(node);
        }

        void ShowNode(DialogueNodeSO node)
        {
            if (currentNode == node) return; // Ngăn double invoke
            currentNode = node;
            Debug.Log($"[DialoguePanel] ShowNode: {node?.name}");
            ShowSpeaker(node);
            ShowDialogueText(node);
            ClearChoices();

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
            Debug.Log($"[DialoguePanel] ShowNextButton: {node?.name}, nextNode: {node?.nextNode?.name}");
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
                var btn = Instantiate(choiceButtonPrefab, choicesPanel);

                // Lấy text từ LocalizedString
                choice.choiceText.StringChanged += (localizedText) =>
                {
                    btn.GetComponentInChildren<TextMeshProUGUI>().text = localizedText;
                };
                choice.choiceText.RefreshString();

                btn.onClick.AddListener(() => OnChoiceSelected(choice));
            }
        }

        private void ClearChoices()
        {
            foreach (Transform child in choicesPanel)
                Destroy(child.gameObject);
        }

        private void ShowDialogueText(DialogueNodeSO node)
        {
            node.dialogueText.StringChanged += SetDialogueText;
            node.dialogueText.RefreshString(); // Cập nhật nếu đổi ngôn ngữ

            void SetDialogueText(string localizedText)
            {
                dialogueText.text = localizedText;
                node.dialogueText.StringChanged -= SetDialogueText; // Xóa event cho node cũ
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
            Debug.Log("[DialoguePanel] EndDialogue called");
            gameObject.SetActive(false);
            onDialogueEnd?.Invoke();
        }
    }
}

