using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Script.Puzzle.Config.Base;
using Script.Puzzle.Config.SO;
using TMPro;
using UnityEngine;

namespace Code.Puzzle.InteractBridge
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
        public bool puzzleCompleted = false;
        private Vector3[] originalPositions;
        private Action _onComplete;

        
        // Phương thức này sẽ được gọi khi bắt đầu bước puzzle, nó sẽ khởi tạo dữ liệu từ ScriptableObject.
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

        public void ForceComplete(bool instant = true)
        {
            throw new NotImplementedException();
        }

        // Phương thức này sẽ được gọi khi bắt đầu bước puzzle, nó sẽ thiết lập vị trí ban đầu của các khối cầu.
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

        // Phương thức này sẽ nâng các khối cầu lên theo thứ tự, sau đó bắt đầu đếm ngược.
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

        // Phương thức này sẽ bắt đầu đếm ngược thời gian, hiển thị trên UI.
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
            
            //if (puzzleCompleted == true) countdownCanvas.enabled = false; 
        }

        // Phương thức này sẽ được gọi khi cầu sập, nó sẽ làm cho các khối cầu rơi xuống.
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
            
            // Khi người chơi qua cầu thành công
            if (puzzleCompleted == true) _onComplete.Invoke();
        }

        // Gọi phương thức này để reset vị trí người chơi về điểm reset đã định.
        public void ResetPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();
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

        // Gọi phương thức này khi cầu sập và người chơi không qua được.
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
