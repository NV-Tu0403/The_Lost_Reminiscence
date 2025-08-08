using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// chứa danh sách tất cả các cấu hình vũ khí.
/// Mỗi vũ khí chỉ cần chỉ định một ID hoặc tên để lấy cấu hình tương ứng.
/// </summary>
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Weapons/WeaponDatabase", order = 1)]
public class WeaponDatabase : ScriptableObject
{
    /// <summary>
    ///lớp con để lưu trữ cấu hình của mỗi vũ khí
    /// </summary>
    [System.Serializable]
    public class WeaponConfig
    {
        public string Name;
        public string Classify;
        public float Power;
        public float Range;
        public float UseSpeed;
        public float ReloadSpeed;
        public float Durability;
    }

    [SerializeField] private List<WeaponConfig> weapons = new List<WeaponConfig>(); // Danh sách tất cả vũ khí trong database.

    // Phương thức để lấy cấu hình theo tên hoặc ID
    public WeaponConfig GetWeaponConfig(string weaponName)
    {
        return weapons.Find(w => w.Name == weaponName);
    }

    // (Tùy chọn) Lấy theo index nếu muốn dùng ID thay vì tên
    public WeaponConfig GetWeaponConfig(int index)
    {
        if (index >= 0 && index < weapons.Count)
            return weapons[index];
        return null;
    }

    // lấy danh sách Name cho custom editor
    public string[] GetAllWeaponNames()
    {
        return weapons.Select(w => w.Name).ToArray();
    }
}
