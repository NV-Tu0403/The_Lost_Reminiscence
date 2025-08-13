using Script.Puzzle.LightTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Code.Puzzle.LightTree
{
    public class UISupDialogue : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject panel;
        public TextMeshProUGUI questionText;
        public Button[] answerButtons; 
        private SupController current;

        private void Awake()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        public void Show(SupController npc)
        {
            current = npc;
            panel.SetActive(true);
            
            // Sử dụng localization nếu có, fallback về string thô
            if (npc.questionLocalized != null && !npc.questionLocalized.IsEmpty)
            {
                SetLocalizedText(questionText, npc.questionLocalized);
            }
            else
            {
                questionText.text = npc.question;
            }
            
            for (var i = 0; i < answerButtons.Length; i++)
            {
                var idx = i;
                answerButtons[i].gameObject.SetActive(i < GetAnswerCount(npc));
                answerButtons[i].interactable = true;
                var colors = answerButtons[i].colors;
                colors.normalColor = Color.white;
                answerButtons[i].colors = colors;
                
                var buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                
                // Sử dụng localization nếu có, fallback về string thô
                if (npc.answersLocalized != null && i < npc.answersLocalized.Length && 
                    npc.answersLocalized[i] != null && !npc.answersLocalized[i].IsEmpty)
                {
                    SetLocalizedText(buttonText, npc.answersLocalized[i]);
                }
                else if (npc.answers != null && i < npc.answers.Length)
                {
                    buttonText.text = npc.answers[i];
                }
                
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswer(idx));
            }
        }

        /// <summary>
        /// Lấy số lượng đáp án (ưu tiên localized, fallback về legacy)
        /// </summary>
        private static int GetAnswerCount(SupController npc)
        {
            if (npc.answersLocalized != null && npc.answersLocalized.Length > 0)
                return npc.answersLocalized.Length;
            
            return npc.answers?.Length ?? 0;
        }

        /// <summary>
        /// Set text với localization
        /// </summary>
        private void SetLocalizedText(TextMeshProUGUI textComponent, LocalizedString localizedString)
        {
            localizedString.StringChanged += text => {
                if (textComponent != null)
                    textComponent.text = text;
            };
            localizedString.RefreshString();
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void OnAnswer(int idx)
        {
            if (current == null) return;
            var isCorrect = idx == current.correctIndex;
            if (isCorrect)
            {
                current.OnAnswered(true);
                for (var i = 0; i < answerButtons.Length; i++)
                {
                    answerButtons[i].interactable = false;
                    var colors = answerButtons[i].colors;
                    colors.normalColor = (i == current.correctIndex) ? Color.green : Color.white;
                    answerButtons[i].colors = colors;
                }
                Invoke(nameof(Hide), 1f);
            }
            else
            {
                current.OnAnswered(false);
                answerButtons[idx].interactable = false;
                var colors = answerButtons[idx].colors;
                colors.normalColor = Color.gray;
                answerButtons[idx].colors = colors;
            }
        }

        public void Close()
        {
            panel.SetActive(false);
            current = null;
        }
    }
}