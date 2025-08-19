using UnityEngine;

namespace _MyGame.Codes
{
    public class CreditScript : MonoBehaviour
    {
        public float scrollSpeed = 20f; // Tốc độ cuộn của credit
        
        private RectTransform _rectTransform; // RectTransform của đối tượng này
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        private void Update()
        {
            _rectTransform.anchoredPosition = new Vector2(0, _rectTransform.anchoredPosition.y + scrollSpeed * Time.deltaTime);
        }
    }
}
