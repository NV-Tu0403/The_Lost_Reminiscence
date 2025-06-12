using System;
using System.Collections.Generic;
using Events.Puzzle.Scripts;
using TMPro;
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Events.Puzzle.StepPuzzle.InteractBridge
{
    public class PuzzleStep3 : MonoBehaviour, IPuzzleStep
    {
        [Header("Bridge Pieces")] [SerializeField]
        private List<Transform> bridgePieces;

        [SerializeField] private float raiseHeight = 1.5f;
        [SerializeField] private float raiseDuration = 0.5f;
        [SerializeField] private float raiseDelay = 0.2f;

        [Header("Collapse Settings")] [SerializeField]
        private float countdownTime = 5f;

        [SerializeField] private float fallDelayBetweenPieces = 0.2f;
        [SerializeField] private float fallDistance = 5f;
        [SerializeField] private float fallDuration = 0.6f;
        [SerializeField] private Vector3 shakeStrength = new Vector3(0.2f, 0.2f, 0.2f);

        [Header("UI Countdown")] [SerializeField]
        private TextMeshProUGUI countdownText;

        [SerializeField] private Canvas countdownCanvas;

        [SerializeField] private Transform playerResetPoint;

        [Header("Cinemachine")]
        //[SerializeField] private Cinemachine.CinemachineVirtualCamera faVirtualCamera;
        //[SerializeField] private Cinemachine.CinemachineVirtualCamera playerVirtualCamera;

        private bool puzzleStarted = false;
        private Vector3[] originalPositions;
        private Action _onComplete;

        public void StartStep(Action onComplete, bool isRetry = false)
        {
            _onComplete = onComplete;
            if (!isRetry)
            {
                // TODO: Hiện dialogue/cutscene nếu cần
                // Sau khi dialogue xong, chuyển điều khiển/camera sang Fa
            }
            SwitchToFa();
            _onComplete?.Invoke();
        }

        private void SwitchToFa()
        {
            // Chuyển camera Cinemachine sang Fa
            //if (faVirtualCamera != null && playerVirtualCamera != null)
            //{
            //    faVirtualCamera.Priority = 20;
            //    playerVirtualCamera.Priority = 10;
            //}
            // Chuyển điều khiển input sang Fa
            var fa = GameObject.FindGameObjectWithTag("Fa");
            var player = GameObject.FindGameObjectWithTag("Player");
            if (fa != null && player != null)
            {
                var faController = fa.GetComponent<Loc_Backend.Scripts.CharacterController>();
                var playerController = player.GetComponent<Loc_Backend.Scripts.CharacterController>();
                if (faController != null) faController.isActive = true;
                if (playerController != null) playerController.isActive = false;
            }
            else
            {
                Debug.LogWarning("[PuzzleStep3] Không tìm thấy Fa hoặc Player để chuyển điều khiển!");
            }
        }
    }
}