using System;
using Functions.Dialogue.Scripts;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Events.Dialogue.Scripts
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialoguePanel dialoguePanel;
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
            
            if (dialoguePanel != null) dialoguePanel.gameObject.SetActive(false);
            else Debug.Log("[DialogueManager] Hệ thống chưa gán dialoguePanel: " + dialoguePanel );
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
                    Debug.Log($"Hệ thống không truy xuất được dialogue với ID: {dialogueId}");
            };
        }
    }
}

