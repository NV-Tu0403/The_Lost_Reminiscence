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
            if (GetCutsceneDataFromResource(cutsceneId, onFinished, out var data)) return;

            // Lấy VideoClip từ data
            if (GetVideoClipFormData(cutsceneId, onFinished, data, out var clip)) return;

            ShowUI(data, out var rt);

            PlayCutscene(data);

            Debug.Log($"[CutsceneManager] Playing cutscene: {cutsceneId} (clip: {clip.name})");
        }

        private void PlayCutscene(CutsceneSO data)
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

        private void ShowUI(CutsceneSO data, out RenderTexture rt)
        {
            // Hiển thị UI panel, bật/tắt skip theo data.skippable
            cutscenePanel.SetActive(true);
            skipButton.gameObject.SetActive(data.skippable);

            // Tạo RenderTexture theo độ phân giải clip 
            rt = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = rt;
            cutsceneImage.texture = rt;
        }

        private static bool GetVideoClipFormData(string cutsceneId, Action onFinished, CutsceneSO data, out VideoClip clip)
        {
            clip = data.videoClip;
            if (clip == null)
            {
                Debug.LogError($"[CutsceneManager] videoClip null trong CutsceneDataSO {cutsceneId}");
                onFinished?.Invoke();
                return true;
            }

            return false;
        }

        private static bool GetCutsceneDataFromResource(string cutsceneId, Action onFinished, out CutsceneSO data)
        {
            data = Resources.Load<CutsceneSO>($"Cutscenes/{cutsceneId}");
            if (data == null)
            {
                Debug.LogError($"[CutsceneManager] Không tìm thấy CutsceneDataSO với ID: {cutsceneId}");
                onFinished?.Invoke();
                return true;
            }

            return false;
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