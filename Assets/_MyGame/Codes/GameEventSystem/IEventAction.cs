namespace _MyGame.Codes.GameEventSystem
{
    public interface IEventAction
    {
        void Execute(BaseEventData data);
    }
}