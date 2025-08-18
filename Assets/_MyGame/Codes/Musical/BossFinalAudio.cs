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
            if (Input.GetKeyDown(KeyCode.H))
            {
                _bgmInstance.setParameterByName("FieldOfAge", 0f);
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                _bgmInstance.setParameterByName("FieldOfAge", 1f);
            }
        }

        private void OnDestroy()
        {
            _bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _bgmInstance.release();
        }
    }
}