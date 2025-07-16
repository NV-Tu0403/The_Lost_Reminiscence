
using UnityEngine;
using UnityEditor; 

// Báo cho Unity biết script này dùng để vẽ lại Inspector cho script "DungeonTrigger"
[CustomEditor(typeof(DungeonTrigger))]
public class DungeonTriggerEditor : Editor
{
    // Hàm này sẽ vẽ lại toàn bộ giao diện cho component
    public override void OnInspectorGUI()
    {
        // Vẽ các trường public mặc định có sẵn (như trường dungeonGenerator)
        DrawDefaultInspector();

        // Lấy tham chiếu đến script "DungeonTrigger" đang được chọn
        DungeonTrigger myScript = (DungeonTrigger)target;

        // Tạo một khoảng trống cho đẹp mắt
        EditorGUILayout.Space();

        // Tạo một nút bấm với nhãn là "Tạo Dungeon Ngay!"
        // GUILayout.Button chỉ trả về true trong một frame khi nút được nhấn
        if (GUILayout.Button("Tạo Dungeon Ngay! (Trong Play Mode)"))
        {
            // Gọi hàm public GenerateDungeon() trên script đó
            myScript.GenerateDungeon();
        }
    }
}