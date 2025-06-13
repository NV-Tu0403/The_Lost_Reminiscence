using Events.TestPuzzle;
using Script.GameEventSystem;
using Script.Procession;
using UnityEngine;

namespace Events.TriggerZone
{
    public class DeadTriggerZone : TriggerZone
    {
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            var puzzleStep5 = FindObjectOfType<Events.Puzzle.StepPuzzle.InteractBridge.PuzzleStep5>();
            if (puzzleStep5 != null)
            {
                puzzleStep5.ResetPlayer();
                puzzleStep5.StartCoroutine(puzzleStep5.ResetPuzzleAfterFail());
            }
            else
            {
                Debug.LogWarning("[DeadTriggerZone] Không tìm thấy PuzzleStep5 để reset!");
            }
        }
    }
}