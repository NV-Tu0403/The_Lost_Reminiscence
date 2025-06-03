using System;
using System.Collections.Generic;
using Loc_Backend.Dialogue.Scripts.SO;
using Loc_Backend.Dialogue.Scripts.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Loc_Backend.Scripts
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialoguePanel dialoguePanel;
        [SerializeField] private Script.GameEventSystem.EventExecutor eventExecutor;
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
            
            if (dialoguePanel == null)
            {
                Debug.LogError("DialoguePanel is not assigned in DialogueManager.");
            }
            
            dialoguePanel.gameObject.SetActive(false);
        }

        public void StartDialogue(string dialogueId)
        {
            Addressables.LoadAssetAsync<DialogueNodeSO>(dialogueId).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var dialogue = handle.Result;
                    Debug.Log($"Showing dialogue: {dialogueId}");
                    dialoguePanel.ShowDialogue(dialogue, OnDialogueEnd);
                }
                else
                {
                    Debug.LogWarning($"Dialogue SO not found (Addressable): {dialogueId}");
                }
            };
        }
        
        private void OnDialogueEnd()
        {
            Debug.Log("Dialogue ended");
            if (eventExecutor != null)
                eventExecutor.OnDialogueFinished();
            else
                Debug.LogWarning("[DialogueManager] EventExecutor is not assigned.");
        }
    }
}

