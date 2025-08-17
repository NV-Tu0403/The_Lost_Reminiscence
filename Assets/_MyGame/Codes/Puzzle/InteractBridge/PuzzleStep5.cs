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
        
        
        private PuzzleConfig _puzzleConfig;
        
        
        private bool puzzleStarted;
        public bool puzzleCompleted;
        private Vector3[] originalPositions;
        public Action _onComplete;

        
        // Phương thức này sẽ được gọi khi bắt đầu bước puzzle, nó sẽ khởi tạo dữ liệu từ ScriptableObject.
        public void StartStep(Action onComplete)
        {
            _puzzleConfig = puzzleConfig.ToRunTimeData();
            _onComplete = onComplete;
            if (puzzleStarted) return;
            puzzleStarted = true;
            StartCoroutine(RaiseBridgeSequence());
        }

        // Phương thức này sẽ được gọi khi bắt đầu bước puzzle, nó sẽ thiết lập vị trí ban đầu của các khối cầu.
        private void Start()
        {
            originalPositions = new Vector3[bridgePieces.Count];
            for (var i = 0; i < bridgePieces.Count; i++)
            {
                originalPositions[i] = bridgePieces[i].position;
                bridgePieces[i].position -= Vector3.up * puzzleConfig.raiseHeight;
            }
            countdownCanvas.enabled = false;
        }

        // Phương thức này sẽ nâng các khối cầu lên theo thứ tự, sau đó bắt đầu đếm ngược.
        private IEnumerator RaiseBridgeSequence()
        {
            foreach (var piece in bridgePieces)
            {
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
            var timeLeft = puzzleConfig.countdownTime;
            while (timeLeft > 0)
            {
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }
            countdownText.text = "";
            countdownCanvas.enabled = false;
        }

        // Phương thức này sẽ được gọi khi cầu sập, nó sẽ làm cho các khối cầu rơi xuống.
        private IEnumerator CollapseBridgeSequence()
        {
            foreach (var piece in bridgePieces)
            {
                piece.DOShakePosition(0.3f, puzzleConfig.shakeStrength);
                yield return new WaitForSeconds(0.2f);
                piece.DOMoveY(piece.position.y - puzzleConfig.fallDistance, puzzleConfig.fallDuration)
                    .SetEase(Ease.InBack);
                yield return new WaitForSeconds(puzzleConfig.fallDelayBetweenPieces);
            }
            yield return new WaitForSeconds(1f);
            
            // Khi người chơi qua cầu thành công
            //if (puzzleCompleted == true) _onComplete.Invoke();
        }
        
        // Phương thức này sẽ được gọi để buộc hoàn thành bước puzzle, nếu cần thiết.
        public void ForceComplete(bool instant = true) {}
    }
}
