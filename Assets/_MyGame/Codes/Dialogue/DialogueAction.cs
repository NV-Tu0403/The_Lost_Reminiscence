using _MyGame.Codes.GameEventSystem;

namespace _MyGame.Codes.Dialogue
{
    public class DialogueAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            data.OnFinish = FinishCallback;

            // Gửi sự kiện bắt đầu hội thoại
            EventBus.Publish("StartDialogue", data);
            return;

            void FinishCallback()
            {
                EventBus.Publish(data.eventId, data);
            }
        }
    }
}