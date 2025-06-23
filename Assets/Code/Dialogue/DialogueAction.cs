using Script.GameEventSystem;
using Script.GameEventSystem.EventAction;
using Script.Procession;
using UnityEngine;

namespace Code.Dialogue
{
      
    public class DialogueAction : IEventAction
    {
        private string _eventIdCurrent;
        
        // Phương thức này sẽ được gọi khi bắt đầu một hội thoại.
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[DialogueAction] Starting dialogue for event: {data.eventId}");

            _eventIdCurrent = data.eventId;
            DialogueManager.Instance.StartDialogue(data.eventId, ()=> Finished(data.eventId));
        }


        // Phương thức này sẽ được gọi khi hội thoại kết thúc.
        public void Finished(string eventId)
        {
            Debug.Log($"[DialogueAction] Finished dialogue: {eventId}");

            // Khi UI Dialogue kết thúc, gọi ngược về EventManager
            EventManager.Instance.OnEventFinished(eventId);

            bool isProgressionComplete = ProgressionManager.Instance.CheckProcessCompletion(eventId, true);
        }
    }
}