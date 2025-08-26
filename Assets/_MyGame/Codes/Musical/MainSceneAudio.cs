using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _MyGame.Codes.Musical
{
    public class MainSceneAudio : MonoBehaviour
    {
        [SerializeField] private EventReference mapBGMEvent;
        [SerializeField] private EventReference ambientGbEvent;
        
        [SerializeField] private EventReference noteFmodEvent;
        private EventInstance _bgmInstance;
        private EventInstance _ambientInstance;
        
        private void Start()
        {
            _ambientInstance = RuntimeManager.CreateInstance(ambientGbEvent);
            _ambientInstance.start();
        }
        
        public void PlayNoteSound()
        {
            if (!noteFmodEvent.IsNull)
            {
                RuntimeManager.PlayOneShot(noteFmodEvent);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _bgmInstance = RuntimeManager.CreateInstance(mapBGMEvent);
                _bgmInstance.start();
            }
        }


        private void OnDestroy()
        {
            _bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _bgmInstance.release();
            
            _ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _ambientInstance.release();
        }
    }
}
