using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Events.Puzzle.Test.PuzzleDemo
{
    public class UISpirit : MonoBehaviour
    {
        [SerializeField] private Slider spiritSlider;
        [SerializeField] private TextMeshProUGUI spiritText;
        [SerializeField] private float smoothDuration = 0.5f;
        private Coroutine smoothCoroutine;

        public void SetSpirit(int current, int max)
        {
            if (spiritSlider != null)
            {
                if (smoothCoroutine != null) StopCoroutine(smoothCoroutine);
                smoothCoroutine = StartCoroutine(SmoothSpirit(current, max));
            }
            if (spiritText != null)
            {
                spiritText.text = $"{current} / {max}";
            }
        }

        private IEnumerator SmoothSpirit(int target, int max)
        {
            float start = spiritSlider.value;
            float end = (float)target / max;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / smoothDuration;
                spiritSlider.value = Mathf.Lerp(start, end, t);
                yield return null;
            }
            spiritSlider.value = end;
        }
    }
}