using Code.Puzzle.InteractBridge;
using Code.Trigger;
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
            if (puzzleStep5 != null) puzzleStep5.puzzleCompleted = true;
            else
            // Vô hiệu hóa zone sau khi trigger
            DisableZone();
            puzzleStep5.countdownCanvas.enabled = false;
        }
    }
}
