using Code.GameEventSystem;
using Events.Cutscene.Scripts;
using Script.GameEventSystem;
using UnityEngine;

namespace Code.Cutscene
{
    public class CutsceneAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[CutsceneAction] Starting cutscene for event: {data.eventId}");
            CutsceneManager.Instance.StartCutscene(data.eventId, () =>
            {
                Debug.Log($"[CutsceneAction] Finished cutscene for event: {data.eventId}");
                EventBus.Publish(data.eventId, data);
            });
        }
    }
}