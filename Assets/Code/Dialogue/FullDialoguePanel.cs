using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Dialogue
{
    /// <summary>
    /// DialoguePanel chỉ chuyên trách phần UI:
    /// - Hiển thị tên, text.
    /// - Hiệu ứng gõ chữ Typewriter.
    /// - Hiển thị nút Next / các lựa chọn (prefab).
    /// - Skip toàn bộ hội thoại.
    /// Khi có tương tác (Next / Choice / Skip), panel sẽ gọi các callback nội bộ để logic xử lý (ví dụ ShowNode hoặc EndDialogue).
    /// </summary>
    public class FullDialoguePanel : MonoBehaviour
    {
        [Header("=== UI References ===")]
        public TextMeshProUGUI nameSpeaker;
        public TextMeshProUGUI dialogueText;

        [Header("=== Buttons & Prefabs ===")]
        public Button nextButton;
        public Button skipButton;
        public Transform choicesPanel;
        public Button choiceButtonPrefab;

        // Callback để thông báo về bên ngoài khi kết thúc toàn bộ dialogue
        private Action onDialogueEnd;

        // Tham chiếu node đang hiển thị
        private DialogueNodeSO currentNode;

        // Coroutine gõ chữ
        private Coroutine typingCoroutine;
        private bool isTyping = false;

        // Thời gian delay giữa các ký tự (0.05s) => Lưu ý GIỮA CÁC KÝ TỰ, không phải giữa các từ.
        private const float TYPEWRITER_DELAY = 0.05f;
        
        
        /// <summary>
        /// Được gọi từ DialogueManager:
        /// - node gốc
        /// - callback khi kết thúc (truyền tiếp sang Manager hoặc EventExecutor)
        /// </summary>
        public void ShowDialogue(DialogueNodeSO rootNode, Action onEnd)
        {
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;

            // Luôn luôn hiện nút Skip
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipPressed);

            // Bắt đầu hiển thị node đầu tiên
            ShowNode(rootNode);
        }

        /// <summary>
        /// Hiển thị một node cụ thể:
        /// - Nếu node null thì kết thúc.
        /// - Hiển thị tên nhân vật.
        /// - Xóa lựa chọn cũ, ẩn Next, dừng typewriter cũ.
        /// - Bắt đầu hiệu ứng typewriter cho node mới.
        /// </summary>
        private void ShowNode(DialogueNodeSO node)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }
            currentNode = node;
            ShowSpeaker(node);
            ClearChoices();
            nextButton.gameObject.SetActive(false);
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterCoroutine(node));
        }

        /// <summary>
        /// Coroutine gõ chữ: 
        /// - Lấy text thực sự từ LocalizedString (đợi cho LocalizedString trả về).
        /// - Gõ từng ký tự với delay 0.5s.
        /// - Sau khi gõ xong, show Next hoặc các lựa chọn.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSO node)
        {
            isTyping = true;
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TYPEWRITER_DELAY);
            isTyping = false;
            if (node.choices != null && node.choices.Length > 0)
                ShowChoices(node);
            else
                ShowNextButton(node);
        }

        /// <summary>
        /// Hiển thị avatar, tên trái/phải dựa vào node.isLeftSpeaker
        /// </summary>
        private void ShowSpeaker(DialogueNodeSO node)
        {
            nameSpeaker.text = node.speakerName.ToString();
        }

        /// <summary>
        /// Hiển thị nút Next (nếu node.nextNode != null),
        /// hoặc gọi EndDialogue khi nextNode == null.
        /// </summary>
        private void ShowNextButton(DialogueNodeSO node)
        {
            choicesPanel.gameObject.SetActive(false);
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
        /// Tạo và hiển thị các nút lựa chọn.
        /// Khi người chơi ấn, sẽ gọi ShowNode với nextNode tương ứng.
        /// </summary>
        private void ShowChoices(DialogueNodeSO node)
        {
            choicesPanel.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);

            foreach (var choiceData in node.choices)
            {
                CreateChoiceButton(choiceData);
            }
        }

        /// <summary>
        /// Instantiate một nút lựa chọn, bind text từ LocalizedString,
        /// và gán sự kiện OnClick để chuyển sang node kế tiếp.
        /// </summary>
        private void CreateChoiceButton(DialogueChoiceData choice)
        {
            var btn = Instantiate(choiceButtonPrefab, choicesPanel);
            btn.onClick.RemoveAllListeners();

            // Lấy component TextMeshProUGUI in children để hiển thị text
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();

            // Đăng ký LocalizedString để cập nhật text
            choice.choiceText.StringChanged += localizedText =>
            {
                if (txt != null)
                    txt.text = localizedText;
            };
            choice.choiceText.RefreshString();

            // Khi bấm vào, chuyển node
            btn.onClick.AddListener(() => OnChoiceSelected(choice));
        }

        /// <summary>
        /// Xóa tất cả nút lựa chọn cũ và remove listener.
        /// </summary>
        private void ClearChoices()
        {
            foreach (Transform child in choicesPanel)
            {
                var btn = child.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.RemoveAllListeners();

                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Khi chọn một lựa chọn, chuyển sang node kế tiếp.
        /// </summary>
        private void OnChoiceSelected(DialogueChoiceData choice)
        {
            ShowNode(choice.nextNode);
        }

        /// <summary>
        /// Skip toàn bộ hội thoại:
        /// - Dừng coroutine typewriter (nếu đang chạy).
        /// - Gọi EndDialogue ngay lập tức.
        /// </summary>
        private void OnSkipPressed()
        {
            // Nếu đang gõ chữ, dừng và show full text (tránh crash)
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            isTyping = false;
            EndDialogue();
        }

        /// <summary>
        /// Kết thúc hội thoại:
        /// - Ẩn panel
        /// - Gọi callback lên Manager
        /// </summary>
        private void EndDialogue()
        {
            ClearChoices();
            nextButton.onClick.RemoveAllListeners();
            skipButton.onClick.RemoveAllListeners();

            gameObject.SetActive(false);
            onDialogueEnd?.Invoke();
        }
    }
}
