using System;
using Code.GameEventSystem;
using UnityEngine;

namespace Code.Cutscene
{
    public class CutsceneAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            data.OnFinish = FinishCallback;

            // Gửi sự kiện bắt đầu cutscene
            EventBus.Publish("StartCutscene", data);
            return;

            //Debug.Log($"[CutsceneAction] Requesting cutscene for event: {data.eventId}");
            // Đặt callback khi cutscene kết thúc
            void FinishCallback()
            {
                EventBus.Publish(data.eventId, data);
            }
        }
    }
}