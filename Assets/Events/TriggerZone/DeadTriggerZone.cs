using Script.GameEventSystem;
using Script.Procession;
using UnityEngine;

namespace Events.TriggerZone
{
    public class DeadTriggerZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            // Kiểm tra điều kiện progression
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[DeadTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }
        }
    }
}