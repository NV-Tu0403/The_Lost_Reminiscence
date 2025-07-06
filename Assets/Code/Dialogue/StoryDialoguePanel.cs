using System;
using System.Collections;
using Code.GameEventSystem;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Dialogue
{
    public class StoryDialoguePanel : MonoBehaviour
    {
        [Header("=== UI References ===")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;

        // Callback for when dialogue ends
        private Action _onDialogueEnd;
        private DialogueNodeSO _currentNode;
        private Coroutine _typingCoroutine;
        private bool _isTyping = false;
        private Tween _blinkNextTween;
        private Tween _blinkSkipTween;
        private const float TypewriterDelay = 0.05f;

        /// <summary>
        /// Show dialogue from root node, set end callback.
        /// </summary>
        public void ShowDialogue(DialogueNodeSO rootNode, Action onEnd)
        {
            EventBus.Publish("StartDialogue"); // Đảm bảo phát event này khi panel hiện lên
            gameObject.SetActive(true);
            _onDialogueEnd = onEnd;

            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipPressed);

            StopBlinking(ref _blinkSkipTween);
            StartBlinking(skipButton, ref _blinkSkipTween);

            ShowNode(rootNode);
        }

        /// <summary>
        /// Show a dialogue node, or end if null.
        /// </summary>
        private void ShowNode(DialogueNodeSO node)
        {
            StopBlinking(ref _blinkNextTween);

            if (node == null)
            {
                EndDialogue();
                return;
            }
            _currentNode = node;
            nextButton.gameObject.SetActive(false);
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypewriterCoroutine(node));
        }

        /// <summary>
        /// Typewriter effect for dialogue text.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSO node)
        {
            _isTyping = true;
            Debug.Log("Typing dialogue: " + _isTyping);
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TypewriterDelay);
            _isTyping = false;
            Debug.Log("Finished typing: " + _isTyping);
            ShowNextButton(node);

            StopBlinking(ref _blinkNextTween);
            StartBlinking(nextButton, ref _blinkNextTween);
        }

        /// <summary>
        /// Show next button, bind click to next node or end.
        /// </summary>
        private void ShowNextButton(DialogueNodeSO node)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            if (node.nextNode != null)
            {
                nextButton.onClick.AddListener(() => ShowNode(node.nextNode));
            }
            else
            {
                nextButton.onClick.AddListener(EndDialogue);
            }
        }

        /// <summary>
        /// Skip all dialogue.
        /// </summary>
        private void OnSkipPressed()
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _isTyping = false;
            EndDialogue();
        }

        /// <summary>
        /// End dialogue, cleanup.
        /// </summary>
        private void EndDialogue()
        {
            StopBlinking(ref _blinkNextTween);
            nextButton.onClick.RemoveAllListeners();
            skipButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
            _onDialogueEnd?.Invoke();
        }

        /// <summary>
        /// Button blinking effect.
        /// </summary>
        private void StartBlinking(Button button, ref Tween tweenHolder)
        {
            var image = button.GetComponent<Image>();
            if (image == null) return;

            tweenHolder?.Kill();

            tweenHolder = image.DOFade(0.3f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void StopBlinking(ref Tween tween)
        {
            if (tween != null)
            {
                tween.Kill();
                tween = null;
            }
        }
    }
}