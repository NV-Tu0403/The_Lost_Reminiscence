using System;
using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Timeline;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _MyGame.Codes.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private FullDialoguePanel fullDialoguePanel;
        [SerializeField] private BubbleDialoguePanel bubbleDialoguePanel;
        [SerializeField] private StoryDialoguePanel storyDialoguePanel; 
        
        [Header("Player Lock")]
        [SerializeField] private bool lockPlayerOnFullDialogue = true;
        [SerializeField] private string playerTag = "Player";
        private GameObject lockedPlayer;
        private PlayerLocker.Snapshot playerSnapshot;
        private bool isPlayerLocked;

        // Track deferred hide subscription to avoid duplicates
        private bool waitingHideBubble;
        
        /// <summary>
        /// Quản lý hiển thị hội thoại:
        /// - Singleton quản lý toàn bộ hệ thống dialogue.
        /// - Chứa tham chiếu tới FullDialoguePanel và BubbleDialoguePanel.
        /// - Ẩn panel khi khởi tạo.
        /// </summary>
        public static DialogueManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (fullDialoguePanel != null) fullDialoguePanel.gameObject.SetActive(false);
            if (bubbleDialoguePanel != null) bubbleDialoguePanel.gameObject.SetActive(false);
            if (storyDialoguePanel != null) storyDialoguePanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// Bắt đầu hội thoại:
        /// - Nhận dialogueId và callback khi kết thúc.
        /// - Load DialogueNodeSO từ Addressables.
        /// - Khi load xong, gọi CheckDisplayDialogue để chọn panel phù hợp.
        /// </summary>
        private void StartDialogue(string dialogueId, Action onFinish)
        {
            if (dialogueId == null) return;
            Addressables.LoadAssetAsync<DialogueNodeSo>(dialogueId).Completed += handle =>
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
        private void CheckDisplayDialogue(string dialogueId, Action onFinish, AsyncOperationHandle<DialogueNodeSo> handle)
        {
            var dialogue = handle.Result;
            var onDialogueEnd = CallEvent(onFinish);

            // Kiểm tra displayMode và hiển thị panel tương ứng
            switch (dialogue.displayMode)
            {
                case DialogueDisplayMode.FullPanel:
                    LockPlayer();
                    fullDialoguePanel.ShowDialogue(dialogue, WrapEndWithUnlock(onDialogueEnd));
                    break;
                case DialogueDisplayMode.BubblePanel:
                    bubbleDialoguePanel.ShowDialogue(dialogue, onDialogueEnd);
                    break;
                case DialogueDisplayMode.StoryPanel:
                    storyDialoguePanel.ShowDialogue(dialogue, onDialogueEnd);
                    break;
                case DialogueDisplayMode.None:
                default:
                    Debug.LogWarning($"DialogueNodeSO {dialogueId} không có displayMode hợp lệ!");
                    break;
            }

            return;

            // Helper to wrap end callback with unlock when needed
            Action WrapEndWithUnlock(Action endCb)
            {
                return () =>
                {
                    UnlockPlayer();
                    endCb?.Invoke();
                };
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

            return OnDialogueEnd;

            // Tạo callback kết thúc hội thoại
            void OnDialogueEnd()
            {
                EventBus.Publish("EndDialogue");
                onFinish?.Invoke();
            }
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
            // In case object is disabled while a full dialogue is active, ensure unlock
            UnlockPlayer();
        }
        
        /// <summary>
        /// Xử lý sự kiện bắt đầu hội thoại:
        /// - Nhận dữ liệu từ sự kiện, kiểm tra kiểu dữ liệu.
        /// - Nếu dữ liệu là BaseEventData, lấy eventId và callback.
        /// - Gọi StartDialogue với eventId và callback.
        /// </summary>
        private void OnStartDialogueEvent(object data)
        {
            if (data is not BaseEventData eventData) return;
            StartDialogue(eventData.eventId, eventData.OnFinish);
        }

        // Public API cho tutorial gọi bubble persistent
        public void ShowBubbleTutorial(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId) || bubbleDialoguePanel == null) return;
            Addressables.LoadAssetAsync<DialogueNodeSo>(dialogueId).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    bubbleDialoguePanel.ShowDialoguePersistent(handle.Result);
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Không load được dialogue (tutorial) id={dialogueId}");
                }
            };
        }
        
        public void HideBubbleTutorial()
        {
            if (bubbleDialoguePanel == null) return;
            // If waiting for deferred hide, unsubscribe first
            if (waitingHideBubble)
            {
                TryUnsubscribeBubbleTyping();
                waitingHideBubble = false;
            }
            bubbleDialoguePanel.HideManually();
        }

        // New: hide bubble after typewriter finishes; if not typing, hide immediately
        public void HideBubbleTutorialDeferred()
        {
            if (bubbleDialoguePanel == null) return;
            if (!bubbleDialoguePanel.gameObject.activeInHierarchy)
            {
                return; // nothing to hide
            }
            if (bubbleDialoguePanel.IsTyping)
            {
                if (waitingHideBubble) return; // already waiting
                bubbleDialoguePanel.TypingCompleted += OnBubbleTypingCompleted;
                waitingHideBubble = true;
            }
            else
            {
                HideBubbleTutorial();
            }
        }

        private void OnBubbleTypingCompleted()
        {
            TryUnsubscribeBubbleTyping();
            waitingHideBubble = false;
            HideBubbleTutorial();
        }

        private void TryUnsubscribeBubbleTyping()
        {
            if (bubbleDialoguePanel != null)
            {
                bubbleDialoguePanel.TypingCompleted -= OnBubbleTypingCompleted;
            }
        }

        private void LockPlayer()
        {
            if (!lockPlayerOnFullDialogue || isPlayerLocked) return;
            var player = !string.IsNullOrEmpty(playerTag) ? GameObject.FindGameObjectWithTag(playerTag) : null;
            if (!player)
            {
                Debug.LogWarning("[DialogueManager] Không tìm thấy Player theo tag để khóa input trong FullDialogue.");
                return;
            }
            lockedPlayer = player;
            playerSnapshot = PlayerLocker.Lock(player);
            isPlayerLocked = true;
        }

        private void UnlockPlayer()
        {
            if (!isPlayerLocked)
                return;

            if (lockedPlayer)
            {
                PlayerLocker.Unlock(lockedPlayer, playerSnapshot);
            }
            lockedPlayer = null;
            isPlayerLocked = false;
        }
    }
}
