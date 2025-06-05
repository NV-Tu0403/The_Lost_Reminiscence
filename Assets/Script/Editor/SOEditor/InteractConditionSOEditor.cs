using System;
using Script.GameEventSystem;
using UnityEditor;
using UnityEngine;

/// <summary>
/// chỉnh sửa InteractCondition trong Unity Editor.
/// </summary>
[CustomEditor(typeof(InteractConditionSO))]
public class InteractConditionSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var condition = (InteractConditionSO)target;
        var weaponDatabase = AssetDatabase.LoadAssetAtPath<WeaponDatabase>("Assets/Resources/Config/LootDatabse.asset");
        var lootDatabase = AssetDatabase.LoadAssetAtPath<LootDatabase>("Assets/Resources/Config/WeaponDatabase.asset");
        var cutsceneDatabase = AssetDatabase.LoadAssetAtPath<EventDatabase>("Assets/Resources/Config/EventDatabase.asset");

        // Chọn TargetType
        string[] TagetTypes = { "Weapon", "Loot", "Cutscene" };
        int selectedType = Array.IndexOf(TagetTypes, condition.TargetType);
        if (selectedType < 0) selectedType = 0;
        selectedType = EditorGUILayout.Popup("Target Type", selectedType, TagetTypes);
        condition.TargetType = TagetTypes[selectedType];

        // Chọn TagetId
        if (condition.TargetType == "Weapon" && weaponDatabase != null)
        {
            var TagetIds = weaponDatabase.GetAllWeaponNames(); // lấy danh sách tên vũ khí từ WeaponDatabase
            int selectedIndex = Array.IndexOf(TagetIds, condition.TargetName);
            selectedIndex = EditorGUILayout.Popup("Target Name", selectedIndex, TagetIds);
            condition.TargetName = selectedIndex >= 0 ? TagetIds[selectedIndex] : "";
        }
        else if (condition.TargetType == "Loot" && lootDatabase != null)
        {
            var TagetIds = lootDatabase.GetAllLootNames(); // lấy danh sách tên loot từ LootDatabase
            int selectedIndex = Array.IndexOf(TagetIds, condition.TargetName);
            selectedIndex = EditorGUILayout.Popup("Target Name", selectedIndex, TagetIds);
            condition.TargetName = selectedIndex >= 0 ? TagetIds[selectedIndex] : "";
        }else if (condition.TargetType == "Cutscene" && cutsceneDatabase != null)
        {
            var TagetIds = cutsceneDatabase.GetAllEvenID(); // lấy danh sách ID cutscene từ EventDatabase
            int selectedIndex = Array.IndexOf(TagetIds, condition.TargetName);
            selectedIndex = EditorGUILayout.Popup("Target Name", selectedIndex, TagetIds);
            condition.TargetName = selectedIndex >= 0 ? TagetIds[selectedIndex] : "";
        }
        else
        {
            condition.TargetName = EditorGUILayout.TextField("Target Name", condition.TargetName);
        }
        condition.RequiredAmount = EditorGUILayout.IntField("Required Amount", condition.RequiredAmount);
        EditorUtility.SetDirty(target);
    }
}