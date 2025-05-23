// Phần thưởng item
using UnityEngine;

/// <summary>
/// Lớp cơ sở cho phần thưởng.
/// </summary>
[CreateAssetMenu(fileName = "NewItemReward", menuName = "Progression/Reward/Item")]
public class ItemRewardSO : RewardSO
{
    public string ItemType; // "Weapon" hoặc "Loot, CuScene"
    public string ItemName; // ID trong WeaponDatabase hoặc LootDatabase
    public int Amount;

    public override Reward ToReward()
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

// Phần thưởng kinh nghiệm (đang lỗi)
[CreateAssetMenu(fileName = "NewExperienceReward", menuName = "Progression/Reward/Experience")]
public class ExperienceRewardSO : RewardSO
{
    public int Amount;

    public override Reward ToReward()
    {
        return new ExperienceReward
        {
            Type = "Experience",
            Amount = Amount
        };
    }
}

// ScriptableObject trừu tượng cho Reward
public abstract class RewardSO : ScriptableObject
{
    public abstract Reward ToReward();
}