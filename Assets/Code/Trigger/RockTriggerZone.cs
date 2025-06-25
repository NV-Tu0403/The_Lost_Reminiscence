using Code.GameEventSystem;
using Code.Procession;
using Code.Trigger;
using Script.Procession;
using UnityEngine;

namespace Script.Trigger
{
    public class RockTriggerZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Rock");
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
        }
    }
}