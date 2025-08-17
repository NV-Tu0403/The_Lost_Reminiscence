using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using Script.Trigger;
using UnityEngine;

namespace Code.Trigger
{
    public class PlayerTriggerZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            // Chỉ trigger khi va chạm với Player
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            // Kiểm tra điều kiện progression
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[PlayerTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }
            // Unlock → Trigger → Disable zone
            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
            DisableZone();
        }
    }
}