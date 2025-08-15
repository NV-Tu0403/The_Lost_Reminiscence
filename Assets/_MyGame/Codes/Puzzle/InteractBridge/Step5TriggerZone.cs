using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using Code.GameEventSystem;
using Code.Trigger;
using UnityEngine;

namespace _MyGame.Codes.Puzzle.InteractBridge
{
    public class Step5TriggerZone : TriggerZone
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
                Debug.Log($"[Step5TriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }
            
            // Unlock → Trigger
            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
        }
    }
}