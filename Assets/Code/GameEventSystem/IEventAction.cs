namespace Script.GameEventSystem.EventAction
{
    public interface IEventAction
    {
        void Execute(BaseEventData data);
        void Finished(string eventId = null);
    }
}