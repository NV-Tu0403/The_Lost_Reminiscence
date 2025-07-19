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
            //Debug.Log($"[DialogueAction] Starting dialogue for event: {data.eventId}");
            // Đặt callback khi hội thoại kết thúc
            Action finishCallback = () => {
                EventBus.Publish(data.eventId, data);
            };
            data.OnFinish = finishCallback;

            // Gửi sự kiện bắt đầu hội thoại
            EventBus.Publish("StartDialogue", data);
        }
    }
}