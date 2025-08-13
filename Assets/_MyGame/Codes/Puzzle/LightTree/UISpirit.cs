using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Puzzle.LightTree
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
        public void SetSpirit(int current, int max, bool instantUpdate = false)
        {
            if (spiritSlider != null)
            {
                if (_smoothCoroutine != null) StopCoroutine(_smoothCoroutine);
                var value = (float)current / max;
                if (instantUpdate)
                {
                    spiritSlider.value = value;
                }
                else
                {
                    _smoothCoroutine = StartCoroutine(SmoothSpirit(current, max));
                }
            }
            if (spiritText != null)
            {
                spiritText.text = $"{current} / {max}";
            }
        }

        // Coroutine để làm mượt quá trình cập nhật thanh Spirit (Sunsilk belike)
        private IEnumerator SmoothSpirit(int target, int max)
        {
            var start = spiritSlider.value;
            var end = (float)target / max;
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / smoothDuration;
                spiritSlider.value = Mathf.Lerp(start, end, t);
                yield return null;
            }
            spiritSlider.value = end;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}