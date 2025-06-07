using UnityEngine;

namespace Script.Procession.Conditions.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TestConditionSO", menuName = "Procession/Conditions/TestConditionSO")]
    public class TestConditionSO : ConditionSO
    {
        public override Condition ToCondition()
        {
            return new TestCondition();
        }
    }
}