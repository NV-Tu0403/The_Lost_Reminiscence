using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
{
    public class PuzzleStep1 : MonoBehaviour, IPuzzleStep
    {
        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;       // Vị trí camera khi nhìn vào cổng
        [SerializeField] private Transform cameraDefault;      // Vị trí ban đầu của camera
        [SerializeField] private float cameraMoveDuration = 1f;
        [SerializeField] private float cameraHoldDuration = 2f;  
        
        public void StartStep(Action onComplete, bool isRetry = false)
        {
            if (isRetry)
            {
                // Bỏ qua hiệu ứng, chỉ báo hoàn thành step
                onComplete?.Invoke();
                return;
            }
            // Lấy camera chính
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[PuzzleStep1] Không tìm thấy Camera chính!");
                onComplete?.Invoke();
                return;
            }
            // Sử dụng DOTween để di chuyển camera 
            var seq = DOTween.Sequence();
            seq.Append(mainCamera.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            seq.AppendInterval(cameraHoldDuration);
            seq.Append(mainCamera.transform.DOMove(cameraDefault.position, cameraMoveDuration));
            seq.OnComplete(() => {
                Debug.Log("[PuzzleStep1] Camera xong → báo progression hoàn thành bước này");
                onComplete?.Invoke();
            });
        }
    }
}