using Script.Procession.Reward.Base;
using UnityEngine;

namespace Script.Procession.Reward.ScriptableObjects
{
    /// <summary>
    /// Lớp cơ sở cho phần thưởng.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemReward", menuName = "Progression/Reward/Item")]
    public class ItemRewardSO : RewardSO
    {
        public string ItemType; // "Weapon" hoặc "Loot, CuScene"
        public string ItemName; // ID trong WeaponDatabase hoặc LootDatabase
        public int Amount;

        public override Base.Reward ToReward()
        {
            return new ItemReward
            {
                Type = "Item",
                ItemType = ItemType,
                ItemName = ItemName,
                Amount = Amount
            };
        }
    }
}