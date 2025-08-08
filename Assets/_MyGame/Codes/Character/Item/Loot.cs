using Duckle;
using System.Collections.Generic;
using UnityEngine;

namespace DuckLe
{
    /// <summary>
    /// Lớp cụ thể cho Loot.
    /// Gán vào prefab Loot trong impector.
    /// 
    /// Lưu ý: Tên Loot trong prefab phải trùng với tên trong LootDatabase để lấy cấu hình chính xác.
    /// </summary>
    public class Loot : Item
    {
        [SerializeField] private LootDatabase database; // Tham chiếu đến LootDatabase
        [SerializeField] private string LootName;       // Tên Loot để tra cứu trong database

        protected virtual void Awake()
        {
            if (database == null)
            {
                Debug.LogError("LootDatabase is not assigned in the Inspector for " + gameObject.name);
                return;
            }

            LootDatabase.LootConfig config = database.GetLootConfig(LootName);
            if (config == null)
            {
                Debug.LogError($"Loot config for '{LootName}' not found in LootDatabase!");
                return;
            }

            // Lấy dữ liệu từ config
            Name = config.Name;
            Classify = config.Classify;
            Power = config.Power;
            Range = config.Range;
            UseSpeed = config.UseSpeed;
            ReloadSpeed = config.ReloadSpeed;
            Durability = config.Durability;
        }

        /// <summary>
        /// Phương thức sử dụng Loot.
        /// </summary>
        /// <param name="user"></param>
        public override void OnUse(PlayerController user)
        {
            //Durability -= 1f; // Giảm độ bền khi sử dụng
            if (Durability <= 0)
            {
                Debug.Log($"{Name} has broken!");
            }
        }

        /// <summary>
        /// Phương thức áp dụng nâng cấp cho Loot.
        /// </summary>
        /// <param name="upgrade"></param>
        public override void ApplyUpgrade(Upgrade upgrade)
        {
            Power = upgrade.PowerOverride ?? Power; // Nếu không có giá trị mới, giữ nguyên giá trị cũ
            Range = upgrade.RangeOverride ?? Range;
            UseSpeed = upgrade.UseSpeedOverride ?? UseSpeed;
            ReloadSpeed = upgrade.ReloadSpeedOverride ?? ReloadSpeed;
            Durability = upgrade.DurabilityOverride ?? Durability;
        }
    }
}

