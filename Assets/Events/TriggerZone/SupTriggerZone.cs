using Events.Puzzle.StepPuzzle.LightTree;
using UnityEngine;

namespace Events.TriggerZone
{
    public class SupTriggerZone : TriggerZone
    {
        [SerializeField] private PuzzleStep6 puzzleStep6;
        private int zoneIndex = 2;

        protected override bool IsValidTrigger(Collider other)
        {
            // Chỉ trigger với Player (TestController)
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            if (puzzleStep6 != null)
            {
                // Đánh dấu player đang ở zone 2
                puzzleStep6.SetPlayerCurrentZone(zoneIndex);
            }
            DisableZone();
        }
    }
}