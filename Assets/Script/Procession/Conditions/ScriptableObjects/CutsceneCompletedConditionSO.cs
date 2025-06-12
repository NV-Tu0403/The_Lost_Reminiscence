using UnityEngine;
using Script.Procession.Conditions;

namespace Script.Procession.Conditions.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CutsceneCompletedConditionSO", menuName = "Progression/Condition/CutsceneCompleted", order = 1)]
    public class CutsceneCompletedConditionSO : ConditionSO
    {
        public string cutsceneId;

        public override Condition ToCondition()
        {
            return new CutsceneCompletedCondition(cutsceneId);
        }
    }
}