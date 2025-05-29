using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loc_Backend.Test
{
    public class CutscenePanel : MonoBehaviour
    {
        public TextMeshProUGUI cutsceneText;
        public Button nextButton;

        private Action onEnd;

        public void Show(string text, Action onEndCallback)
        {
            cutsceneText.text = text;
            onEnd = onEndCallback;
            gameObject.SetActive(true);
        }

        void OnEnable()
        {
            nextButton.onClick.AddListener(HandleNext);
        }

        void OnDisable()
        {
            nextButton.onClick.RemoveListener(HandleNext);
        }

        void HandleNext()
        {
            gameObject.SetActive(false);
            onEnd?.Invoke();
        }
    }
}