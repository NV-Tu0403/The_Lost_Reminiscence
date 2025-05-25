using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Loc_Backend.Scripts
{
    public class VideoIntroPlayer : MonoBehaviour
    {
        public string videoFileName = "intro.mp4";
        public VideoPlayer videoPlayer;
        public GameObject mainMenuPanel;

        void Start()
        {
            mainMenuPanel.SetActive(false);
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoPath;

            videoPlayer.loopPointReached += OnVideoEnd;

            videoPlayer.Play();
        }

        void OnVideoEnd(VideoPlayer vp)
        {
            mainMenuPanel.SetActive(true);
            videoPlayer.gameObject.SetActive(false);
        }
    }
}