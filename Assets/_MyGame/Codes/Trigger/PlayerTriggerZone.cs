using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    public class PlayerTriggerZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other) { return other.CompareTag("Player"); }
        protected override void OnTriggered(Collider other) { HandleDefaultTrigger(); }
    }
}