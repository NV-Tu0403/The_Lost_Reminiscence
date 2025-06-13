using System;
using System.Collections;
using System.Collections.Generic;
using Events.Puzzle.Scripts;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Events.Puzzle.Config.Base;
using Events.Puzzle.SO;

namespace Events.Puzzle.StepPuzzle.InteractBridge
{
    // Step 5: Dựng cầu, đếm ngược, player qua cầu
    public class PuzzleStep5 : MonoBehaviour, IPuzzleStep
    {
        [Header("Config Scriptable Object")]
        [SerializeField] private PuzzleConfigSO puzzleConfig;
        
        [Header("Bridge Pieces")] 
        [Tooltip("Danh sách các khối của cầu (bridge piece) sẽ được nâng lên hoặc thả xuống.")]
        public List<Transform> bridgePieces;
        
        [Header("UI Countdown")] 
        [Tooltip("Text đếm ngược hiển thị trên màn hình.")]
        public TextMeshProUGUI countdownText;

        [Tooltip("Canvas chứa text đếm ngược.")]
        public Canvas countdownCanvas;

        [Tooltip("Vị trí reset của người chơi sau khi cầu sập.")]
        public Transform playerResetPoint;
        
        
        private PuzzleConfig _puzzleConfig;
        
        
        private bool puzzleStarted = false;
        private Vector3[] originalPositions;
        private Action _onComplete;

        public void StartStep(Action onComplete)
        {
            _puzzleConfig = puzzleConfig.ToRunTimeData();
            
            _onComplete = onComplete;
            if (!puzzleStarted)
            {
                puzzleStarted = true;
                StartCoroutine(RaiseBridgeSequence());
            }
        }

        private void Start()
        {
            originalPositions = new Vector3[bridgePieces.Count];
            for (int i = 0; i < bridgePieces.Count; i++)
            {
                originalPositions[i] = bridgePieces[i].position;
                bridgePieces[i].position -= Vector3.up * puzzleConfig.raiseHeight;
            }
            countdownCanvas.enabled = false;
        }

        private IEnumerator RaiseBridgeSequence()
        {
            for (int i = 0; i < bridgePieces.Count; i++)
            {
                var piece = bridgePieces[i];
                piece.DOMoveY(piece.position.y + puzzleConfig.raiseHeight, puzzleConfig.raiseDuration).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(puzzleConfig.raiseDelay);
            }
            yield return StartCoroutine(StartCountdown());
            yield return StartCoroutine(CollapseBridgeSequence());
        }

        private IEnumerator StartCountdown()
        {
            countdownCanvas.enabled = true;
            float timeLeft = puzzleConfig.countdownTime;
            while (timeLeft > 0)
            {
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }
            countdownText.text = "";
            countdownCanvas.enabled = false;
        }

        private IEnumerator CollapseBridgeSequence()
        {
            for (int i = 0; i < bridgePieces.Count; i++)
            {
                var piece = bridgePieces[i];
                piece.DOShakePosition(0.3f, puzzleConfig.shakeStrength);
                yield return new WaitForSeconds(0.2f);
                piece.DOMoveY(piece.position.y - puzzleConfig.fallDistance, puzzleConfig.fallDuration)
                    .SetEase(Ease.InBack);
                yield return new WaitForSeconds(puzzleConfig.fallDelayBetweenPieces);
            }
            yield return new WaitForSeconds(1f);
            // Kiểm tra player đã qua cầu thành công hay chưa
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.transform.position.y < transform.position.y - 1f)
            {
                ResetPlayer();
                StartCoroutine(ResetPuzzleAfterFail());
            }
            else
            {
                // Qua cầu thành công, báo hoàn thành event
                _onComplete?.Invoke();
            }
        }

        public void ResetPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var controller = player.GetComponent<UnityEngine.CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }
                player.transform.position = playerResetPoint.position;
                if (controller != null)
                {
                    controller.enabled = true;
                }
                Debug.Log("Player position reset to: " + playerResetPoint.position);
            }
        }

        public IEnumerator ResetPuzzleAfterFail()
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < bridgePieces.Count; i++)
            {
                bridgePieces[i].position = 
                    originalPositions[i] - Vector3.up * puzzleConfig.raiseHeight;
            }
            puzzleStarted = false;
            countdownText.text = "";
            countdownCanvas.enabled = false;
        }
    }
}
