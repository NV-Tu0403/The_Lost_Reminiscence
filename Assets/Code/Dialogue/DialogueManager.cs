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
        [SerializeField] private StoryDialoguePanel storyDialoguePanel; 
        
        /// <summary>
        /// Quản lý hiển thị hội thoại:
        /// - Singleton quản lý toàn bộ hệ thống dialogue.
        /// - Chứa tham chiếu tới FullDialoguePanel và BubbleDialoguePanel.
        /// - Ẩn panel khi khởi tạo.
        /// </summary>
        private void Awake()
        {
            if (fullDialoguePanel != null) fullDialoguePanel.gameObject.SetActive(false);
            else Debug.Log("[DialogueManager] Hệ thống chưa gán:" + fullDialoguePanel );
            
            if (bubbleDialoguePanel != null) bubbleDialoguePanel.gameObject.SetActive(false);
            else Debug.Log("[DialogueManager] Hệ thống chưa gán:" + bubbleDialoguePanel );
            
            if (storyDialoguePanel != null) storyDialoguePanel.gameObject.SetActive(false);
            else Debug.Log("[DialogueManager] Hệ thống chưa gán:" + storyDialoguePanel );
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

        /// <summary>
        /// Kiểm tra chế độ hiển thị của dialogue:
        /// - Dựa vào displayMode của DialogueNodeSO để quyết định sử dụng panel nào.
        /// - Hiển thị FullDialoguePanel, BubbleDialoguePanel hoặc StoryDialoguePanel tương ứng
        /// - Nếu không có displayMode hợp lệ, ghi log cảnh báo.
        /// </summary>
        private void CheckDisplayDialogue(string dialogueId, Action onFinish, AsyncOperationHandle<DialogueNodeSO> handle)
        {
            var dialogue = handle.Result;
            var onDialogueEnd = CallEvent(onFinish);
            // Kiểm tra displayMode và hiển thị panel tương ứng
            switch (dialogue.displayMode)
            {
                case DialogueDisplayMode.FullPanel:
                    fullDialoguePanel.ShowDialogue(dialogue, onDialogueEnd);
                    Debug.Log("[DialogueManager] Hiển thị FullDialoguePanel: " + dialogueId);
                    break;
                case DialogueDisplayMode.BubblePanel:
                    bubbleDialoguePanel.ShowDialogue(dialogue, onDialogueEnd);
                    Debug.Log("[DialogueManager] Hiển thị BubbleDialoguePanel: " + dialogueId);
                    break;
                case DialogueDisplayMode.StoryPanel:
                    storyDialoguePanel.ShowDialogue(dialogue, onDialogueEnd);
                    Debug.Log("[DialogueManager] Hiển thị StoryDialoguePanel: " + dialogueId);
                    break;
                default:
                    Debug.LogWarning($"DialogueNodeSO {dialogueId} không có displayMode hợp lệ!");
                    break;
            }
        }

        /// <summary>
        /// Tạo callback cho sự kiện bắt đầu hội thoại:
        /// - Phát sự kiện "StartDialogue" để thông báo bắt đầu hội thoại.
        /// - Tạo callback để phát sự kiện "EndDialogue" khi hội thoại kết thúc.
        /// </summary>
        private static Action CallEvent(Action onFinish)
        {
            // Phát event bắt đầu hội thoại
            EventBus.Publish("StartDialogue");
            // Tạo callback kết thúc hội thoại
            Action onDialogueEnd = () =>
            {
                EventBus.Publish("EndDialogue");
                onFinish?.Invoke();
            };
            return onDialogueEnd;
        }

        /// <summary>
        /// Bắt đầu hội thoại từ sự kiện "StartDialogue":
        /// - Lắng nghe sự kiện từ EventBus.
        /// - Khi sự kiện xảy ra, gọi StartDialogue với eventId và callback.
        /// </summary>
        
        private void OnEnable()
        {
            EventBus.Subscribe("StartDialogue", OnStartDialogueEvent);
        }
        
        /// <summary>
        /// Hủy đăng ký sự kiện khi không cần thiết:
        /// - Đảm bảo không còn lắng nghe sự kiện "StartDialogue" khi DialogueManager bị vô hiệu hóa.
        /// </summary>
        private void OnDisable()
        {
            EventBus.Unsubscribe("StartDialogue", OnStartDialogueEvent);
        }
        
        /// <summary>
        /// Xử lý sự kiện bắt đầu hội thoại:
        /// - Nhận dữ liệu từ sự kiện, kiểm tra kiểu dữ liệu.
        /// - Nếu dữ liệu là BaseEventData, lấy eventId và callback.
        /// - Gọi StartDialogue với eventId và callback.
        /// </summary>
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
