using System;
using _MyGame.Codes.GameEventSystem;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _MyGame.Codes.Timeline
{
    public class TimelineManager : MonoBehaviour
    {
        [Header("Timeline UI Panel")]
        [Tooltip("Panel chứa UI timeline (bao gồm nút skip), sẽ hiển thị trên cùng khi timeline chạy")]
        public GameObject timelineUIPanel;
        [Tooltip("Canvas của timeline panel để set sortingOrder cao")]
        public Canvas timelinePanelCanvas;
        [Tooltip("SortingOrder cho timeline panel (mặc định 1000 để luôn hiển thị trên cùng)")]
        public int timelinePanelSortingOrder = 1000;
        [Tooltip("Nút Skip để bỏ qua timeline, nếu không có sẽ không hiển thị")]
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
            // Khởi tạo timeline panel
            InitializeTimelinePanel();
            
            if (!skipButton) return;
            skipButton.onClick.AddListener(SkipTimeline);
            
            // Ẩn panel khi chưa chạy timeline
            if (timelineUIPanel)
                timelineUIPanel.SetActive(false);
        }

        /// <summary>
        /// Khởi tạo timeline UI panel với Canvas có sortingOrder cao
        /// </summary>
        private void InitializeTimelinePanel()
        {
            if (!timelineUIPanel) return;

            // Đảm bảo panel có Canvas component
            if (!timelinePanelCanvas)
                timelinePanelCanvas = timelineUIPanel.GetComponent<Canvas>();

            if (!timelinePanelCanvas)
                timelinePanelCanvas = timelineUIPanel.AddComponent<Canvas>();

            // Cấu hình Canvas để hiển thị trên cùng
            timelinePanelCanvas.overrideSorting = true;
            timelinePanelCanvas.sortingOrder = timelinePanelSortingOrder;

            // Đảm bảo có GraphicRaycaster cho UI interaction
            if (!timelineUIPanel.GetComponent<GraphicRaycaster>())
                timelineUIPanel.AddComponent<GraphicRaycaster>();
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

            //Hiện thị nút Skip, Mouse cursor
            ActiveMethod();
            
            // 1) Load prefab
            if (LoadPrefabs(timelineId, out var prefab)) return;

            // 2) Instantiate -> Director
            if (Instantiate(prefab)) return;

            // 3) Lấy PlayableDirector trong instance (có thể nằm ở child)
            if (GetPlayableDirector()) return;

            // 4) Xử lý Player
            PlayerCheck();
            
            // 5) Play
            PlayTimelines();
        }

        private void ActiveMethod()
        {
            // Đặt trạng thái timeline đang chạy TRƯỚC khi bật cursor (giống CutsceneManager)
            Core.Instance.IsTimelinePlaying = true;
            Core.Instance.ActiveMouseCursor(true);
            
            // Hiển thị timeline panel 
            if (timelineUIPanel && !timelineUIPanel.activeSelf)
            {
                timelineUIPanel.SetActive(true);
            }
        }

        private void PlayTimelines()
        {
            currentDirector.stopped += OnTimelineFinished;
            currentDirector.time = 0;
            currentDirector.Play();
        }

        private void PlayerCheck()
        {
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
        }

        private bool GetPlayableDirector()
        {
            currentDirector = currentInstance.GetComponentInChildren<PlayableDirector>(true);
            if (!currentDirector)
            {
                Debug.LogError("[TimelineManager] Prefab thiếu PlayableDirector trong children.");
                FinishEarly();
                return true;
            }

            if (currentDirector.playableAsset != null) return false;
            Debug.LogError("[TimelineManager] PlayableDirector chưa gán playableAsset trong prefab timeline.");
            FinishEarly();
            return true;
        }

        private bool Instantiate(GameObject prefab)
        {
            currentInstance = Object.Instantiate(prefab);
            if (currentInstance) return false;
            Debug.LogError("[TimelineManager] Instantiate trả về null.");
            FinishEarly();
            return true;
        }

        private bool LoadPrefabs(string timelineId, out GameObject prefab)
        {
            var path = $"{resourcesFolder}/{timelineId}";
            prefab = Resources.Load<GameObject>(path);
            if (prefab) return false;
            Debug.LogError($"[TimelineManager] Không tìm thấy prefab: Resources/{path}.prefab");
            FinishEarly();
            return true;

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
            // Ẩn timeline panel thay vì chỉ nút skip
            if (timelineUIPanel && timelineUIPanel.activeSelf)
            {
                timelineUIPanel.SetActive(false);
            }
            
            Core.Instance.IsTimelinePlaying = false;
            Core.Instance.ActiveMouseCursor(false);
            
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
