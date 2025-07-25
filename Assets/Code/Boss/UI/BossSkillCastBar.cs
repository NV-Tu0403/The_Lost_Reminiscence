using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        private Coroutine castAnimationCoroutine;

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
                if (fillImage != null && uiConfig != null)
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
            Debug.Log($"[BossSkillCastBar] OnSkillCasted triggered - Skill: {data?.stringValue}");
            SetVisible(true);
            
            if (skillNameText != null && data != null)
            {
                skillNameText.text = data.stringValue ?? "Casting Skill...";
                Debug.Log($"[BossSkillCastBar] Skill name set to: {skillNameText.text}");
            }
            else
            {
                Debug.LogWarning("[BossSkillCastBar] skillNameText is null or data is null");
            }
            
            if (castSlider != null)
            {
                castSlider.value = 0f;
                Debug.Log("[BossSkillCastBar] Cast slider reset to 0");
            }
            else
            {
                Debug.LogWarning("[BossSkillCastBar] castSlider is null");
            }
        }

        private void OnSkillCastProgress(BossEventData data)
        {
            if (castSlider != null && isVisible)
            {
                // Trực tiếp set giá trị thay vì animation phức tạp
                castSlider.value = data.floatValue;
                
                // Debug để xem progress có được nhận không
                Debug.Log($"[BossSkillCastBar] Progress updated: {data.floatValue:F2}");
            }
        }

        private void OnSkillInterrupted(BossEventData data)
        {
            Debug.Log("[BossSkillCastBar] OnSkillInterrupted called - hiding cast bar");
            SetVisible(false);
        }

        private void OnStateChanged(BossEventData data)
        {
            Debug.Log($"[BossSkillCastBar] OnStateChanged called - current state: {data?.stringValue}, isVisible: {isVisible}");
            // Chỉ hide cast bar khi chuyển sang state khác (không phải ScreamState hoặc FearZoneState)
            // và không phải khi skill vừa được activate
            if (isVisible && data?.stringValue != "ScreamState" && data?.stringValue != "FearZoneState" && data?.stringValue != "DecoyState")
            {
                Debug.Log("[BossSkillCastBar] Hiding cast bar due to state change");
                SetVisible(false);
            }
        }

        private void SetVisible(bool visible)
        {
            Debug.Log($"[BossSkillCastBar] SetVisible called with: {visible}");
            isVisible = visible;
            
            if (castBarContainer != null)
            {
                Debug.Log($"[BossSkillCastBar] Using castBarContainer, setting active to: {visible}");
                castBarContainer.SetActive(visible);
            }
            else if (gameObject != null)
            {
                Debug.Log($"[BossSkillCastBar] castBarContainer is null, using gameObject, setting active to: {visible}");
                gameObject.SetActive(visible);
            }
            else
            {
                Debug.LogError("[BossSkillCastBar] Both castBarContainer and gameObject are null!");
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