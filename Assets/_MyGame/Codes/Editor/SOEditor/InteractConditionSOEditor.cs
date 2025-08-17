using System;
using _MyGame.Codes.GameEventSystem;
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
        var lootDatabase = AssetDatabase.LoadAssetAtPath<LootDatabase>("Assets/Resources/Config/WeaponDatabase.asset");
        var cutsceneDatabase = AssetDatabase.LoadAssetAtPath<EventDatabase>("Assets/Resources/Config/EventDatabase.asset");

        // Chọn TargetType
        string[] TagetTypes = {"Loot", "Cutscene" };
        int selectedType = Array.IndexOf(TagetTypes, condition.TargetType);
        if (selectedType < 0) selectedType = 0;
        selectedType = EditorGUILayout.Popup("Target Type", selectedType, TagetTypes);
        condition.TargetType = TagetTypes[selectedType];

        // Chọn TagetId
        if (condition.TargetType == "Loot" && lootDatabase != null)
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