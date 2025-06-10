using UnityEngine;
using Script.Procession.Conditions;

namespace Script.Procession.Conditions.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EventCompletedConditionSO", menuName = "Progression/Condition/EventCompleted", order = 1)]
    public class EventCompletedConditionSO : ConditionSO
    {
        public string eventId;

        public override Condition ToCondition()
        {
            return new EventCompletedCondition(eventId);
        }
    }
}