using UnityEngine;

namespace Script.Procession.Conditions
{
    // ScriptableObject trừu tượng cho Condition
    public abstract class ConditionSO : ScriptableObject
    {
        public abstract Condition ToCondition();
    }
}