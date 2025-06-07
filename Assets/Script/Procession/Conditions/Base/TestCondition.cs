using UnityEngine;

namespace Script.Procession.Conditions
{
    public class TestCondition : Condition
    {
        public override bool IsSatisfied(object data)
        {
            Debug.Log($"[TestCondition] IsSatisfied called with data: {data}");
            return true;
        }
    }
}