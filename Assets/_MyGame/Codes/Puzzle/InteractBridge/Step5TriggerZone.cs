using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using _MyGame.Codes.Trigger;
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
            TryExecuteProgression(true, false);
        }
    }
}