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
        private Coroutine _blinkCoroutine;

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
        
        public void StartBlink(TextMeshProUGUI target)
        {
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = StartCoroutine(BlinkRoutine(target));
        }

        public void StopBlink()
        {
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        IEnumerator BlinkRoutine(TextMeshProUGUI target)
        {
            while (true)
            {
                target.alpha = 1f;
                yield return new WaitForSeconds(0.5f);
                target.alpha = 0.2f;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
    }
}