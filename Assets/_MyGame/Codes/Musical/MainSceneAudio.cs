using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _MyGame.Codes.Musical
{
    // Quản lý âm thanh chính cho Main Scene
    public class MainSceneAudio : MonoBehaviour
    {
        // Sự kiện BGM bản đồ
        [SerializeField] private EventReference mapBGMEvent;
        // Sự kiện âm thanh ambient
        [SerializeField] private EventReference ambientGbEvent;
        private EventInstance _bgmInstance;
        private EventInstance _ambientInstance;
        
        // Khởi tạo và phát ambient khi bắt đầu scene
        // private void Start()
        // {
        //     _ambientInstance = RuntimeManager.CreateInstance(ambientGbEvent);
        //     _ambientInstance.start();
        // }
        
        // Khi player đi vào trigger, phát BGM
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("[MainSceneAudio] Player entered BGM trigger area." + mapBGMEvent);
            _bgmInstance = RuntimeManager.CreateInstance(mapBGMEvent);
            _bgmInstance.start();
        }

        // Giải phóng tài nguyên âm thanh khi object bị huỷ
        private void OnDestroy()
        {
            _bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _bgmInstance.release();
            
            _ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _ambientInstance.release();
        }
    }
}
