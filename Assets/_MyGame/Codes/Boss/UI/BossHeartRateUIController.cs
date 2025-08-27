using _MyGame.Codes.Boss.CoreSystem;
using _ThirdParty.SWP_HeartRateMonitor.Scripts;
using UnityEngine;

namespace _MyGame.Codes.Boss.UI
{
    /// <summary>
    /// Controls the Heart Rate UI canvas for the boss fight.
    /// - Shows when boss fight starts (boss spawn), hides when boss defeated.
    /// - Updates color based on player health (3/2/1 HP => Normal/Medium/Bad).
    /// - Sets BPM=180 when player in FearZone, restores to default when out.
    /// </summary>
    public class BossHeartRateUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject heartRateCanvas;
        [SerializeField] private SwpHeartRateMonitor heartRateMonitor;

        [Header("BPM Settings")]
        [SerializeField] private int fearZoneBpm = 180;

        private int _defaultBpm = 90;

        private void Awake()
        {
            // Auto-find monitor if not assigned
            if (heartRateMonitor == null && heartRateCanvas != null)
            {
                heartRateMonitor = heartRateCanvas.GetComponentInChildren<SwpHeartRateMonitor>(true);
            }
        }

        private void Start()
        {
            if (heartRateMonitor != null)
            {
                _defaultBpm = Mathf.Max(1, heartRateMonitor.beatsPerMinute);
            }
            SetCanvasActive(false);

            BossEventSystem.Subscribe(BossEventType.BossFightStarted, OnBossFightStarted);
            BossEventSystem.Subscribe(BossEventType.BossDefeated, OnBossDefeated);
            BossEventSystem.Subscribe(BossEventType.PlayerHealthChanged, OnPlayerHealthChanged);
            BossEventSystem.Subscribe(BossEventType.PlayerHealthReset, OnPlayerHealthReset);
            BossEventSystem.Subscribe(BossEventType.PlayerEnteredFearZone, OnPlayerEnteredFearZone);
            BossEventSystem.Subscribe(BossEventType.PlayerExitedFearZone, OnPlayerExitedFearZone);
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.BossFightStarted, OnBossFightStarted);
            BossEventSystem.Unsubscribe(BossEventType.BossDefeated, OnBossDefeated);
            BossEventSystem.Unsubscribe(BossEventType.PlayerHealthChanged, OnPlayerHealthChanged);
            BossEventSystem.Unsubscribe(BossEventType.PlayerHealthReset, OnPlayerHealthReset);
            BossEventSystem.Unsubscribe(BossEventType.PlayerEnteredFearZone, OnPlayerEnteredFearZone);
            BossEventSystem.Unsubscribe(BossEventType.PlayerExitedFearZone, OnPlayerExitedFearZone);
        }

        private void OnBossFightStarted(BossEventData _)
        {
            SetCanvasActive(true);
            // Initialize to healthy visuals and default BPM
            ApplyColorForHealth(3);
            RestoreDefaultBpm();
        }

        private void OnBossDefeated(BossEventData _)
        {
            SetCanvasActive(false);
        }

        private void OnPlayerHealthChanged(BossEventData data)
        {
            ApplyColorForHealth(Mathf.Clamp(data?.intValue ?? 3, 0, 3));
        }

        private void OnPlayerHealthReset(BossEventData data)
        {
            ApplyColorForHealth(Mathf.Clamp(data?.intValue ?? 3, 0, 3));
            RestoreDefaultBpm();
        }

        private void OnPlayerEnteredFearZone(BossEventData _)
        {
            if (heartRateMonitor == null) return;
            heartRateMonitor.beatsPerMinute = fearZoneBpm;
        }

        private void OnPlayerExitedFearZone(BossEventData _)
        {
            RestoreDefaultBpm();
        }

        private void ApplyColorForHealth(int health)
        {
            if (heartRateMonitor == null) return;
            // Ensure material is configured on the monitor to avoid exceptions
            if (heartRateMonitor.mainMaterial == null) return;

            switch (health)
            {
                case 3:
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.normalColour);
                    break;
                case 2:
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.mediumColour);
                    break;
                case 1:
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.badColour);
                    break;
                default:
                    return;
            }
        }

        private void RestoreDefaultBpm()
        {
            if (heartRateMonitor == null) return;
            heartRateMonitor.beatsPerMinute = _defaultBpm;
        }

        private void SetCanvasActive(bool active)
        {
            if (heartRateCanvas == null) return;
            if (heartRateCanvas.activeSelf != active)
            {
                heartRateCanvas.SetActive(active);
            }
        }
    }
}

