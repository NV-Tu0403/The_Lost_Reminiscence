using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using Code.GameEventSystem;
using Code.Trigger;
using UnityEngine;

namespace Script.Trigger
{
    public class FaTriggerZone : TriggerZone
    {
        private PortalRound_Controller portal;
        
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Fa");
        }

        protected override void OnTriggered(Collider other)
        {
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[PlayerTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }

            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);

            
            if (portal == null)
            {
                portal = FindFirstObjectByType<PortalRound_Controller>();
            }
            portal.F_TogglePortalRound(true);
        }
    }
}