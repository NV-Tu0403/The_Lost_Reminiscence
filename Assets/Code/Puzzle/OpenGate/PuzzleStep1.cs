using System;
using DG.Tweening;
using UnityEngine;

namespace Code.Puzzle.OpenGate
{
    public class PuzzleStep1 : PuzzleStepCameraBase, IPuzzleStep
    {
        [Header("Camera")]
        [Tooltip("Vị trí vật thể camera sẽ nhìn vào")]
        [SerializeField] private Transform cameraTarget;
        
        [Range(0.1f, 10f)] 
        [Tooltip("Thời gian tween camera di chuyển đến vị trí cổng")] 
        [SerializeField] private float cameraMoveDuration = 1f;
            
        [Range(0.1f,  10f)]
        [Tooltip("Thời gian giữ camera ở vị trí cổng")]
        [SerializeField] private float cameraHoldDuration = 2f;
        
        [Header("Gate")]
        [Tooltip("Vị trí cổng")]
        [SerializeField] private Transform gate;    // Cánh cổng sẽ nhìn vào
        

        public void StartStep(Action onComplete)
        {
            if (!CheckCameraAvailable(onComplete)) return;
            
            var playerCam = GetPlayerCam(out var characterCamera);
            var sequence = MoveCameraToTarget(playerCam, cameraTarget, gate, cameraMoveDuration);
            sequence.AppendInterval(cameraHoldDuration);
            ReturnCameraToPlayer(sequence, playerCam, cameraMoveDuration, onComplete, characterCamera);
        }

        public void ForceComplete(bool instant = true)
        {
            // Nếu có logic trạng thái hoàn thành cho step này, set ở đây.
            // Ví dụ: Đặt camera/gate về trạng thái đã hoàn thành, không chạy tween.
            // Nếu không cần gì, để trống.
        }
    }
}
