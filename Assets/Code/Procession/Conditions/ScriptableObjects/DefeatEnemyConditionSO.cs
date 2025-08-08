using UnityEngine;

namespace Script.Procession.Conditions
{
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
}