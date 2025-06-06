using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Functions.Dialogue.Scripts
{
    /// <summary>
    /// DialoguePanel chỉ chuyên trách phần UI:
    /// - Hiển thị avatar, tên, text.
    /// - Hiệu ứng gõ chữ Typewriter.
    /// - Hiển thị nút Next / các lựa chọn (prefab).
    /// - Skip toàn bộ hội thoại.
    /// Khi có tương tác (Next / Choice / Skip), panel sẽ gọi các callback nội bộ để logic xử lý (ví dụ ShowNode hoặc EndDialogue).
    /// </summary>
    public class DialoguePanel : MonoBehaviour
    {
        [Header("=== UI References ===")]
        public Image leftAvatar;
        public Image rightAvatar;
        public TextMeshProUGUI leftName;
        public TextMeshProUGUI rightName;
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

            // Ẩn nút Skip ban đầu, sẽ hiện khi bắt đầu typewriter
            skipButton.gameObject.SetActive(false);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipPressed);

            // Bắt đầu hiển thị node đầu tiên
            ShowNode(rootNode);
        }

        /// <summary>
        /// Hiển thị một node cụ thể.
        /// Tách riêng phần logic lấy text từ LocalizedString và phần UI show (ShowSpeaker, StartTypewriter, ShowOptions/Next).
        /// </summary>
        private void ShowNode(DialogueNodeSO node)
        {
            currentNode = node;

            // Hiển thị avatar và tên (UI)
            ShowSpeaker(node);

            // Xóa mọi lựa chọn cũ
            ClearChoices();

            // Ẩn các nút Next trước khi text gõ xong
            nextButton.gameObject.SetActive(false);

            // Nếu đang còn coroutine gõ chữ cũ, dừng luôn
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            // Bắt đầu typewriter
            typingCoroutine = StartCoroutine(TypewriterCoroutine(node));
        }

        /// <summary>
        /// Coroutine gõ chữ: 
        /// - Lấy text thực sự từ LocalizedString (đợi cho LocalizedString trả về).
        /// - Gõ từng ký tự với delay 0.2s.
        /// - Sau khi gõ xong, show Next hoặc các lựa chọn.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueNodeSO node)
        {
            // Lấy text gốc từ LocalizedString
            string fullText = "";
            bool receivedText = false;

            node.dialogueText.StringChanged += (localizedText) =>
            {
                fullText = localizedText;
                receivedText = true;
            };
            node.dialogueText.RefreshString();

            // Chờ LocalizedString load xong
            while (!receivedText)
                yield return null;

            // Đặt text ban đầu trống, bật isTyping và show Skip button
            dialogueText.text = "";
            isTyping = true;
            skipButton.gameObject.SetActive(true);

            // Thực hiện gõ ký tự
            for (int i = 0; i < fullText.Length; i++)
            {
                if (!isTyping)
                    break;

                dialogueText.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(TYPEWRITER_DELAY);
            }

            // Nếu bị skip giữa chừng, ngay lập tức show đủ fullText
            dialogueText.text = fullText;
            isTyping = false;

            // Khi gõ xong, ẩn nút Skip
            skipButton.gameObject.SetActive(false);

            // Hiển thị Next hoặc Choices
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
            if (node.isLeftSpeaker)
            {
                leftAvatar.gameObject.SetActive(true);
                leftAvatar.sprite = node.speakerAvatar;
                leftName.text = node.speakerName.ToString();

                rightAvatar.gameObject.SetActive(false);
                rightName.text = "";
            }
            else
            {
                rightAvatar.gameObject.SetActive(true);
                rightAvatar.sprite = node.speakerAvatar;
                rightName.text = node.speakerName.ToString();

                leftAvatar.gameObject.SetActive(false);
                leftName.text = "";
            }
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
