using UnityEngine;

namespace DunGen.DungeonCrawler
{
    // Script này chỉ dùng để chạy các hàm xử lý hậu kỳ của DunGen
    // sau khi dungeon được tạo xong, ví dụ như spawn cửa.
    [RequireComponent(typeof(RuntimeDungeon))]
    public class DungeonSetup : MonoBehaviour
    {
        private RuntimeDungeon runtimeDungeon;

        private void OnEnable()
        {
            runtimeDungeon = GetComponent<RuntimeDungeon>();
            runtimeDungeon.Generator.OnGenerationStatusChanged += OnDungeonGenerationStatusChanged;
        }

        private void OnDisable()
        {
            // Luôn hủy đăng ký sự kiện để tránh lỗi
            if (runtimeDungeon != null && runtimeDungeon.Generator != null)
                runtimeDungeon.Generator.OnGenerationStatusChanged -= OnDungeonGenerationStatusChanged;
        }

        private void OnDungeonGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
        {
            // Chỉ chạy khi dungeon đã hoàn thành
            if (status != GenerationStatus.Complete)
                return;

            // Dòng này rất quan trọng, nó gọi lại các hệ thống hậu kỳ của DunGen,
            // bao gồm cả việc xử lý các vật thể có thể ẩn/hiện như cửa.
            // Chỉ cần gọi trực tiếp hàm này là đủ.
            HideableObject.RefreshHierarchies();
        }
    }
}