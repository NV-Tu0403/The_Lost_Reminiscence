using UnityEngine;
using FMODUnity;

namespace _MyGame.Codes.Musical
{
    public class BossFinalAudio : MonoBehaviour
    {
        [SerializeField] private EventReference mapBGMEvent; // Sự kiện âm thanh nền của bản đồ
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            AudioManager.Instance.PlayMusic(mapBGMEvent);
            
            FMODSystem.Instance.SetBusVolume("SFX", 0.5f); // Giảm âm lượng bus SFX xuống 50%
        }

        // Update is called once per frame
        private void Update()
        {
        
        }

        private void OnDestroy()
        {
            AudioManager.Instance.StopMusic();
        }
    }
}
