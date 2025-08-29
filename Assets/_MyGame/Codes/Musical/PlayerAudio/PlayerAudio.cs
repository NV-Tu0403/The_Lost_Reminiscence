using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace _MyGame.Codes.Musical.PlayerAudio
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private EventReference playerFootstep; 
        [SerializeField] private PlayerController_02 playerController;

        private EventInstance _footstepInstance;
        private bool _isFootstepPlaying;

        private void Start()
        {
            if (!playerController) playerController = GetComponent<PlayerController_02>();
            if (!playerController)
            {
                Debug.LogWarning("PlayerController_02 không được gán!");
                return;
            }

            if (playerFootstep.IsNull)
            {
                Debug.LogWarning("Player Footstep Event chưa được gán!");
                return;
            }

            _footstepInstance = RuntimeManager.CreateInstance(playerFootstep);
            _isFootstepPlaying = false;
        }

        private void Update()
        {
            if (playerFootstep.IsNull || !playerController) return;

            // Luôn cập nhật vị trí 3DAttributes
            if (_footstepInstance.isValid())
                _footstepInstance.set3DAttributes(transform.position.To3DAttributes());

            // Điều kiện dừng (Idle hoặc trên không)
            if (!playerController.isGrounded || playerController.CurrentPlayerState == CharacterStateType.Idle)
            {
                if (_isFootstepPlaying)
                {
                    _footstepInstance.stop(STOP_MODE.ALLOWFADEOUT);
                    _isFootstepPlaying = false;
                }
                return;
            }

            // Nếu Walk hoặc Sprint → đảm bảo đang play + set parameter
            if (playerController.CurrentPlayerState == CharacterStateType.Walk)
            {
                SetFootstepParameter(0f); // 0 = Walk
            }
            else if (playerController.CurrentPlayerState == CharacterStateType.Sprint)
            {
                SetFootstepParameter(1f); // 1 = Sprint
            }
        }

        private void SetFootstepParameter(float value)
        {
            if (!_isFootstepPlaying)
            {
                _footstepInstance.start();
                _isFootstepPlaying = true;
            }
            _footstepInstance.setParameterByName("Footstep", value);
        }

        private void OnDestroy()
        {
            if (_footstepInstance.isValid())
            {
                _footstepInstance.stop(STOP_MODE.IMMEDIATE);
                _footstepInstance.release();
            }
        }
    }
}
