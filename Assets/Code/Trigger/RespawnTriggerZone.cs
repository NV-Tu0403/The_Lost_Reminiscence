using Code.GameEventSystem;
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
            EventBus.Publish(eventId, eventId);
        }
    }
}