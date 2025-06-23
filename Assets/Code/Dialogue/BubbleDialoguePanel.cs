using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Code.Dialogue
{
    public class BubbleDialoguePanel : MonoBehaviour
    {
        [Header("=== UI References ===")]
        public TextMeshProUGUI dialogueText;

        private Action onDialogueEnd;
        private DialogueNodeSO currentNode;
        private Coroutine typingCoroutine;
        private bool isTyping = false;
        private const float TYPEWRITER_DELAY = 0.05f;

        /// <summary>
        /// Hiển thị bubble dialogue đơn giản:
        /// - Bật panel, gán callback kết thúc.
        /// - Hiển thị text với hiệu ứng typewriter.
        /// - Tự động ẩn sau 2 giây khi gõ xong.
        /// </summary>
        public void ShowDialogue(DialogueNodeSO node, Action onEnd)
        {
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;
            currentNode = node;

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
            currentNode = node;
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterCoroutine(node));
        }

        /// <summary>
        /// Coroutine hiệu ứng typewriter cho bubble:
        /// - Lấy text từ LocalizedString và chạy hiệu ứng gõ chữ.
        /// - Sau khi gõ xong, tự động ẩn bubble sau 2 giây.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSO node)
        {
            isTyping = true;
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TYPEWRITER_DELAY);
            isTyping = false;
            yield return new WaitForSeconds(2f); // Hiện 2s sau khi gõ xong
            EndDialogue();
        }

        private void EndDialogue()
        {
            gameObject.SetActive(false);
            onDialogueEnd?.Invoke();
        }
    }
}