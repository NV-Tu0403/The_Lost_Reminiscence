using Code.Trigger;
using UnityEngine;

namespace Code.Puzzle.InteractBridge
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
            if (puzzleStep5 != null)
            {
                // Nếu puzzle đã hoàn thành, thực hiện hành động cần thiết
               // Debug.Log("Puzzle 5 đã hoàn thành!");
                puzzleStep5.puzzleCompleted = true;
            }
            else
            // Vô hiệu hóa zone sau khi trigger
            DisableZone();
            puzzleStep5.countdownCanvas.enabled = false;
        }
    }
}
