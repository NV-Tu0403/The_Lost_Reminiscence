using Code.Puzzle.LightTree;
using Code.Trigger;
using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    public class IdTriggerZone : TriggerZone
    {
        [SerializeField] private PuzzleStep6 puzzleStep6;
        private const int ZoneIndex = 1;

        protected override bool IsValidTrigger(Collider other)
        {
            // Chỉ trigger với Player (TestController)
            return other.CompareTag("Player");
        }

        protected override void OnTriggered(Collider other)
        {
            if (puzzleStep6 == null) return;
            // Đánh dấu player đang ở zone 1
            puzzleStep6.SetPlayerCurrentZone(ZoneIndex);
            Debug.Log("[IdTriggerZone] Player entered zone 1");
            // Gọi ghost chase player ngay khi vào zone
            var player = other.GetComponent<PlayerController_02>();
            if (player == null) return;
            foreach (var id in puzzleStep6.GetIds())
            {
                id.SetChaseTarget(player);
            }
            //DisableZone();
        }
    }
}