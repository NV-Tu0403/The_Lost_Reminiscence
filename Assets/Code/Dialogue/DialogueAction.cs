using System;
using Code.GameEventSystem;
using UnityEngine;
using EventBus = Code.GameEventSystem.EventBus;

namespace Code.Dialogue
{
    public class DialogueAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            Action finishCallback = () => { EventBus.Publish(data.eventId, data); };
            data.OnFinish = finishCallback;

            // Gửi sự kiện bắt đầu hội thoại
            EventBus.Publish("StartDialogue", data);
        }
    }
}