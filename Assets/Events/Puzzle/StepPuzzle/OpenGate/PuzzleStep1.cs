using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
{
    public class PuzzleStep1 : MonoBehaviour, IPuzzleStep
    {
        [Header("Camera")]
        [Tooltip("Vị trí vật thể camera sẽ nhìn vào")]
        [SerializeField] private Transform cameraTarget;
        
        [Range(0.1f, 10f)] 
        [Tooltip("Thời gian tween camera di chuyển đến vị trí cổng")] 
        [SerializeField] private float cameraMoveDuration = 1f;
            
        [Range(0.1f,  10f)]
        [Tooltip("Thời gian giữ camera ở vị trí cổng trước khi mở cổng")]
        [SerializeField] private float cameraHoldDuration = 2f;
        
        [Header("Gate")]
        [Tooltip("Vị trí cổng")]
        [SerializeField] private Transform gate;    // Cánh cổng sẽ nhìn vào

        private Vector3 _playerCamPosition;
        private Quaternion _playerCamRotation;

        public void StartStep(Action onComplete)
        {
            if (Camera.main == null)
            {
                Debug.LogError("[PuzzleStep1] Không tìm thấy Camera.main!");
                onComplete?.Invoke();
                return;
            }
            var playerCam = GetPlayerCam(out var characterCamera);
            var sequence = MoveCameraToDoor(playerCam);
            ReturnToPlayer(onComplete, sequence, playerCam, characterCamera);
        }

        private void ReturnToPlayer(Action onComplete, Sequence sequence, Camera playerCam, CharacterCamera characterCamera)
        {
            // Tween camera về lại vị trí/góc quay ban đầu
            sequence.Append(playerCam.transform.DOMove(_playerCamPosition, cameraMoveDuration));
            sequence.Join(playerCam.transform.DORotateQuaternion(_playerCamRotation, cameraMoveDuration));
            sequence.OnComplete(() =>
            {
                if (characterCamera != null)
                    characterCamera.enabled = true;
                Debug.Log("[PuzzleStep1] Camera event xong → báo progression hoàn thành bước này");
                onComplete?.Invoke();
            });
        }

        private Sequence MoveCameraToDoor(Camera playerCam)
        {
            // Tween camera tới vị trí/góc quay của cameraTarget
            var sequence = DOTween.Sequence();
            var lookRotation = Quaternion.LookRotation(gate.position - cameraTarget.position);
            sequence.Append(playerCam.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            sequence.Join(playerCam.transform.DORotateQuaternion(lookRotation, cameraMoveDuration));
            sequence.AppendInterval(cameraHoldDuration);
            return sequence;
        }

        private Camera GetPlayerCam(out CharacterCamera characterCamera)
        {
            var playerCam = Camera.main;
            characterCamera = playerCam.GetComponent<CharacterCamera>();
            if (characterCamera != null)
                characterCamera.enabled = false;

            _playerCamPosition = playerCam.transform.position;
            _playerCamRotation = playerCam.transform.rotation;
            return playerCam;
        }
    }
}
