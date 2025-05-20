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
// Điều kiện tiêu diệt kẻ địch (đang lỗi)
[CreateAssetMenu(fileName = "NewDefeatEnemyCondition", menuName = "Progression/Condition/DefeatEnemy")]
public class DefeatEnemyConditionSO : ConditionSO
{
    public string EnemyType;
    public int RequiredAmount;

    public override Condition ToCondition()
    {
        return new DefeatEnemyCondition
        {
            Type = "DefeatEnemy",
            EnemyType = EnemyType,
            RequiredAmount = RequiredAmount
        };
    }
}

// ScriptableObject trừu tượng cho Condition
public abstract class ConditionSO : ScriptableObject
{
    public abstract Condition ToCondition();
}
