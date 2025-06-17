using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
{
    public class PuzzleStep2 : PuzzleStepCameraBase, IPuzzleStep
    {
        [Header("Camera")]
        [Tooltip("Vị trí vật thể camera sẽ nhìn vào")]
        [SerializeField] private Transform cameraTarget;
        
        [Range(0.1f, 10f)]
        [Tooltip("Thời gian tween camera di chuyển đến vị trí cổng")]
        [SerializeField] private float cameraMoveDuration = 1f;
        
        [Range(0.1f, 10f)]
        [Tooltip("Thời gian giữ camera ở vị trí cổng trước khi mở cổng")]
        [SerializeField] private float gateOpenDuration = 2f;

        [Header("Gate")]
        [Tooltip("Vị trí cổng")]
        [SerializeField] private Transform gate;
        
        [Tooltip("Khoảng cách mở cổng (cổng sẽ dịch chuyển từ vị trí này đến vị trí này)")]
        [SerializeField] private Vector3 openOffset = new Vector3(0, -10, 0);
        
        [Range(0.1f, 10f)]
        [Tooltip("Thời gian tween mở cửa cổng")]
        [SerializeField] private float gateTweenDuration = 1f;
        
        [Header("Audio")]
        [Tooltip("Âm thanh mở cổng, kéo AudioSource chứa file âm thanh này vào đây")]
        [SerializeField] private AudioSource gateAudio;

        public void StartStep(Action onComplete)
        {
            if (!CheckCameraAvailable(onComplete)) return;
            
            var playerCam = GetPlayerCam(out var characterCamera);
            var seq = MoveCameraToTarget(playerCam, cameraTarget, gate, cameraMoveDuration);
            seq.AppendCallback(() => {
                Debug.Log("[PuzzleStep2] Camera đã tới vị trí cổng → bắt đầu mở cửa");
                OpenGate();
            });
            seq.AppendInterval(gateTweenDuration + gateOpenDuration);
            ReturnCameraToPlayer(seq, playerCam, cameraMoveDuration, onComplete, characterCamera);
        }

        // Mở cổng 
        private void OpenGate()
        {
            if (gate == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy gate!");
                return;
            }

            // Nếu có âm thanh mở cổng, phát nó
            if (gateAudio != null)
                gateAudio.Play();

            // Tween cổng mở
            Vector3 targetPos = gate.position + openOffset;
            gate.DOMove(targetPos, gateTweenDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => Debug.Log("[PuzzleStep2] Cổng đã mở xong"));
        }
    }
}
