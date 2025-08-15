using System;
using _MyGame.Codes.GameEventSystem;

namespace Code.Timeline
{
    public class TimelineAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            data.OnFinish = FinishCallback;

            // Gửi sự kiện bắt đầu timeline
            EventBus.Publish("StartTimeline", data);
            return;

            //Debug.Log($"[TimelineAction] Requesting timeline for event: {data.eventId}");
            // Đặt callback khi timeline kết thúc
            void FinishCallback()
            {
                EventBus.Publish(data.eventId, data);
            }
        }
    }
}