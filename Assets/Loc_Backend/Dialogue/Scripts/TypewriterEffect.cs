using System.Collections;
using TMPro;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts
{
    public class TypewriterEffect : MonoBehaviour
    {
        public TextMeshProUGUI dialogueText;
        public float typeSpeed = 0.05f;

        public void StartTypewriter(string message)
        {
            StopAllCoroutines();
            StartCoroutine(TypeText(message));
        }

        IEnumerator TypeText(string message)
        {
            dialogueText.text = "";
            foreach (char c in message)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }
        }
    }
}