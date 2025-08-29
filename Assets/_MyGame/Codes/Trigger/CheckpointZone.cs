using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    public class CheckpointZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        { 
            if (!CanProgress()) return;
            ExecuteProgression(false, false);
        }
    }
}