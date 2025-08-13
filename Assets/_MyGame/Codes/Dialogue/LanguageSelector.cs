using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using DG.Tweening;

namespace Code.Dialogue
{
    public class LanguageSelector : MonoBehaviour
    {
        [Header("UI Panel")]
        [SerializeField] private GameObject languagePanel;
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private TextMeshProUGUI languageText;
        [SerializeField] private Image languageIcon;
        
        [Header("Language Data")]
        [SerializeField] private List<Sprite> flagIcons;
        [SerializeField] private List<string> languageNames;
        
        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float slideOutDuration = 0.5f;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private Vector2 hiddenPosition = new Vector2(400f, 0f);
        [SerializeField] private Vector2 shownPosition = Vector2.zero;
        
        private int currentLanguageIndex = 0;
        private bool isAnimating = false;
        private Coroutine hideCoroutine;
        private Vector2 originalPosition;

        private void Start()
        {
            InitializeLanguageData();
            SetupPanel();
            UpdateCurrentLanguageIndex();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.L) || isAnimating) return;
            HandlePress();
            Debug.Log($"Current Language: {LocalizationSettings.SelectedLocale.Identifier.Code}");
        }

        private void InitializeLanguageData()
        {
            // Nếu không có language names được set, tự động lấy từ localization
            if (languageNames != null && languageNames.Count != 0) return;
            languageNames = new List<string>();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                languageNames.Add(locale.Identifier.CultureInfo.NativeName);
            }
        }

        private void SetupPanel()
        {
            if (panelRectTransform == null)
                panelRectTransform = languagePanel.GetComponent<RectTransform>();
            
            // Lưu vị trí gốc và ẩn panel
            originalPosition = panelRectTransform.anchoredPosition;
            shownPosition = originalPosition;
            panelRectTransform.anchoredPosition = originalPosition + hiddenPosition;
            languagePanel.SetActive(false);
        }

        private void UpdateCurrentLanguageIndex()
        {
            var currentLocale = LocalizationSettings.SelectedLocale;
            for (var i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                if (LocalizationSettings.AvailableLocales.Locales[i].Identifier != currentLocale.Identifier) continue;
                currentLanguageIndex = i;
                break;
            }
        }

        private void HandlePress()
        {
            if (languagePanel.activeInHierarchy)
            {
                // Nếu panel đang hiển thị, đổi ngôn ngữ
                ChangeToNextLanguage();
            }
            
            ShowLanguagePanel();
        }

        private void ChangeToNextLanguage()
        {
            currentLanguageIndex = (currentLanguageIndex + 1) % LocalizationSettings.AvailableLocales.Locales.Count;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        }

        private void ShowLanguagePanel()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            isAnimating = true;
            languagePanel.SetActive(true);
            
            // Cập nhật nội dung panel
            UpdatePanelContent();
            
            // Animation slide in
            panelRectTransform.anchoredPosition = originalPosition + hiddenPosition;
            panelRectTransform.DOAnchorPos(shownPosition, slideInDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    isAnimating = false;
                    hideCoroutine = StartCoroutine(HidePanelAfterDelay());
                });
        }

        private void UpdatePanelContent()
        {
            // Cập nhật text
            if (languageText != null && currentLanguageIndex < languageNames.Count)
            {
                languageText.text = languageNames[currentLanguageIndex];
            }
            
            // Cập nhật icon
            if (languageIcon != null && currentLanguageIndex < flagIcons.Count)
            {
                languageIcon.sprite = flagIcons[currentLanguageIndex];
            }
        }

        private IEnumerator HidePanelAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            HideLanguagePanel();
        }

        private void HideLanguagePanel()
        {
            if (!languagePanel.activeInHierarchy) return;
            
            isAnimating = true;
            
            // Animation slide out
            panelRectTransform.DOAnchorPos(originalPosition + hiddenPosition, slideOutDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    languagePanel.SetActive(false);
                    isAnimating = false;
                });
        }

        private void OnDestroy()
        {
            // Cleanup DOTween
            panelRectTransform?.DOKill();
        }
    }
}