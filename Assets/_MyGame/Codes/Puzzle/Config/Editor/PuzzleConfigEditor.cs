#if UNITY_EDITOR
using Script.Puzzle.Config.SO;
using UnityEditor;
using UnityEngine;

namespace Script.Puzzle.Config.Editor
{
    [CustomEditor(typeof(PuzzleConfigSO))]
    public class PuzzleConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Title lớn màu trắng
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 16;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("CẤU HÌNH THÔNG SỐ PUZZLE BRIDGE", titleStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(10);

            // Vẽ lại các field mặc định
            DrawDefaultInspector();
        }
    }
}
#endif