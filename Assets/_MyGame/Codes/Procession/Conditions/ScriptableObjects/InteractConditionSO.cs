using Script.Procession.Conditions;
using UnityEngine;

// Điều kiện thu thập item
[CreateAssetMenu(fileName = "NewCollectItemCondition", menuName = "Progression/Condition/CollectItem")]
public class InteractConditionSO : ConditionSO
{
    public string TargetType;
    public string TargetName;
    public int RequiredAmount;

    public override Condition ToCondition()
    {
        return new InteractCondition
        {
            Type = "CollectItem",
            TargetType = TargetType,
            TargetName = TargetName,
            RequiredAmount = RequiredAmount
        };
    }
}



