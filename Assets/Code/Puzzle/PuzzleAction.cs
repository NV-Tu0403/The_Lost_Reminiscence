using Code.GameEventSystem;
using Script.Puzzle;
using UnityEngine;

namespace Code.Puzzle
{
    public class PuzzleAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[PuzzleAction] Starting puzzle for event: {data.eventId}");
            
            PuzzleManager.Instance.StartPuzzle(data.eventId, () =>
            {
                Debug.Log($"[PuzzleAction] Finished puzzle for event: {data.eventId}");
                EventBus.Publish(data.eventId, data);
            });
        }
        
    }
}