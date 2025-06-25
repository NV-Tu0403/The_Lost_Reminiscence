using System;
using Code.GameEventSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Code.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private FullDialoguePanel fullDialoguePanel;
        [SerializeField] private BubbleDialoguePanel bubbleDialoguePanel;
        
        /// <summary>
        /// Quản lý hiển thị hội thoại:
        /// - Singleton quản lý toàn bộ hệ thống dialogue.
        /// - Chứa tham chiếu tới FullDialoguePanel và BubbleDialoguePanel.
        /// - Ẩn panel khi khởi tạo.
        /// </summary>
        private void Awake()
        {
            if (fullDialoguePanel != null) fullDialoguePanel.gameObject.SetActive(false);
            else Debug.Log("Hệ thống chưa gán:" + fullDialoguePanel );
            
            if (bubbleDialoguePanel != null) bubbleDialoguePanel.gameObject.SetActive(false);
            else Debug.Log("Hệ thống chưa gán:" + bubbleDialoguePanel );
        }

        /// <summary>
        /// Bắt đầu hội thoại:
        /// - Nhận dialogueId và callback khi kết thúc.
        /// - Load DialogueNodeSO từ Addressables.
        /// - Khi load xong, gọi CheckDisplayDialogue để chọn panel phù hợp.
        /// </summary>
        public void StartDialogue(string dialogueId, Action onFinish)
        {
            if (dialogueId == null) return;
            Addressables.LoadAssetAsync<DialogueNodeSO>(dialogueId).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    CheckDisplayDialogue(dialogueId, onFinish, handle);
                }
                else
                    Debug.Log($"Hệ thống không truy xuất được dialogue với ID: {dialogueId}");
            };
        }

        private void CheckDisplayDialogue(string dialogueId, Action onFinish, AsyncOperationHandle<DialogueNodeSO> handle)
        {
            var dialogue = handle.Result;
            switch (dialogue.displayMode)
            {
                case DialogueDisplayMode.FullPanel:
                    fullDialoguePanel.ShowDialogue(dialogue, onFinish);
                    Debug.Log("[DialogueManager] Hiển thị FullDialoguePanel: " + dialogueId);
                    break;
                case DialogueDisplayMode.Bubble:
                    bubbleDialoguePanel.ShowDialogue(dialogue, onFinish);
                    Debug.Log("[DialogueManager] Hiển thị BubbleDialoguePanel: " + dialogueId);
                    break;
                default:
                    Debug.LogWarning($"DialogueNodeSO {dialogueId} không có displayMode hợp lệ!");
                    break;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe("StartDialogue", OnStartDialogueEvent);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe("StartDialogue", OnStartDialogueEvent);
        }
        private void OnStartDialogueEvent(object data)
        {
            Debug.Log($"[DialogueManager] OnStartDialogueEvent received | Data: {data}");
            var eventData = data as BaseEventData;
            if (eventData == null) return;
            Debug.Log($"[DialogueManager] Starting dialogue with eventId: {eventData.eventId}");
            StartDialogue(eventData.eventId, eventData.onFinish);
        }
    }
}
