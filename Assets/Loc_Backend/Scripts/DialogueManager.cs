using System;
using Loc_Backend.Dialogue.Scripts.UI;
using Loc_Backend.Dialogue.Scripts.SO;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts.Manager
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        public DialoguePanel dialoguePanel;

        private Action onDialogueEnd;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void StartDialogue(DialogueNodeSO rootNode, Action onEnd)
        {
            onDialogueEnd = onEnd;
            dialoguePanel.ShowDialogue(rootNode, HandleDialogueEnd);
        }

        private void HandleDialogueEnd()
        {
            onDialogueEnd?.Invoke();
        }
    }
}