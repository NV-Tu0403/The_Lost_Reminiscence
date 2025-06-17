using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Events.Puzzle.Test.Puzzle3
{
    public class FadeManager : MonoBehaviour
    {
        public static FadeManager Instance;
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (fadeImage != null)
            {
                var c = fadeImage.color;
                fadeImage.color = new Color(c.r, c.g, c.b, 0f);
            }
        }

        public void FadeToBlack(System.Action onComplete = null)
        {
            StartCoroutine(Fade(0f, 1f, onComplete));
        }

        public void FadeFromBlack(System.Action onComplete = null)
        {
            StartCoroutine(Fade(1f, 0f, onComplete));
        }

        private IEnumerator Fade(float from, float to, System.Action onComplete)
        {
            float elapsed = 0f;
            Color c = fadeImage.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                fadeImage.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
            fadeImage.color = new Color(c.r, c.g, c.b, to);
            onComplete?.Invoke();
        }
    }
}