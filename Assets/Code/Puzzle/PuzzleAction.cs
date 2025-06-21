using Script.GameEventSystem;
using Script.GameEventSystem.EventAction;
using UnityEngine;

namespace Script.Puzzle
{
    public class PuzzleAction : IEventAction
    {
        private string _eventIdCurrent;
        
        // Phương thức này sẽ được gọi khi bắt đầu một câu đố.
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[PuzzleAction] Starting puzzle for event: {data.eventId}");
            _eventIdCurrent = data.eventId;
            PuzzleManager.Instance.StartPuzzle(data.eventId, () => Finished(data.eventId));
        }

        // Phương thức này sẽ được gọi khi câu đố kết thúc.
        public void Finished(string eventId = null)
        {
            Debug.Log($"[PuzzleAction] Finished puzzle: {eventId}");
            
            // Khi Puzzle kết thúc, gọi ngược về EventManager
            EventManager.Instance.OnEventFinished(eventId);
        }
    }
}