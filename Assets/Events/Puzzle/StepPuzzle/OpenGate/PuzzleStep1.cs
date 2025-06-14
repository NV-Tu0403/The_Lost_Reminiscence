using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using Events.TestPuzzle;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
{
    public class PuzzleStep1 : MonoBehaviour, IPuzzleStep
    {
        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;       // Vị trí camera khi nhìn vào cổng
        [SerializeField] private float cameraMoveDuration = 1f;
        [SerializeField] private float cameraHoldDuration = 2f;  
        
        private Vector3 _playerCamPosition;
        private Quaternion _playerCamRotation;

        public void StartStep(Action onComplete)
        {
            if (EventCamera.Instance == null)
            {
                Debug.LogError("[PuzzleStep1] Không tìm thấy EventCamera!");
                onComplete?.Invoke();
                return;
            }
            
            if (GetPosPlayerCamera(onComplete, out var playerCam)) return;
            PlayCameraSequence(onComplete);
        }

        // Lưu vị trí và rotation hiện tại của camera player
        private bool GetPosPlayerCamera(Action onComplete, out Camera playerCam)
        {
            playerCam = Camera.main;
            if (playerCam == null)
            {
                Debug.LogError("[PuzzleStep1] Không tìm thấy Camera player!");
                onComplete?.Invoke();
                return true;
            }
            _playerCamPosition = playerCam.transform.position;
            _playerCamRotation = playerCam.transform.rotation;
            return false;
        }

        private void PlayCameraSequence(Action onComplete)
        {
            var eventCam = EventCamera.Instance;
            eventCam.SwitchToEventCamera();
            var seq = DOTween.Sequence();
            seq.Append(eventCam.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            seq.AppendInterval(cameraHoldDuration);
            seq.Append(eventCam.transform.DOMove(_playerCamPosition, cameraMoveDuration));
            seq.Join(eventCam.transform.DORotateQuaternion(_playerCamRotation, cameraMoveDuration));
            seq.OnComplete(() => {
                eventCam.SwitchToPlayerCamera();
                Debug.Log("[PuzzleStep1] Camera event xong → báo progression hoàn thành bước này");
                onComplete?.Invoke();
            });
        }
    }
}