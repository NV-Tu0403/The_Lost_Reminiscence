using Events.Cutscene.Scripts;
using Script.GameEventSystem;
using Script.GameEventSystem.EventAction;
using UnityEngine;

namespace Script.Cutscene
{
    public class CutsceneAction : IEventAction
    {
        private string _eventIdCurrent;
        
        // Phương thức này sẽ được gọi khi bắt đầu một cutscene.
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[CutsceneAction] Starting cutscene for event: {data.eventId}");
            
            _eventIdCurrent = data.eventId;
            CutsceneManager.Instance.StartCutscene(data.eventId, () => Finished(data.eventId));
        }

        // Phương thức này sẽ được gọi khi cutscene kết thúc.
        public void Finished(string eventId = null)
        {
            Debug.Log($"[CutsceneAction] Finished event: {eventId}");  
            
            EventManager.Instance.OnEventFinished(eventId);
        }
    }
}