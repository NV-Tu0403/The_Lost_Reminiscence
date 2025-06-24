using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.Dialogue
{
    public class BubbleDialoguePanel : MonoBehaviour
    {
        [Header("=== UI References ===")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private float timeToHide = 2f;

        [Header("=== Audio ===")] 
        [SerializeField] private AudioClip appearSfx;
        [SerializeField] private AudioSource sfxSource;
        
        private Action _onDialogueEnd;
        private DialogueNodeSO _currentNode;
        private Coroutine _typingCoroutine;
        private CanvasGroup _canvasGroup;
        private bool isTyping = false;
        private const float TypewriterDelay = 0.05f;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// Hiển thị bubble dialogue đơn giản:
        /// - Bật panel, gán callback kết thúc.
        /// - Hiển thị text với hiệu ứng typewriter.
        /// - Tự động ẩn sau 2 giây khi gõ xong.
        /// </summary>
        public void ShowDialogue(DialogueNodeSO node, Action onEnd)
        {
            gameObject.SetActive(true);
            _onDialogueEnd = onEnd;
            _currentNode = node;

            ShowAnimation();
            ShowNode(node);
        }

        /// <summary>
        /// Hiển thị một node cụ thể:
        /// - Nếu node null thì kết thúc.
        /// - Dừng hiệu ứng cũ nếu có, bắt đầu hiệu ứng mới.
        /// </summary>
        private void ShowNode(DialogueNodeSO node)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }
            _currentNode = node;
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypewriterCoroutine(node));
        }

        /// <summary>
        /// Coroutine hiệu ứng typewriter cho bubble:
        /// - Lấy text từ LocalizedString và chạy hiệu ứng gõ chữ.
        /// - Sau khi gõ xong, tự động ẩn bubble sau 2 giây.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSO node)
        {
            isTyping = true;
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TypewriterDelay);
            isTyping = false;
            yield return new WaitForSeconds(timeToHide);
            EndDialogue();
        }

        private void EndDialogue()
        {
            HideAnimation();
        }

        
        /// <summary>
        /// Hiệu ứng hiển thị và ẩn bubble dialogue:
        /// - Hiển thị với hiệu ứng fade-in và scale.
        /// - Ẩn với hiệu ứng fade-out và scale.
        /// - Gọi callback khi ẩn xong.
        /// </summary>

        private void ShowAnimation()
        {
            if (_canvasGroup == null) return;

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            // Khởi tạo trạng thái
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.8f; // scale nhỏ ban đầu

            // Vị trí ban đầu: thấp hơn 30px
            Vector2 originalPos = rectTransform.anchoredPosition;
            Vector2 startPos = originalPos - new Vector2(0f, 30f);
            rectTransform.anchoredPosition = startPos;

            // Tween hiệu ứng
            _canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutSine);
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            rectTransform.DOAnchorPos(originalPos, 0.3f).SetEase(Ease.OutCubic);

            // Âm thanh (nếu có)
            if (appearSfx != null && sfxSource != null)
                sfxSource.PlayOneShot(appearSfx);
        }

        
        private void HideAnimation()
        {
            if (_canvasGroup == null) return;

            // Lưu vị trí hiện tại để tween anchorPos
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, 30f); // bay lên 30px

            // Di chuyển vị trí UI (bay lên)
            rectTransform.DOAnchorPos(endPos, 0.25f).SetEase(Ease.OutSine);

            // Fade out
            _canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InSine);

            // Scale down + gọi callback
            transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _onDialogueEnd?.Invoke();
                    rectTransform.anchoredPosition = startPos; // reset vị trí để dùng lại
                });
        }

    }
}