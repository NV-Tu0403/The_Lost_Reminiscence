using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using UnityEngine;

namespace Code.Trigger
{
    public class CheckpointZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        { 
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[PlayerTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }
            EventExecutor.Instance.TriggerEvent(eventId); 
        }
    }
}