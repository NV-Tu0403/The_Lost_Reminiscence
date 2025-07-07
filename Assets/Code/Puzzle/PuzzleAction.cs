using Code.GameEventSystem;
using Script.Puzzle;
using UnityEngine;

namespace Code.Puzzle
{
    public class PuzzleAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            PuzzleManager.Instance.StartPuzzle(data.eventId, () =>
            {
                EventBus.Publish(data.eventId, data);
            });
        }
    }
}