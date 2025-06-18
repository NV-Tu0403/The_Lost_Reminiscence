using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Events.Puzzle.StepPuzzle.LightTree
{
    public class UISupDialogue : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject panel;
        public TextMeshProUGUI questionText;
        public Button[] answerButtons; 
        private SupController _current;

        private void Awake()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        public void Show(SupController npc)
        {
            _current = npc;
            panel.SetActive(true);
            questionText.text = npc.question;
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int idx = i;
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
            if (_current == null) return;
            bool isCorrect = idx == _current.correctIndex;
            if (isCorrect)
            {
                _current.OnAnswered(true);
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    answerButtons[i].interactable = false;
                    var colors = answerButtons[i].colors;
                    colors.normalColor = (i == _current.correctIndex) ? Color.green : Color.white;
                    answerButtons[i].colors = colors;
                }
                Invoke(nameof(Hide), 1f);
            }
            else
            {
                _current.OnAnswered(false);
                answerButtons[idx].interactable = false;
                var colors = answerButtons[idx].colors;
                colors.normalColor = Color.gray;
                answerButtons[idx].colors = colors;
            }
        }

        public void Close()
        {
            panel.SetActive(false);
            _current = null;
        }
    }
}