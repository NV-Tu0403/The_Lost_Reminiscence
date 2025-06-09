using System;
using DG.Tweening;
using UnityEngine;

namespace Events.Puzzle.TestPuzzle
{
    public class PuzzleTest : MonoBehaviour
    {
        [Header("Camera")]
        public Transform cameraTarget;       // Vị trí camera khi nhìn vào cổng
        public Transform cameraDefault;      // Vị trí ban đầu của camera
        public float cameraMoveDuration = 1f;
        public float cameraHoldDuration = 2f;

        [Header("Gate")]
        public Transform gate;
        public Vector3 openOffset = new Vector3(0, -5, 0); // Offset cửa trượt xuống
        public float gateOpenDuration = 2f;

        private bool rockTriggered = false;
        private bool fragmentInserted = false;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        // Gọi khi đá rơi vào trigger
        public void TriggerRock()
        {
            if (rockTriggered) return;
            rockTriggered = true;

            Debug.Log("[Puzzle] Rock triggered → Camera chiếu vào cổng");

            FocusCamera(
                () => { Debug.Log("[Puzzle] Camera xong → chờ nhặt mảnh vỡ"); }
            );
        }

        // Gọi khi nhặt mảnh vỡ
        public void InsertFragment()
        {
            if (!rockTriggered || fragmentInserted) return;
            fragmentInserted = true;

            Debug.Log("[Puzzle] Fragment inserted → Camera chiếu lại và mở cổng");

            FocusCameraUntil(
                () =>
                {
                    Debug.Log("[Puzzle] Camera đã tới vị trí cổng → bắt đầu mở cửa");
                    OpenGate(() =>
                    {
                        Debug.Log("[Puzzle] Cổng đã mở xong");
                    });
                },
                () =>
                {
                    Debug.Log("[Puzzle] Camera quay lại vị trí ban đầu sau khi mở cổng");
                }
            );
        }

        // Dùng khi chỉ cần focus ngắn (trigger đá)
        private void FocusCamera(Action onComplete)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(mainCamera.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            seq.AppendInterval(cameraHoldDuration);
            seq.Append(mainCamera.transform.DOMove(cameraDefault.position, cameraMoveDuration));
            seq.OnComplete(() => onComplete?.Invoke());
        }

        // Dùng khi cần giữ camera cho đến khi xử lý xong (như mở cổng)
        private void FocusCameraUntil(Action onReachedTarget, Action onBackComplete)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(mainCamera.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            seq.AppendCallback(() =>
            {
                Debug.Log("[Puzzle] Camera đã tới vị trí → gọi onReachedTarget");
                onReachedTarget?.Invoke();
            });
            seq.AppendInterval(gateOpenDuration); // Đợi cửa mở
            seq.Append(mainCamera.transform.DOMove(cameraDefault.position, cameraMoveDuration));
            seq.OnComplete(() => onBackComplete?.Invoke());
        }

        private void OpenGate(Action onComplete)
        {
            Vector3 targetPos = gate.position + openOffset;
            gate.DOMove(targetPos, gateOpenDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
