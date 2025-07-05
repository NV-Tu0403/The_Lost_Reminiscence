using System;
using Code.GameEventSystem;
using Events.Cutscene.Scripts;
using UnityEngine;

namespace Code.Cutscene
{
    public class CutsceneAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            //Debug.Log($"[CutsceneAction] Requesting cutscene for event: {data.eventId}");

            // Đặt callback khi cutscene kết thúc
            Action finishCallback = () => {
                EventBus.Publish(data.eventId, data);
            };
            data.onFinish = finishCallback;

            // Gửi sự kiện bắt đầu cutscene
            EventBus.Publish("StartCutscene", data);
        }
    }
}