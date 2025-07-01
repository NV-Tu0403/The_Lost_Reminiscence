using UnityEngine;
using Code.GameEventSystem;
using DuckLe;
using UnityEngine.InputSystem;

namespace Code.Character
{
    public class InputLockManager : MonoBehaviour
    {
        private PlayerController _playerController;
        private PlayerAnimator _playerAnimator;
        private CharacterInput _playerInput;

        private void Awake()
        {
            if (_playerController == null)
                _playerController = GetComponentInParent<PlayerController>();

            if (_playerAnimator == null)
                _playerAnimator = GetComponentInParent<PlayerAnimator>();
            
            if (_playerInput == null)
                _playerInput = GetComponentInParent<CharacterInput>();
            
            EventBus.Subscribe("StartDialogue", OnLockInput);
            EventBus.Subscribe("EndDialogue", OnUnlockInput);
            EventBus.Subscribe("StartCutscene", OnLockInput);
            EventBus.Subscribe("EndCutscene", OnUnlockInput);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe("StartDialogue", OnLockInput);
            EventBus.Unsubscribe("EndDialogue", OnUnlockInput);
            EventBus.Unsubscribe("StartCutscene", OnLockInput);
            EventBus.Unsubscribe("EndCutscene", OnUnlockInput);
        }

        private void OnLockInput(object data)
        {
            if (_playerController != null)
                _playerController.enabled = false;
            
            if (_playerAnimator != null)
                _playerAnimator.enabled = false;
            
            if (_playerInput != null)
                _playerInput.enabled = false;
            Debug.Log("[InputLockManager] Input & Controller locked due to event: " + data);
        }

        private void OnUnlockInput(object data)
        {
            if (_playerController != null)
                _playerController.enabled = true;
            
            if (_playerAnimator != null)
                _playerAnimator.enabled = true;
            
            if (_playerInput != null)
                _playerInput.enabled = true;
            Debug.Log("[InputLockManager] Input & Controller unlocked due to event: " + data);
        }
    }
}
