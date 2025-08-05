using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.GameEventSystem; 

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

        [Header("=== Audio ===")] 
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip appearSfx;


        // Callback để thông báo về bên ngoài khi kết thúc toàn bộ dialogue
        private Action onDialogueEnd;

        // Coroutine gõ chữ
        private Coroutine typingCoroutine;
        
        // Coroutine nhấp nháy
        private Tween blinkNextTween;
        private Tween blinkSkipTween;

        // Thời gian delay giữa các ký tự (0.05s) => Lưu ý GIỮA CÁC KÝ TỰ, không phải giữa các từ.
        private const float TypewriterDelay = 0.05f;
        
        
        /// <summary>
        /// Được gọi từ DialogueManager:
        /// - node gốc
        /// - callback khi kết thúc 
        /// </summary>
        public void ShowDialogue(DialogueNodeSo rootNode, Action onEnd)
        {
            EventBus.Publish("StartDialogue"); // Đảm bảo phát event này khi panel hiện lên
            
            // Hiện chuột
            Core.Instance.IsDialoguePlaying = true;
            Core.Instance.ActiveMouseCursor(true);
            
            gameObject.SetActive(true);
            onDialogueEnd = onEnd;

            // Phát âm thanh mở hội thoại
            if (sfxSource != null && appearSfx != null)
            {
                sfxSource.PlayOneShot(appearSfx);
            }
            
            // Luôn luôn hiện nút Skip
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipPressed);

            // Bắt đầu blink cho Skip
            StopBlinking(ref blinkSkipTween);
            StartBlinking(skipButton, ref blinkSkipTween);
            
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
        private void ShowNode(DialogueNodeSo node)
        {
            StopBlinking(ref blinkNextTween);
            
            if (node == null)
            {
                EndDialogue();
                return;
            }
            
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
        private IEnumerator TypewriterCoroutine(DialogueNodeSo node)
        {
            yield return TypewriterEffect.PlayLocalized(dialogueText, node.dialogueText, TypewriterDelay);
            
            if (node.choices is { Length: > 0 }) ShowChoices(node);
            else
            {
                ShowNextButton(node);
    
                // Bắt đầu blink cho Next
                StopBlinking(ref blinkNextTween);
                StartBlinking(nextButton, ref blinkNextTween);
            }
        }

        /// <summary>
        /// Hiển thị tên nhân vật nói
        /// </summary>
        private void ShowSpeaker(DialogueNodeSo node)
        {
            nameSpeaker.text = node.speakerName.ToString();
        }

        /// <summary>
        /// Hiển thị nút Next (nếu node.nextNode != null),
        /// hoặc gọi EndDialogue khi nextNode == null.
        /// </summary>
        private void ShowNextButton(DialogueNodeSo node)
        {
            choicesPanel.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            if (node.nextNode != null) nextButton.onClick.AddListener(() => ShowNode(node.nextNode));
            else nextButton.onClick.AddListener(EndDialogue);
        }

        /// <summary>
        /// Tạo và hiển thị các nút lựa chọn.
        /// Khi người chơi ấn, sẽ gọi ShowNode với nextNode tương ứng.
        /// </summary>
        private void ShowChoices(DialogueNodeSo node)
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
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();

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
            EndDialogue();
        }

        /// <summary>
        /// Kết thúc hội thoại:
        /// - Ẩn panel
        /// - Gọi callback lên Manager
        /// </summary>
        private void EndDialogue()
        {
            // Tắt chuột
            Core.Instance.IsDialoguePlaying = false;
            Core.Instance.ActiveMouseCursor(false);
            
            // Tắt hiệu ứng nhấp nháy
            StopBlinking(ref blinkNextTween);
            
            // Bỏ tất cả listeners
            ClearChoices();
            nextButton.onClick.RemoveAllListeners();
            skipButton.onClick.RemoveAllListeners();

            // Tắt panel
            gameObject.SetActive(false);
            onDialogueEnd?.Invoke();
        }
        
        
        /// <summary>
        /// Hiệu ứng nhấp nháy nút
        /// </summary>
        private static void StartBlinking(Button button, ref Tween tweenHolder)
        {
            var image = button.GetComponent<Image>();
            if (image == null) return;

            tweenHolder?.Kill();

            tweenHolder = image.DOFade(0.3f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private static void StopBlinking(ref Tween tween)
        {
            if (tween == null) return;
            tween.Kill();
            tween = null;
        }
    }
}
