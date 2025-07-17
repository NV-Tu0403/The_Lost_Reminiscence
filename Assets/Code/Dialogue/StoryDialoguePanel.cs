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
        private Action onDialogueEnd;
        private DialogueNodeSO currentNode;
        private Coroutine typingCoroutine;
        private bool isTyping = false;
        private Tween blinkNextTween;
        private Tween blinkSkipTween;
        private const float TypewriterDelay = 0.05f;

        /// <summary>
        /// Show dialogue from root node, set end callback.
        /// </summary>
        public void ShowDialogue(DialogueNodeSO rootNode, Action onEnd)
        {
            EventBus.Publish("StartDialogue"); // Đảm bảo phát event này khi panel hiện lên
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;

            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipPressed);

            StopBlinking(ref blinkSkipTween);
            StartBlinking(skipButton, ref blinkSkipTween);

            ShowNode(rootNode);
        }

        /// <summary>
        /// Show a dialogue node, or end if null.
        /// </summary>
        private void ShowNode(DialogueNodeSO node)
        {
            StopBlinking(ref blinkNextTween);

            if (node == null)
            {
                EndDialogue();
                return;
            }
            currentNode = node;
            nextButton.gameObject.SetActive(false);
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterCoroutine(node));
        }

        /// <summary>
        /// Typewriter effect for dialogue text.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSO node)
        {
            isTyping = true;
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TypewriterDelay);
            isTyping = false;
            ShowNextButton(node);

            StopBlinking(ref blinkNextTween);
            StartBlinking(nextButton, ref blinkNextTween);
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
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            isTyping = false;
            EndDialogue();
        }

        /// <summary>
        /// End dialogue, cleanup.
        /// </summary>
        private void EndDialogue()
        {
            StopBlinking(ref blinkNextTween);
            nextButton.onClick.RemoveAllListeners();
            skipButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
            onDialogueEnd?.Invoke();
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