using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Events.Puzzle.Test.PuzzleDemo
{
    /// <summary>
    /// Script này quản lí UI thanh Spirit của người chơi.
    /// </summary>
    public class UISpirit : MonoBehaviour
    {
        [Header("UI Spirit Settings")]
        [Tooltip("Thanh Slider để hiển thị Spirit.")]
        [SerializeField] private Slider spiritSlider;
        
        [Tooltip("Text để hiển thị số lượng Spirit hiện tại.")]
        [SerializeField] private TextMeshProUGUI spiritText;
        
        [Tooltip("Thời gian làm mượt quá trình cập nhật thanh Spirit.")]
        [SerializeField] private float smoothDuration = 0.5f;
        
        
        private Coroutine _smoothCoroutine;

        /// Cập nhật thanh Spirit và hiển thị số lượng Spirit hiện tại
        public void SetSpirit(int current, int max)
        {
            if (spiritSlider != null)
            {
                if (_smoothCoroutine != null) StopCoroutine(_smoothCoroutine);
                _smoothCoroutine = StartCoroutine(SmoothSpirit(current, max));
            }
            if (spiritText != null)
            {
                spiritText.text = $"{current} / {max}";
            }
        }

        // Coroutine để làm mượt quá trình cập nhật thanh Spirit (Sunsilk belike)
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