using _MyGame.Codes.Trigger;
using Code.Puzzle.InteractBridge;
using UnityEngine;

namespace _MyGame.Codes.Puzzle.InteractBridge
{
    public class CheckCompleteZone : TriggerZone
    {
        public PuzzleStep5 puzzleStep5; 
        
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            Debug.Log("[CheckCompleteZone] Triggered by " + other.name);
            if (puzzleStep5 != null)
            {
                puzzleStep5.puzzleCompleted = true;
                puzzleStep5._onComplete?.Invoke();
                
            }
            DisableZone();
        }
    }
}
