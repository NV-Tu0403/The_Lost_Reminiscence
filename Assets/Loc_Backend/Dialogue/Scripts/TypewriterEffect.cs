using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts
{
    public class TypewriterEffect : MonoBehaviour
    {
        public TextMeshProUGUI dialogueText;
        public float typeSpeed = 0.05f;
        private Coroutine _typingCoroutine;

        public void StartTypewriter(string message, Action onComplete = null)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeText(message, onComplete));
        }

        IEnumerator TypeText(string message, Action onComplete)
        {
            dialogueText.text = "";
            foreach (char c in message)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }
            onComplete?.Invoke();
        }
    }
}