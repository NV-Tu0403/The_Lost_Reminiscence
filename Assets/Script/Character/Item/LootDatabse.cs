using UnityEngine;
using System.Collections.Generic;
using DuckLe;
using System.Linq;

/// <summary>
/// Lớp quản lý cấu hình Loot.
/// </summary>
[CreateAssetMenu(fileName = "LootDatabse", menuName = "Loot/LootDatabase", order = 1)]
public class LootDatabase : ScriptableObject
{
    [System.Serializable]
    public class LootConfig
    {
        public string Name;
        public string Classify;
        public float Power;
        public float Range;
        public float UseSpeed;
        public float ReloadSpeed;
        public float Durability;
    }

    [SerializeField] private List<LootConfig> loots = new List<LootConfig>();


    // Phương thức để lấy cấu hình theo tên hoặc ID
    public LootConfig GetLootConfig(string LootName)
    {
        return loots.Find(w => w.Name == LootName);
    }

    // (Tùy chọn) Lấy theo index nếu muốn dùng ID thay vì tên
    public LootConfig GetLootConfig(int index)
    {
        if (index >= 0 && index < loots.Count)
            return loots[index];
        return null;
    }

    // lấy danh sách Name cho custom editor
    public string[] GetAllLootNames()
    {
        return loots.Select(w => w.Name).ToArray();
    }
}
