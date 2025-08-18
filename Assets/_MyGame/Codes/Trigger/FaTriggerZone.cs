using UnityEngine;

namespace _MyGame.Codes.Trigger
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
            if (!TryExecuteProgression(true, false)) return;
            if (portal == null)
            {
                portal = FindFirstObjectByType<PortalRound_Controller>();
            }
            if (portal != null)
            {
                portal.F_TogglePortalRound(true);
            }
            else
            {
                Debug.LogWarning("[FaTriggerZone] Không tìm thấy PortalRound_Controller trong scene.");
            }
        }
    }
}