using System;
using Code.GameEventSystem;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace _MyGame.Codes.Timeline
{
    public class TimelineManager : MonoBehaviour
    {
        [Header("UI")]
        public Button skipButton;

        [Header("Spawn Settings")]
        [Tooltip("Thư mục Resources chứa prefab timeline")]
        public string resourcesFolder = "Timelines";

        [Header("Player Handling")]
        [Tooltip("Nếu bật: ẩn hẳn Player trong lúc timeline chạy. Nếu tắt: dùng PlayerLocker để khóa input/physics.")]
        public bool hidePlayerInsteadOfLock;
        public bool lockPlayer = true;    // chỉ dùng khi hidePlayerInsteadOfLock = false
        public string playerTag = "Player";

        // runtime state
        private PlayableDirector currentDirector;
        private GameObject currentInstance;
        private Action onFinished;
        private bool isRunning;

        // Player state
        private GameObject player;
        private PlayerLocker.Snapshot playerSnapshot;
        private bool playerWasActive;

        private void Awake()
        {
            if (skipButton) skipButton.onClick.AddListener(SkipTimeline);
        }

        private void OnEnable()  => EventBus.Subscribe("StartTimeline", OnStartTimelineEvent);
        private void OnDisable() => EventBus.Unsubscribe("StartTimeline", OnStartTimelineEvent);

        private void OnStartTimelineEvent(object data)
        {
            if (data is not BaseEventData eventData) return;
            StartTimeline(eventData.eventId, eventData.OnFinish);
        }

        private void StartTimeline(string timelineId, Action finished)
        {
            // Chặn gọi chồng khi đang chạy
            if (isRunning)
            {
                Debug.LogWarning("[TimelineManager] Timeline đang chạy, bỏ qua yêu cầu mới.");
                finished?.Invoke();
                return;
            }
            isRunning = true;
            this.onFinished = finished;

            // 1) Load prefab
            var path = $"{resourcesFolder}/{timelineId}";
            var prefab = Resources.Load<GameObject>(path);
            if (!prefab)
            {
                Debug.LogError($"[TimelineManager] Không tìm thấy prefab: Resources/{path}.prefab");
                FinishEarly();
                return;
            }

            // 2) Instantiate -> Director
            currentInstance = Instantiate(prefab);
            if (!currentInstance)
            {
                Debug.LogError("[TimelineManager] Instantiate trả về null.");
                FinishEarly();
                return;
            }

            // 3) Lấy PlayableDirector trong instance (có thể nằm ở child)
            currentDirector = currentInstance.GetComponentInChildren<PlayableDirector>(true);
            if (!currentDirector)
            {
                Debug.LogError("[TimelineManager] Prefab thiếu PlayableDirector trong children.");
                FinishEarly();
                return;
            }
            if (currentDirector.playableAsset == null)
            {
                Debug.LogError("[TimelineManager] PlayableDirector chưa gán playableAsset trong prefab timeline.");
                FinishEarly();
                return;
            }

            // 4) Xử lý Player
            player = GameObject.FindGameObjectWithTag(playerTag);
            if (player)
            {
                if (hidePlayerInsteadOfLock)
                {
                    playerWasActive = player.activeSelf;
                    player.SetActive(false);
                }
                else if (lockPlayer)
                {
                    playerSnapshot = PlayerLocker.Lock(player);
                }
            }
            else
            {
                if (hidePlayerInsteadOfLock || lockPlayer)
                    Debug.LogWarning("[TimelineManager] Không tìm thấy Player theo tag để ẩn/khóa.");
            }

            // 5) Play
            currentDirector.stopped += OnTimelineFinished;
            currentDirector.time = 0;
            currentDirector.Play();
        }

        private void SkipTimeline()
        {
            if (currentDirector != null)
                currentDirector.Stop();
        }

        private void OnTimelineFinished(PlayableDirector _)
        {
            if (currentDirector != null)
                currentDirector.stopped -= OnTimelineFinished;

            CleanupAndFinish();
        }

        private void FinishEarly()
        {
            // Gọi khi fail sớm để vẫn callback
            isRunning = false;
            onFinished?.Invoke();
            onFinished = null;
        }

        private void CleanupAndFinish()
        {
            // Khôi phục/hiện lại Player
            if (player)
            {
                if (hidePlayerInsteadOfLock)
                    player.SetActive(playerWasActive);
                else if (lockPlayer)
                    PlayerLocker.Unlock(player, playerSnapshot);
            }

            // Hủy instance
            if (currentInstance) Destroy(currentInstance);
            currentInstance = null;
            currentDirector = null;

            // Callback
            isRunning = false;
            onFinished?.Invoke();
            onFinished = null;
        }
    }
}
