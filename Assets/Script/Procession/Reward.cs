using System;
using UnityEngine;

/// <summary>
/// Lớp cơ sở cho phần thưởng.
/// </summary>
[Serializable]
public abstract class Reward
{
    public string Type; // Loại phần thưởng (Item, Experience,...)
    public abstract void Grant(); // Cấp phần thưởng
}

// Phần thưởng item (Weapon hoặc Loot)
[Serializable]
public class ItemReward : Reward
{
    public string ItemType; // "Weapon" hoặc "Loot CutScene"
    public string ItemName; // ID trong WeaponDatabase hoặc LootDatabase
    public int Amount;

    public override void Grant()
    {
        Debug.Log($"Granted {Amount} {ItemType} {ItemName}");
        // Gọi hệ thống kho để thêm item
        // Ví dụ: InventoryManager.Instance.AddItem(ItemType, ItemId, Amount);
    }
}

// Phần thưởng kinh nghiệm
[Serializable]
public class ExperienceReward : Reward
{
    public int Amount;

    public override void Grant()
    {
        Debug.Log($"Granted {Amount} experience");
        // Gọi hệ thống người chơi để thêm kinh nghiệm
        // Ví dụ: PlayerManager.Instance.AddExperience(Amount);
    }
}
