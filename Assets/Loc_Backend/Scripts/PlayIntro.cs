using UnityEngine;
using UnityEngine.Video;

namespace Loc_Backend.Scripts
{
    public class PlayIntro : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public GameObject videoPanel; // Panel chứa RawImage (UI)
        public GameObject mainMenu;   // Menu chính (ẩn khi phát video)

        void Start()
        {
            videoPanel.SetActive(true);
            mainMenu.SetActive(false);

            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();
        }

        void OnVideoEnd(VideoPlayer vp)
        {
            videoPanel.SetActive(false);
            mainMenu.SetActive(true);
        }
    }
}
