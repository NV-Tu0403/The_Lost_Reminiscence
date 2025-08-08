using System;
using UnityEngine;

namespace Script.Procession.Reward.Base
{
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
}