using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using _MyGame.Codes.Trigger;
using UnityEngine;

namespace Script.Trigger
{
    public class FaTriggerZone : TriggerZone
    {
        private PortalRound_Controller portal;
        
        protected override bool IsValidTrigger(Collider other) { return other.CompareTag("Fa"); }

        protected override void OnTriggered(Collider other) { HandleCustomTrigger(afterProgression: PlayEffect);}

        private void PlayEffect()
        {
            if (portal == null)
            {
                portal = FindFirstObjectByType<PortalRound_Controller>();
            }

            portal.F_TogglePortalRound(true);
        }
    }
}