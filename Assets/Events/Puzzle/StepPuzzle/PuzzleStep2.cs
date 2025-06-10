using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle
{
    public class PuzzleStep2 : MonoBehaviour, IPuzzleStep
    {
        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Transform cameraDefault;
        [SerializeField] private float cameraMoveDuration = 1f;
        [SerializeField] private float gateOpenDuration = 2f;

        [Header("Gate")]
        [SerializeField] private Transform gate;
        [SerializeField] private Vector3 openOffset = new Vector3(0, -5, 0);

        public void StartStep(Action onComplete)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy Camera chính!");
                onComplete?.Invoke();
                return;
            }
            // Di chuyển camera tới cổng, mở cửa, rồi trả camera về vị trí cũ
            MoveCameraToDoor(onComplete, mainCamera);
        }

        private void MoveCameraToDoor(Action onComplete, Camera mainCamera)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(mainCamera.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            seq.AppendCallback(() => {
                Debug.Log("[PuzzleStep2] Camera đã tới vị trí cổng → bắt đầu mở cửa");
                OpenGate();
            });
            seq.AppendInterval(gateOpenDuration); // Đợi cửa mở
            seq.Append(mainCamera.transform.DOMove(cameraDefault.position, cameraMoveDuration));
            seq.OnComplete(() => {
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

