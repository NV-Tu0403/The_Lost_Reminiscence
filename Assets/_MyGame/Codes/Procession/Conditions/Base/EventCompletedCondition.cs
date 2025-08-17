using _MyGame.Codes.Procession;

namespace Script.Procession.Conditions
{
    public class EventCompletedCondition : Condition
    {
        private string _eventId;
        
        public EventCompletedCondition(string eventId)
        {
            _eventId = eventId;
        }
        
        public override bool IsSatisfied(object data)
        {
            return ProgressionManager.Instance.IsEventCompleted(_eventId);
        }
    }
}