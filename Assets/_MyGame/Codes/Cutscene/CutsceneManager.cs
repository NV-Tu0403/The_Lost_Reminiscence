using System;
using _MyGame.Codes.GameEventSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Code.Cutscene
{
    public class CutsceneManager : MonoBehaviour
    {
        [Header("UI Elements")] 
        public GameObject cutscenePanel;
        public RawImage cutsceneImage;
        public VideoPlayer videoPlayer;
        public AudioSource audioSource;
        public Button skipButton;

        private Action onFinished;
        
        private void Awake()
        {
            cutscenePanel.SetActive(false);
            skipButton.onClick.AddListener(SkipCutscene);
        }

        private void OnEnable()
        {
            EventBus.Subscribe("StartCutscene", OnStartCutsceneEvent);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("StartCutscene", OnStartCutsceneEvent);
        }
        
        private void OnStartCutsceneEvent(object data)
        {
            if (data is not BaseEventData eventData) return;
            StartCutscene(eventData.eventId, eventData.OnFinish);
        }

        private void StartCutscene(string cutsceneId, Action onFinished)
        {
            if (cutsceneId == null) return;
            this.onFinished = onFinished;
            
            // Hiển thị chuột
            Core.Instance.IsCutscenePlaying = true;
            Core.Instance.ActiveMouseCursor(true);
            
            //  Tự load CutsceneDataSO asset từ Resources/Cutscenes/eventId
            if (GetCutsceneDataFromResource(cutsceneId, onFinished, out var data)) return;

            // Lấy VideoClip từ data
            if (GetVideoClipFormData(cutsceneId, onFinished, data, out var clip)) return;

            ShowUI(data, out var rt);

            PlayCutscene(data);
        }

        private void PlayCutscene(CutsceneSo data)
        {
            // Play video
            videoPlayer.clip = data.videoClip;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.Play();

            // Play separate audio if available
            if (data.audioClip != null)
            {
                audioSource.clip = data.audioClip;
                audioSource.Play();
            }
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        private void ShowUI(CutsceneSo data, out RenderTexture rt)
        {
            // Hiển thị UI panel, bật/tắt skip theo data.skippable
            cutscenePanel.SetActive(true);
            skipButton.gameObject.SetActive(data.skippable);

            // Tạo RenderTexture theo độ phân giải clip 
            rt = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = rt;
            cutsceneImage.texture = rt;
        }

        private static bool GetVideoClipFormData(string cutsceneId, Action onFinished, CutsceneSo data, out VideoClip clip)
        {
            clip = data.videoClip;
            if (clip != null) return false;
            Debug.LogError($"[CutsceneManager] videoClip null trong CutsceneDataSO {cutsceneId}");
            onFinished?.Invoke();
            return true;
        }

        private static bool GetCutsceneDataFromResource(string cutsceneId, Action onFinished, out CutsceneSo data)
        {
            data = Resources.Load<CutsceneSo>($"Cutscenes/{cutsceneId}");
            if (data != null) return false;
            Debug.LogError($"[CutsceneManager] Không tìm thấy CutsceneDataSO với ID: {cutsceneId}");
            onFinished?.Invoke();
            return true;

        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            EndCutscene();
        }
        
        private void SkipCutscene()
        {
            EndCutscene();
        }
        
        private void EndCutscene()
        {
            // Bỏ listener
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.Stop();

            // Ẩn panel
            cutscenePanel.SetActive(false);

            // Gọi callback để báo cutscene đã xong
            onFinished?.Invoke();
            onFinished = null;
            
            // Ẩn chuột
            Core.Instance.IsCutscenePlaying = false;
            Core.Instance.ActiveMouseCursor(false);

            // Phát sự kiện để các hệ thống khác biết cutscene đã kết thúc
            EventBus.Publish("EndCutscene");
        }
    }
}