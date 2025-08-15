using System;
using _MyGame.Codes.GameEventSystem;
using UnityEngine;

namespace Code.Cutscene
{
    public class CutsceneAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            Action finishCallback = () => {
                EventBus.Publish(data.eventId, data);
            };
            data.OnFinish = finishCallback;
            
            // Gửi sự kiện bắt đầu hội thoại
            EventBus.Publish("StartCutscene", data);
        }
    }
}