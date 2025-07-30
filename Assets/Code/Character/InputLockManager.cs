using UnityEngine;
using Code.GameEventSystem;
using Duckle;
using DuckLe;

namespace Code.Character
{
    public class InputLockManager : MonoBehaviour
    {
        private PlayerController _playerController;
        private CharacterInput _playerInput;

        private void Awake()
        {
            if (_playerController == null)
                _playerController = GetComponentInParent<PlayerController>();
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
            if (_playerController != null && _playerController._stateMachine != null)
            {
                //_playerController._stateMachine.SetPrimaryState(new IdleState());
            }
            
            if (_playerInput != null) _playerInput.isInputLocked = true;
        }

        private void OnUnlockInput(object data)
        {
            if (_playerInput != null) _playerInput.isInputLocked = false;
        }
    }
}
