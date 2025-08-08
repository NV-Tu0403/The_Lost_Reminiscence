namespace Script.Procession.Conditions
{
    public class DefeatEnemyCondition : Condition
    {
        public string EnemyType;
        public int RequiredAmount;
        
        public override bool IsSatisfied(object data)
        {
            if (data is (string enemyType, int amount))
            {
                return enemyType == EnemyType && amount >= RequiredAmount;
            }
            return false;
        }
    }
}