using DuckLe;
using System.Collections.Generic;
using UnityEngine;

namespace Duckle
{
    /// <summary>
    /// Lớp cụ thể cho vũ khí.
    /// Gán vào prefab vũ khí trong impector.
    /// 
    /// Lưu ý: Tên vũ khí trong prefab phải trùng với tên trong WeaponDatabase để lấy cấu hình chính xác.
    /// </summary>
    public class Weapon : Item
    {
        [SerializeField] private WeaponDatabase database; // Tham chiếu đến WeaponDatabase
        [SerializeField] private string weaponName;       // Tên vũ khí để tra cứu trong database

        protected virtual void Awake()
        {
            if (database == null)
            {
                Debug.LogError("WeaponDatabase is not assigned in the Inspector for " + gameObject.name);
                return;
            }

            WeaponDatabase.WeaponConfig config = database.GetWeaponConfig(weaponName);
            if (config == null)
            {
                Debug.LogError($"Weapon config for '{weaponName}' not found in WeaponDatabase!");
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
        /// Phương thức sử dụng vũ khí.
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
        /// Phương thức áp dụng nâng cấp cho vũ khí.
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