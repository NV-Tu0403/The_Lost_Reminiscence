namespace Code.GameEventSystem
{
    public interface IEventAction
    {
        void Execute(BaseEventData data);
    }
}