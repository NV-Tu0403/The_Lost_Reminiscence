using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// chỉnh sửa ItemRewardSO trong Unity Editor.
/// </summary>
[CustomEditor(typeof(ItemRewardSO))]
public class ItemRewardSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var reward = (ItemRewardSO)target;
        var weaponDatabase = AssetDatabase.LoadAssetAtPath<WeaponDatabase>("Assets/Resources/Config/LootDatabse.asset");
        var lootDatabase = AssetDatabase.LoadAssetAtPath<LootDatabase>("Assets/Resources/Config/WeaponDatabase.asset");
        var cutsceneDatabase = AssetDatabase.LoadAssetAtPath<EventDatabase>("Assets/Resources/Config/EventDatabase.asset");

        // Chọn ItemType
        string[] itemTypes = { "Weapon", "Loot", "Cutscene" };
        int selectedType = Array.IndexOf(itemTypes, reward.ItemType);
        if (selectedType < 0) selectedType = 0;
        selectedType = EditorGUILayout.Popup("Item Type", selectedType, itemTypes);
        reward.ItemType = itemTypes[selectedType];

        // Chọn ItemId
        if (reward.ItemType == "Weapon" && weaponDatabase != null)
        {
            var itemIds = weaponDatabase.GetAllWeaponNames(); // lấy danh sách tên vũ khí từ WeaponDatabase
            int selectedIndex = Array.IndexOf(itemIds, reward.ItemName);
            selectedIndex = EditorGUILayout.Popup("Item Name", selectedIndex, itemIds);
            reward.ItemName = selectedIndex >= 0 ? itemIds[selectedIndex] : "";
        }
        else if (reward.ItemType == "Loot" && lootDatabase != null)
        {
            var itemIds = lootDatabase.GetAllLootNames(); // lấy danh sách tên loot từ LootDatabase
            int selectedIndex = Array.IndexOf(itemIds, reward.ItemName);
            selectedIndex = EditorGUILayout.Popup("Item Name", selectedIndex, itemIds);
            reward.ItemName = selectedIndex >= 0 ? itemIds[selectedIndex] : "";
        }
        else if (reward.ItemType == "Cutscene" && cutsceneDatabase != null)
        {
            var itemIds = cutsceneDatabase.GetAllEvenID(); // lấy danh sách ID cutscene từ EventDatabase
            int selectedIndex = Array.IndexOf(itemIds, reward.ItemName);
            selectedIndex = EditorGUILayout.Popup("Item Name", selectedIndex, itemIds);
            reward.ItemName = selectedIndex >= 0 ? itemIds[selectedIndex] : "";
        }
        else
        {
            reward.ItemName = EditorGUILayout.TextField("Item Name", reward.ItemName);
        }

        reward.Amount = EditorGUILayout.IntField("Amount", reward.Amount);
        EditorUtility.SetDirty(target);
    }
}

