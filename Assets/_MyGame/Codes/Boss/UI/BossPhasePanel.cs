using System.Collections;
using _MyGame.Codes.Boss.CoreSystem;
using TMPro;
using UnityEngine;

namespace _MyGame.Codes.Boss.UI
{
    /// <summary>
    /// Panel hiển thị Phase khi boss chuyển phase. Hiện ~2s rồi ẩn.
    /// </summary>
    public class BossPhasePanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panelRoot;             // Gốc panel để show/hide
        [SerializeField] private TextMeshProUGUI phaseText;        // Text hiển thị "Phase X"
        [SerializeField] private CanvasGroup canvasGroup;          // Optional: để fade in/out

        [Header("Behavior")]
        [Tooltip("Thời gian hiển thị panel khi đổi phase")]
        [SerializeField] private float showDuration = 2f;

        [Header("Audio (Normal)")]
        [Tooltip("AudioSource để phát SFX (2D). Nếu bỏ trống, script sẽ tự thêm vào panelRoot).")]
        [SerializeField] private AudioSource sfxSource;
        [Tooltip("Âm thanh phát khi panel phase xuất hiện.")]
        [SerializeField] private AudioClip phaseShowClip;

        private UIConfig uiConfig;
        private Coroutine showRoutine;
        private bool initialized;

        public void Initialize(BossController controller)
        {
            // Ensure a sensible default for panelRoot
            if (panelRoot == null) panelRoot = gameObject;

            uiConfig = controller.Config.uiConfig;

            if (!initialized)
            {
                // Ensure hidden initially
                SetVisible(false, immediate: true);
                EnsureAudioSource();
                RegisterEvents();
                initialized = true;
            }
        }

        private void EnsureAudioSource()
        {
            if (sfxSource == null)
            {
                // Prefer AudioSource on this component's GameObject to avoid inactive panelRoot issues
                sfxSource = GetComponent<AudioSource>();
            }
            if (sfxSource == null)
            {
                // Auto-add a 2D AudioSource for UI SFX on this object
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // 2D sound
                sfxSource.outputAudioMixerGroup = null; // let Unity route to default unless user assigns
            }
        }

        private void RegisterEvents()
        {
            BossEventSystem.Subscribe(BossEventType.PhaseChanged, OnPhaseChanged);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnHideAll);
            BossEventSystem.Subscribe(BossEventType.PlayerDefeated, OnHideAll);
        }

        public void ShowPhaseNow(int phase)
        {
            // Ensure this MonoBehaviour is active and enabled before starting a coroutine
            if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
            if (!enabled) enabled = true;
            
            if (phaseText != null)
            {
                phaseText.text = $"Phase {phase}";
            }

            // Play normal audio SFX (no FMOD)
            if (phaseShowClip != null)
            {
                EnsureAudioSource();
                if (sfxSource != null)
                {
                    sfxSource.PlayOneShot(phaseShowClip);
                }
            }

            if (showRoutine != null) StopCoroutine(showRoutine);
            showRoutine = StartCoroutine(ShowForSeconds(showDuration));
        }

        private void OnPhaseChanged(BossEventData data)
        {
            int phase = data.intValue;
            ShowPhaseNow(phase);
        }

        private IEnumerator ShowForSeconds(float seconds)
        {
            SetVisible(true, immediate: false);
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetVisible(false, immediate: false);
            showRoutine = null;
        }

        private void SetVisible(bool visible, bool immediate)
        {
            if (panelRoot != null)
            {
                if (canvasGroup == null)
                {
                    // Simple toggle without fade
                    panelRoot.SetActive(visible);
                }
                else
                {
                    // Activate object to allow fade
                    if (!panelRoot.activeSelf) panelRoot.SetActive(true);
                    if (immediate)
                    {
                        canvasGroup.alpha = visible ? 1f : 0f;
                        panelRoot.SetActive(visible);
                    }
                    else
                    {
                        // Start fade coroutine
                        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                        fadeCoroutine = StartCoroutine(FadeTo(visible ? 1f : 0f, GetFadeDuration()));
                    }
                }
            }
        }

        private float GetFadeDuration()
        {
            // Dùng uiAnimationSpeed như thời gian fade nếu có, mặc định 0.25s
            return (uiConfig != null && uiConfig.uiAnimationSpeed > 0f) ? Mathf.Min(uiConfig.uiAnimationSpeed, 0.5f) : 0.25f;
        }

        private Coroutine fadeCoroutine;
        private IEnumerator FadeTo(float target, float duration)
        {
            if (canvasGroup == null)
                yield break;

            float start = canvasGroup.alpha;
            float t = 0f;
            var curve = uiConfig != null && uiConfig.uiAnimationCurve != null ? uiConfig.uiAnimationCurve : AnimationCurve.EaseInOut(0, 0, 1, 1);

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float eased = curve.Evaluate(p);
                canvasGroup.alpha = Mathf.Lerp(start, target, eased);
                yield return null;
            }

            canvasGroup.alpha = target;
            // Disable after fade out
            if (Mathf.Approximately(target, 0f) && panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            fadeCoroutine = null;
        }

        private void OnHideAll(BossEventData _)
        {
            if (showRoutine != null)
            {
                StopCoroutine(showRoutine);
                showRoutine = null;
            }
            SetVisible(false, immediate: true);
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.PhaseChanged, OnPhaseChanged);
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnHideAll);
            BossEventSystem.Unsubscribe(BossEventType.PlayerDefeated, OnHideAll);
        }
    }
}
