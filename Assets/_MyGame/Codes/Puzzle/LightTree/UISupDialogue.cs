using Script.Puzzle.LightTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            questionText.text = npc.question;
            for (var i = 0; i < answerButtons.Length; i++)
            {
                var idx = i;
                answerButtons[i].gameObject.SetActive(i < npc.answers.Length);
                answerButtons[i].interactable = true;
                var colors = answerButtons[i].colors;
                colors.normalColor = Color.white;
                answerButtons[i].colors = colors;
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = npc.answers[i];
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswer(idx));
            }
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
                for (int i = 0; i < answerButtons.Length; i++)
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