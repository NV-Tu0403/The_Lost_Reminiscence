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
        [SerializeField] private Transform gate; // Thêm tham chiếu tới cánh cổng
        
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
            SyncCameraWithPlayer(out var eventCam);
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
        
        private void SyncCameraWithPlayer(out EventCamera eventCam)
        {
            eventCam = EventCamera.Instance;
            eventCam.transform.position = _playerCamPosition;
            eventCam.transform.rotation = _playerCamRotation;
        }
        
        private void PlayCameraSequence(Action onComplete)
        {
            var eventCam = EventCamera.Instance;
            eventCam.SwitchToEventCamera();
            SyncCameraWithPlayer(out eventCam);
            var seq = DOTween.Sequence();
            seq.AppendCallback(() => {
                eventCam.MoveTo(cameraTarget, cameraMoveDuration);
                if (gate != null)
                    eventCam.LookAt(gate, cameraMoveDuration);
                else
                    eventCam.LookAt(cameraTarget, cameraMoveDuration);
            });
            seq.AppendInterval(cameraMoveDuration);
            seq.AppendInterval(cameraHoldDuration);
            seq.AppendCallback(() => eventCam.MoveTo(_playerCamPosition, _playerCamRotation, cameraMoveDuration));
            seq.AppendInterval(cameraMoveDuration);
            seq.OnComplete(() => {
                eventCam.SwitchToPlayerCamera();
                Debug.Log("[PuzzleStep1] Camera event xong → báo progression hoàn thành bước này");
                onComplete?.Invoke();
            });
        }
    }
}