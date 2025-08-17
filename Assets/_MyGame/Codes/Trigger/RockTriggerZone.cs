using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using _MyGame.Codes.Trigger;
using UnityEngine;

namespace Code.Trigger
{
    public class RockTriggerZone : TriggerZone
    {
        private Runestone_Controller runestoneController;
        
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Bullet");
        }

        protected override void OnTriggered(Collider other)
        {
            // Kiểm tra điều kiện progression
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[RockTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }
            
            // Unlock → Trigger → Disable zone
            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
            DisableZone();
            
            // Enable Effect
            PlayEffect();
        }

        private void PlayEffect()
        {
            if (runestoneController == null)
            {
                runestoneController = FindFirstObjectByType<Runestone_Controller>();
                if (runestoneController == null)
                {
                    Debug.LogError("[RockTriggerZone] Không tìm thấy Runestone_Controller trong scene.");
                    return;
                }
            }
            runestoneController.ToggleRuneStone(true);
        }
    }
}