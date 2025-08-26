using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace _MyGame.Codes.Musical
{
    public class BossFinalAudio : MonoBehaviour
    {
        [SerializeField] private EventReference mapBGMEvent;
        private EventInstance _bgmInstance;

        private void Start()
        {
            _bgmInstance = RuntimeManager.CreateInstance(mapBGMEvent);
            _bgmInstance.start();
        }

        private void Update()
        {
            
        }

        private void OnDestroy()
        {
            _bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _bgmInstance.release();
        }
    }
}