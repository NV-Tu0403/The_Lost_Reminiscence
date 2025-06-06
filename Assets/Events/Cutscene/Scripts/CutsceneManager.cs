using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Events.Cutscene.Scripts
{
    public class CutsceneManager : MonoBehaviour
    {
        // Xử lí các cutscene trong game
        public static CutsceneManager Instance { get; private set; }

        [Header("UI Elements")] 
        public GameObject cutscenePanel;
        public RawImage cutsceneImage;
        public VideoPlayer videoPlayer;
        public AudioSource audioSource;
        public Button skipButton;

        private Action _onFinished;
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
            
            cutscenePanel.SetActive(false);
            skipButton.onClick.AddListener(SkipCutscene);
        }
        
        public void StartCutscene(string cutsceneId, Action onFinished)
        {
            if (cutsceneId == null) return;
            
            _onFinished = onFinished;
            
            //  Tự load CutsceneDataSO asset từ Resources/Cutscenes/eventId
            var data = Resources.Load<CutsceneSO>($"Cutscenes/{cutsceneId}");
            if (data == null)
            {
                Debug.LogError($"[CutsceneManager] Không tìm thấy CutsceneDataSO với ID: {cutsceneId}");
                onFinished?.Invoke();
                return;
            }

            // Lấy VideoClip từ data
            var clip = data.videoClip;
            if (clip == null)
            {
                Debug.LogError($"[CutsceneManager] videoClip null trong CutsceneDataSO {cutsceneId}");
                onFinished?.Invoke();
                return;
            }

            // Hiển thị UI panel, bật/tắt skip theo data.skippable
            cutscenePanel.SetActive(true);
            skipButton.gameObject.SetActive(data.skippable);

            // Tạo RenderTexture theo độ phân giải clip 
            RenderTexture rt = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = rt;
            cutsceneImage.texture = rt;

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

            Debug.Log($"[CutsceneManager] Playing cutscene: {cutsceneId} (clip: {clip.name})");
        }
        
        private void OnVideoEnd(VideoPlayer vp)
        {
            Debug.Log("[CutsceneManager] Cutscene ended.");
            EndCutscene();
        }
        
        private void SkipCutscene()
        {
            Debug.Log("[CutsceneManager] Cutscene skipped.");
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
            _onFinished?.Invoke();
            _onFinished = null;
            
            Debug.Log("[CutsceneManager] Cutscene ended and cleaned up.");
        }
    }
}