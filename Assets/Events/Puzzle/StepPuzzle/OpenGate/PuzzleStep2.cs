using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using Events.TestPuzzle;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
{
    public class PuzzleStep2 : MonoBehaviour, IPuzzleStep
    {
        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float cameraMoveDuration = 1f;
        [SerializeField] private float gateOpenDuration = 2f;

        [Header("Gate")]
        [SerializeField] private Transform gate;
        [SerializeField] private Vector3 openOffset = new Vector3(0, -5, 0);

        private Vector3 _playerCamPosition;
        private Quaternion _playerCamRotation;

        public void StartStep(Action onComplete)
        {
            if (EventCamera.Instance == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy EventCamera!");
                onComplete?.Invoke();
                return;
            }
            if (GetPosPlayerCamera(onComplete, out var playerCam)) return;
            SyncCameraWithPlayer(out var eventCam);
            PlayCameraAndGateSequence(onComplete);
        }

        private bool GetPosPlayerCamera(Action onComplete, out Camera playerCam)
        {
            // Lưu vị trí và rotation hiện tại của camera player
            playerCam = Camera.main;
            if (playerCam == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy Camera player!");
                onComplete?.Invoke();
                return true;
            }
            //_playerCamPosition = playerCam.transform.position;
            //_playerCamRotation = playerCam.transform.rotation;
            return false;
        }

        // Đặt vị trí và rotation của EventCamera trùng với camera player trước khi tween

        private void SyncCameraWithPlayer(out EventCamera eventCam)
        {
            eventCam = EventCamera.Instance;
            eventCam.transform.position = _playerCamPosition;
            eventCam.transform.rotation = _playerCamRotation;
        }

        
        private void PlayCameraAndGateSequence(Action onComplete)
        {
            var eventCam = EventCamera.Instance;
            eventCam.SwitchToEventCamera();
            SyncCameraWithPlayer(out eventCam);
            var seq = DOTween.Sequence();
            seq.AppendCallback(() => {
                eventCam.MoveTo(cameraTarget, cameraMoveDuration);
                eventCam.LookAt(gate, cameraMoveDuration); // Camera vừa di chuyển vừa xoay về cánh cổng
            });
            seq.AppendInterval(cameraMoveDuration);
            seq.AppendCallback(() => {
                Debug.Log("[PuzzleStep2] Camera đã tới vị trí cổng → bắt đầu mở cửa");
                OpenGate();
            });
            seq.AppendInterval(gateOpenDuration);
            seq.AppendCallback(() => eventCam.MoveTo(_playerCamPosition, _playerCamRotation, cameraMoveDuration));
            seq.AppendInterval(cameraMoveDuration);
            seq.OnComplete(() => {
                eventCam.SwitchToPlayerCamera();
                Debug.Log("[PuzzleStep2] Camera quay lại vị trí ban đầu sau khi mở cổng");
                onComplete?.Invoke();
            });
        }

        private void OpenGate()
        {
            if (gate == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy gate!");
                return;
            }

            Vector3 targetPos = gate.position + openOffset;
            gate.DOMove(targetPos, gateOpenDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => Debug.Log("[PuzzleStep2] Cổng đã mở xong"));
        }
    }
}
