using Code.GameEventSystem;
using Script.GameEventSystem;
using Script.Procession;
using Unity.VisualScripting;
using UnityEngine;
using EventBus = Code.GameEventSystem.EventBus;

namespace Code.Dialogue
{
      
    public class DialogueAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[DialogueAction] Starting dialogue for event: {data.eventId}");

            DialogueManager.Instance.StartDialogue(data.eventId, () =>
            {
                Debug.Log($"[DialogueAction] Finished dialogue for event: {data.eventId}");
                EventBus.Publish(data.eventId, data);
            });
        }
    }
}