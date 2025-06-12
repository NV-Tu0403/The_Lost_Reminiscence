#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Events.Puzzle.SO
{
    [CustomEditor(typeof(PuzzleConfigSO))]
    public class PuzzleConfigEditor : Editor
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