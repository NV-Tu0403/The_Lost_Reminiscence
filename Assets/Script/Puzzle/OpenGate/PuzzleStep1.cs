using System;
using DG.Tweening;
using Script.Puzzle;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
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
    }
}
