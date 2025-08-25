using System;
using System.Collections;
using _MyGame.Codes.GameEventSystem;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _MyGame.Codes.Dialogue
{
    public class BubbleDialoguePanel : MonoBehaviour
    {
        [Header("=== UI References ===")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private float timeToHide = 2f;

        [Header("=== Audio ===")] 
        [SerializeField] private AudioClip appearSfx;
        [SerializeField] private AudioSource sfxSource;
        
        private Action onDialogueEnd;
        private Coroutine typingCoroutine;
        private CanvasGroup canvasGroup;
        private const float TypewriterDelay = 0.005f;

        private bool isPersistent; // new flag
        
        public event Action TypingCompleted; // raised when typewriter completes
        public bool IsTyping { get; private set; }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// Hiển thị bubble dialogue đơn giản:
        /// - Bật panel, gán callback kết thúc.
        /// - Hiển thị text với hiệu ứng typewriter.
        /// - Tự động ẩn sau 2 giây khi gõ xong.
        /// </summary>
        public void ShowDialogue(DialogueNodeSo node, Action onEnd)
        {
            isPersistent = false;
            InternalShow(node, onEnd, false);
        }

        /// <summary>
        /// Hiển thị bubble dialogue bền vững:
        /// - Bật panel, không gán callback kết thúc.
        /// - Hiển thị text với hiệu ứng typewriter.
        /// - Không tự động ẩn, chờ gọi HideManually().
        /// </summary>
        public void ShowDialoguePersistent(DialogueNodeSo node)
        {
            isPersistent = true;
            InternalShow(node, null, true);
        }

        private void InternalShow(DialogueNodeSo node, Action onEnd, bool persistent)
        {
            EventBus.Publish("StartDialogue");
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;
            ShowAnimation();
            ShowNode(node, persistent);
        }

        /// <summary>
        /// Hiển thị một node cụ thể:
        /// - Nếu node null thì kết thúc.
        /// - Dừng hiệu ứng cũ nếu có, bắt đầu hiệu ứng mới.
        /// </summary>
        private void ShowNode(DialogueNodeSo node, bool persistent)
        {
            if (node == null)
            {
                if (!isPersistent) EndDialogue();
                return;
            }
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterCoroutine(node, persistent));
        }

        /// <summary>
        /// Coroutine hiệu ứng typewriter cho bubble:
        /// - Lấy text từ LocalizedString và chạy hiệu ứng gõ chữ.
        /// - Sau khi gõ xong, tự động ẩn bubble sau 2 giây.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSo node, bool persistent)
        {
            IsTyping = true;
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TypewriterDelay);
            IsTyping = false;
            TypingCompleted?.Invoke();
            if (!persistent)
            {
                yield return new WaitForSeconds(timeToHide);
                EndDialogue();
            }
        }

        // New manual hide for persistent
        public void HideManually()
        {
            if (!gameObject.activeSelf) return;
            if (isPersistent)
            {
                EndDialogue();
            }
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
            if (canvasGroup == null) return;

            var rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            // Khởi tạo trạng thái
            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.8f; // scale nhỏ ban đầu

            // Vị trí ban đầu: thấp hơn 30px
            var originalPos = rectTransform.anchoredPosition;
            var startPos = originalPos - new Vector2(0f, 30f);
            rectTransform.anchoredPosition = startPos;

            // Tween hiệu ứng
            canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutSine);
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            rectTransform.DOAnchorPos(originalPos, 0.3f).SetEase(Ease.OutCubic);

            // Âm thanh (nếu có)
            if (appearSfx != null && sfxSource != null)
                sfxSource.PlayOneShot(appearSfx);
        }

        
        private void HideAnimation()
        {
            if (canvasGroup == null) return;

            // Lưu vị trí hiện tại để tween anchorPos
            var rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            var startPos = rectTransform.anchoredPosition;
            var endPos = startPos + new Vector2(0f, 30f); // bay lên 30px

            // Di chuyển vị trí UI (bay lên)
            rectTransform.DOAnchorPos(endPos, 0.25f).SetEase(Ease.OutSine);

            // Fade out
            canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InSine);

            // Scale down + gọi callback
            transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onDialogueEnd?.Invoke();
                    rectTransform.anchoredPosition = startPos; // reset vị trí để dùng lại
                });
        }
    }
}