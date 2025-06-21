using DuckLe;
using Script.Puzzle.LightTree;
using UnityEngine;

namespace Script.Trigger
{
    public class IdTriggerZone : TriggerZone
    {
        [SerializeField] private PuzzleStep6 puzzleStep6;
        private int zoneIndex = 1;

        protected override bool IsValidTrigger(Collider other)
        {
            // Chỉ trigger với Player (TestController)
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            if (puzzleStep6 != null)
            {
                // Đánh dấu player đang ở zone 1
                puzzleStep6.SetPlayerCurrentZone(zoneIndex);
                Debug.Log("[IdTriggerZone] Player entered zone 1");
                // Gọi ghost chase player ngay khi vào zone
                var player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    foreach (var id in puzzleStep6.GetIds())
                    {
                        id.SetChaseTarget(player);
                    }
                }
            }
            //DisableZone();
        }
    }
}