using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Boss
{
   /// <summary>
    /// Thanh cast skill ở dưới thanh máu boss
    /// </summary>
    public class BossSkillCastBar : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider castSlider;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private GameObject castBarContainer;
        
        private UIConfig uiConfig;
        private bool isVisible = false;

        public void Initialize(BossController controller)
        {
            uiConfig = controller.Config.uiConfig;
            
            SetupUI();
            RegisterEvents();
            
            // Hide initially
            SetVisible(false);
        }

        private void SetupUI()
        {
            // Setup cast slider
            if (castSlider != null)
            {
                castSlider.maxValue = 1f;
                castSlider.value = 0f;
                
                // Set colors
                var fillImage = castSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = uiConfig.skillCastColor;
                }
            }
        }

        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.SkillCasted, OnSkillCasted);
            BossEventSystem.Subscribe(BossEventType.SkillCastProgress, OnSkillCastProgress);
            BossEventSystem.Subscribe(BossEventType.SkillInterrupted, OnSkillInterrupted);
            BossEventSystem.Subscribe(BossEventType.StateChanged, OnStateChanged);
        }

        private void OnSkillCasted(BossEventData data)
        {
            SetVisible(true);
            
            if (skillNameText != null && data != null)
            {
                skillNameText.text = data.stringValue ?? "Casting Skill...";
            }
            
            if (castSlider != null)
            {
                castSlider.value = 0f;
            }
        }

        private void OnSkillCastProgress(BossEventData data)
        {
            if (castSlider != null && isVisible)
            {
                castSlider.value = data.floatValue;
            }
        }

        private void OnSkillInterrupted(BossEventData data)
        {
            SetVisible(false);
        }

        private void OnStateChanged(BossEventData data)
        {
            // Hide cast bar when state changes (skill completed)
            if (isVisible)
            {
                SetVisible(false);
            }
        }

        private void SetVisible(bool visible)
        {
            isVisible = visible;
            if (castBarContainer != null)
            {
                castBarContainer.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.SkillCasted, OnSkillCasted);
            BossEventSystem.Unsubscribe(BossEventType.SkillCastProgress, OnSkillCastProgress);
            BossEventSystem.Unsubscribe(BossEventType.SkillInterrupted, OnSkillInterrupted);
            BossEventSystem.Unsubscribe(BossEventType.StateChanged, OnStateChanged);
        }
    }
}