using Script.Puzzle.LightTree;
using UnityEngine;

namespace Script.Trigger
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
            Debug.Log($"[SupTriggerZone] Player entered zone {zoneIndex}");
            if (puzzleStep6 != null)
            {
                // Đánh dấu player đang ở zone 2
                puzzleStep6.SetPlayerCurrentZone(zoneIndex);
            }
            DisableZone();
        }
    }
}