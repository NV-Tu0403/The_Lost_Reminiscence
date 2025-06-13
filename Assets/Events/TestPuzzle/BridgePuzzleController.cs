using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Events.TestPuzzle
{
    public class BridgePuzzleController : MonoBehaviour
    {
        [Header("Bridge Pieces")]
        [SerializeField] private List<Transform> bridgePieces;
        [SerializeField] private float raiseHeight = 1.5f;
        [SerializeField] private float raiseDuration = 0.5f;
        [SerializeField] private float raiseDelay = 0.2f;

        [Header("Collapse Settings")]
        [SerializeField] private float countdownTime = 5f;
        [SerializeField] private float fallDelayBetweenPieces = 0.2f;
        [SerializeField] private float fallDistance = 5f;
        [SerializeField] private float fallDuration = 0.6f;
        [SerializeField] private Vector3 shakeStrength = new Vector3(0.2f, 0.2f, 0.2f);

        [Header("UI Countdown")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Canvas countdownCanvas;
            
        [SerializeField] private Transform playerResetPoint;

        private bool puzzleStarted = false;
        private Vector3[] originalPositions;

        public void StartPuzzle()
        {
            if (!puzzleStarted)
            {
                puzzleStarted = true;
                StartCoroutine(RaiseBridgeSequence());
            }
        }
        
        private void Start()
        {
            // Ghi nhớ vị trí ban đầu của từng khối
            originalPositions = new Vector3[bridgePieces.Count];
            for (int i = 0; i < bridgePieces.Count; i++)
            {
                originalPositions[i] = bridgePieces[i].position;
                bridgePieces[i].position -= Vector3.up * raiseHeight; // Ẩn xuống
            }

            countdownCanvas.enabled = false;
        }

        private IEnumerator RaiseBridgeSequence()
        {
            for (int i = 0; i < bridgePieces.Count; i++)
            {
                var piece = bridgePieces[i];
                piece.DOMoveY(piece.position.y + raiseHeight, raiseDuration).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(raiseDelay);
            }

            yield return StartCoroutine(StartCountdown());
            yield return StartCoroutine(CollapseBridgeSequence());
        }

        private IEnumerator StartCountdown()
        {
            countdownCanvas.enabled = true;
            float timeLeft = countdownTime;

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
                piece.DOShakePosition(0.3f, shakeStrength);
                yield return new WaitForSeconds(0.2f);

                piece.DOMoveY(piece.position.y - fallDistance, fallDuration).SetEase(Ease.InBack);
                yield return new WaitForSeconds(fallDelayBetweenPieces);
            }
            
            yield return new WaitForSeconds(1f); // đợi cầu sụp xong

            // Thủ công kiểm tra vị trí player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.transform.position.y < transform.position.y - 1f) // player rơi
            {
                ResetPlayer();
                StartCoroutine(ResetPuzzleAfterFail());
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
            yield return new WaitForSeconds(1f); // đợi chút cho hiệu ứng rớt xong

            for (int i = 0; i < bridgePieces.Count; i++)
            {
                bridgePieces[i].position = originalPositions[i] - Vector3.up * raiseHeight;
            }

            puzzleStarted = false;
            countdownText.text = "";
            countdownCanvas.enabled = false;
        }
    }
}
