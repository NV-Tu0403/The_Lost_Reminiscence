using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Events.Puzzle.Test.PuzzleDemo
{
    public class DialogueSupUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject panel;
        public TextMeshProUGUI questionText;
        public Button[] answerButtons; 
        private NPCSup currentNpc;

        private void Awake()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        public void Show(NPCSup npc)
        {
            currentNpc = npc;
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

        public void OnAnswer(int idx)
        {
            if (currentNpc == null) return;
            bool isCorrect = idx == currentNpc.correctIndex;
            if (isCorrect)
            {
                currentNpc.OnAnswered(true);
                Close();
            }
            else
            {
                // Làm mờ button, không cho bấm lại
                answerButtons[idx].interactable = false;
                var colors = answerButtons[idx].colors;
                colors.normalColor = Color.gray;
                answerButtons[idx].colors = colors;
                // Trừ máu
                Puzzle3.Instance.ReduceSpirit(2);
                // Nếu hết máu thì tự động đóng UI
                if (Puzzle3.Instance.currentSpirits <= 0)
                {
                    Close();
                }
            }
        }

        public void Close()
        {
            panel.SetActive(false);
            currentNpc = null;
        }
    }
}