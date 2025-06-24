using Script.GameEventSystem;

namespace Code.GameEventSystem
{
    public interface IEventAction
    {
        void Execute(BaseEventData data);
        void Finished(string eventId = null);
    }
}