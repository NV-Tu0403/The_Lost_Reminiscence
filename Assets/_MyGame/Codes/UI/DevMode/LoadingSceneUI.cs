using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace _MyGame.Codes.UI.DevMode
{
    public class LoadingSceneUI : MonoBehaviour
    {
        [Header("Loading UI Elements")]
        [SerializeField] private TextMeshProUGUI loadingText; // Text hiển thị "Đang tải"
        [SerializeField] private TextMeshProUGUI[] loadingDots; // Array 3 TextMeshPro components cho 3 dấu chấm
        [SerializeField] private Image loadingImage;
        [SerializeField] private GameObject loadingPanel; // Panel chứa toàn bộ UI loading
        
        [Header("Loading Images")]
        [SerializeField] private Sprite[] loadingSprites;
        
        [Header("Loading Settings")]
        [SerializeField] private float loadingDuration = 20f;
        [SerializeField] private float imageChangeInterval = 2f;
        [SerializeField] private bool hideAfterComplete = true; // Tự động ẩn sau khi hoàn thành
        
        [Header("Dots Animation Settings")]
        [SerializeField] private float dotAnimationSpeed = 0.5f; // Tốc độ animation dots
        [SerializeField] private AnimationType animationType = AnimationType.FadeInOut; // Loại animation

        private enum AnimationType
        {
            FadeInOut,      // Ẩn hiện
            ScaleUpDown,    // To nhỏ
            MoveUpDown      // Lên xuống
        }
        
        private float currentLoadingTime;
        private int currentImageIndex;
        private int currentDotIndex;
        
        private void Start()
        {
            InitializeLoading();
            StartCoroutine(LoadingProcess());
            StartCoroutine(ChangeLoadingImages());
            StartCoroutine(AnimateLoadingDots());
        }
        
        private void InitializeLoading()
        {
            // Hiển thị loading panel
            if (loadingPanel != null)
                loadingPanel.SetActive(true);
            else
                gameObject.SetActive(true);
                
            if (loadingText != null)
                loadingText.text = "Đang chuẩn bị tài nguyên";
                
            // Khởi tạo dots
            InitializeDots();
                
            if (loadingImage != null && loadingSprites != null && loadingSprites.Length > 0)
                loadingImage.sprite = loadingSprites[0];
        }
        
        private void InitializeDots()
        {
            if (loadingDots == null || loadingDots.Length == 0) return;
            
            for (int i = 0; i < loadingDots.Length; i++)
            {
                if (loadingDots[i] != null)
                {
                    loadingDots[i].text = "•";
                    
                    // Reset về trạng thái ban đầu tùy theo loại animation
                    switch (animationType)
                    {
                        case AnimationType.FadeInOut:
                            SetDotAlpha(i, 0.3f);
                            break;
                        case AnimationType.ScaleUpDown:
                            loadingDots[i].transform.localScale = Vector3.one * 0.5f;
                            break;
                        case AnimationType.MoveUpDown:
                            loadingDots[i].transform.localPosition = Vector3.zero;
                            break;
                    }
                }
            }
        }
        
        private IEnumerator LoadingProcess()
        {
            while (currentLoadingTime < loadingDuration)
            {
                currentLoadingTime += Time.deltaTime;
                yield return null;
            }
            
            // Loading completed
            OnLoadingComplete();
        }
        
        private IEnumerator AnimateLoadingDots()
        {
            if (loadingDots == null || loadingDots.Length == 0)
                yield break;
                
            while (currentLoadingTime < loadingDuration)
            {
                // Reset tất cả dots về trạng thái normal
                ResetAllDots();
                
                // Animate dot hiện tại
                if (currentDotIndex < loadingDots.Length && loadingDots[currentDotIndex] != null)
                {
                    yield return StartCoroutine(AnimateSingleDot(currentDotIndex));
                }
                
                // Chuyển sang dot tiếp theo
                currentDotIndex = (currentDotIndex + 1) % loadingDots.Length;
                
                yield return new WaitForSeconds(dotAnimationSpeed);
            }
        }
        
        private void ResetAllDots()
        {
            for (var i = 0; i < loadingDots.Length; i++)
            {
                if (loadingDots[i] != null)
                {
                    switch (animationType)
                    {
                        case AnimationType.FadeInOut:
                            SetDotAlpha(i, 0.3f);
                            break;
                        case AnimationType.ScaleUpDown:
                            loadingDots[i].transform.localScale = Vector3.one * 0.5f;
                            break;
                        case AnimationType.MoveUpDown:
                            loadingDots[i].transform.localPosition = Vector3.zero;
                            break;
                    }
                }
            }
        }
        
        private IEnumerator AnimateSingleDot(int dotIndex)
        {
            if (dotIndex >= loadingDots.Length || loadingDots[dotIndex] == null)
                yield break;
                
            var dotTransform = loadingDots[dotIndex].transform;
            var animTime = dotAnimationSpeed * 0.8f;
            
            switch (animationType)
            {
                case AnimationType.FadeInOut:
                    // Fade in
                    yield return StartCoroutine(FadeDot(dotIndex, 0.3f, 1f, animTime * 0.5f));
                    // Fade out
                    yield return StartCoroutine(FadeDot(dotIndex, 1f, 0.3f, animTime * 0.5f));
                    break;
                    
                case AnimationType.ScaleUpDown:
                    // Scale up
                    yield return StartCoroutine(ScaleDot(dotTransform, Vector3.one * 0.5f, Vector3.one * 1.2f, animTime * 0.5f));
                    // Scale down
                    yield return StartCoroutine(ScaleDot(dotTransform, Vector3.one * 1.2f, Vector3.one * 0.5f, animTime * 0.5f));
                    break;
                    
                case AnimationType.MoveUpDown:
                    // Move up
                    yield return StartCoroutine(MoveDot(dotTransform, Vector3.zero, Vector3.up * 10f, animTime * 0.5f));
                    // Move down
                    yield return StartCoroutine(MoveDot(dotTransform, Vector3.up * 10f, Vector3.zero, animTime * 0.5f));
                    break;
                default:
                    Debug.LogWarning($"[LoadingSceneUI] Unknown animation type: {animationType}");
                    yield break;
            }
        }
        
        private IEnumerator FadeDot(int dotIndex, float fromAlpha, float toAlpha, float duration)
        {
            var elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / duration);
                SetDotAlpha(dotIndex, alpha);
                yield return null;
            }
            
            SetDotAlpha(dotIndex, toAlpha);
        }
        
        private static IEnumerator ScaleDot(Transform dotTransform, Vector3 fromScale, Vector3 toScale, float duration)
        {
            var elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                dotTransform.localScale = Vector3.Lerp(fromScale, toScale, elapsedTime / duration);
                yield return null;
            }
            
            dotTransform.localScale = toScale;
        }
        
        private static IEnumerator MoveDot(Transform dotTransform, Vector3 fromPos, Vector3 toPos, float duration)
        {
            var elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                dotTransform.localPosition = Vector3.Lerp(fromPos, toPos, elapsedTime / duration);
                yield return null;
            }
            
            dotTransform.localPosition = toPos;
        }
        
        private void SetDotAlpha(int dotIndex, float alpha)
        {
            if (dotIndex >= loadingDots.Length || loadingDots[dotIndex] == null) return;
            var color = loadingDots[dotIndex].color;
            color.a = alpha;
            loadingDots[dotIndex].color = color;
        }
        
        private IEnumerator ChangeLoadingImages()
        {
            if (loadingSprites == null || loadingSprites.Length == 0 || loadingImage == null)
                yield break;
                
            while (currentLoadingTime < loadingDuration)
            {
                yield return new WaitForSeconds(imageChangeInterval);
                
                if (currentLoadingTime < loadingDuration)
                {
                    currentImageIndex = (currentImageIndex + 1) % loadingSprites.Length;
                    loadingImage.sprite = loadingSprites[currentImageIndex];
                }
            }
        }
        
        private void OnLoadingComplete()
        {
            if (loadingText != null)
                loadingText.text = "...";
            
            // Hiển thị tất cả dots khi hoàn thành
            for (int i = 0; i < loadingDots.Length; i++)
            {
                if (loadingDots[i] != null)
                {
                    SetDotAlpha(i, 1f);
                    loadingDots[i].transform.localScale = Vector3.one;
                    loadingDots[i].transform.localPosition = Vector3.zero;
                }
            }
            
            if (hideAfterComplete)
            {
                StartCoroutine(HideLoadingScreen());
            }
        }
        
        private IEnumerator HideLoadingScreen()
        {
            yield return new WaitForSeconds(0.5f);
            
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        
        public void HideLoading()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        
        public void ShowLoading()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(true);
            else
                gameObject.SetActive(true);
        }
        
        public void SetLoadingMessage(string message)
        {
            if (loadingText != null)
                loadingText.text = message;
        }
    }
}