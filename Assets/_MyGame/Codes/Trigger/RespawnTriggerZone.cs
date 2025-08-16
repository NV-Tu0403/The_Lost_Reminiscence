using _MyGame.Codes.GameEventSystem;
using UnityEngine;

namespace Code.Trigger
{
    public class RespawnTriggerZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            EventBus.Publish("Respawn", eventId);
        }
    }
}