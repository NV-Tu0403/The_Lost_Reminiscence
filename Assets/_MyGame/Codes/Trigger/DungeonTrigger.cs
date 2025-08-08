
using UnityEngine;
using DunGen;

public class DungeonTrigger : MonoBehaviour
{
    // Kéo GameObject chứa RuntimeDungeon vào đây trong Inspector
    public RuntimeDungeon dungeonGenerator;

    // Hàm này sẽ được gọi để bắt đầu tạo dungeon
    public void GenerateDungeon()
    {
        if (dungeonGenerator != null && Application.isPlaying)
        {
            dungeonGenerator.Generate();
        }
        else
        {
            if (dungeonGenerator == null)
                Debug.LogError("Chưa gán RuntimeDungeon component cho DungeonTrigger script!");
            if (!Application.isPlaying)
                Debug.LogWarning("Chỉ có thể tạo dungeon trong Play Mode.");
        }
    }
}