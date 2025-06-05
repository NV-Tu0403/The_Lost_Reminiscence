using System;
using Loc_Backend.Dialogue.Scripts.SO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Loc_Backend.Scripts
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialoguePanel dialoguePanel;
        
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

        public void StartDialogue(string dialogueId, Action onFinish)
        {
            if (dialogueId == null) return;
            
            Addressables.LoadAssetAsync<DialogueNodeSO>(dialogueId).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var dialogue = handle.Result;
                    dialoguePanel.ShowDialogue(dialogue, onFinish);
                }
                else
                    Debug.LogWarning($"Dialogue SO not found (Addressable): {dialogueId}");
            };
        }
    }
}

