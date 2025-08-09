using UnityEngine;
using DunGen;
using DunGen.DungeonCrawler;
using UnityEngine.AI; // THÊM DÒNG NÀY ĐỂ SỬ DỤNG NAVMESHAGENT

public class EnterDungeonTrigger : MonoBehaviour
{
    [Header("DunGen Settings")]
    [Tooltip("Kéo GameObject chứa component RuntimeDungeon vào đây.")]
    public RuntimeDungeon dungeonGenerator; // Tham chiếu đến trình tạo Dungeon

    [Header("Player Settings")]
    [Tooltip("Kéo GameObject Player có sẵn trong Scene của bạn vào đây.")]
    public GameObject playerToTeleport; // Tham chiếu đến nhân vật của bạn

    private bool hasBeenTriggered = false; // Một biến để đảm bảo trigger chỉ chạy một lần


    // Hàm này được Unity tự động gọi khi có một vật thể khác đi vào Collider (đã bật "Is Trigger")
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vật thể đi vào có phải là người chơi không và trigger chưa được kích hoạt
        if (other.gameObject == playerToTeleport && !hasBeenTriggered)
        {
            Debug.Log("Player đã đi vào TriggerZone. Bắt đầu tạo Dungeon...");

            // Đánh dấu là đã kích hoạt để không chạy lại code này
            hasBeenTriggered = true;

            // Vô hiệu hóa Collider này để tránh kích hoạt lại
            GetComponent<Collider>().enabled = false;

            // "Lắng nghe" sự kiện khi trạng thái của việc tạo dungeon thay đổi
            dungeonGenerator.Generator.OnGenerationStatusChanged += OnDungeonGenerated;

            // Bắt đầu quá trình tạo dungeon
            dungeonGenerator.Generate();
        }
    }

    // Hàm này sẽ được gọi bởi sự kiện OnGenerationStatusChanged
    private void OnDungeonGenerated(DungeonGenerator generator, GenerationStatus status)
    {
        // Chúng ta chỉ quan tâm khi dungeon đã được tạo XONG
        if (status != GenerationStatus.Complete)
        {
            return; // Nếu chưa xong, không làm gì cả
        }

        Debug.Log("Dungeon đã tạo xong! Chuẩn bị dịch chuyển người chơi.");

        // Ngừng "lắng nghe" sự kiện để tránh lỗi hoặc gọi lại không cần thiết
        generator.OnGenerationStatusChanged -= OnDungeonGenerated;

        // Tìm điểm spawn trong phòng đầu tiên của dungeon
        // Đây là cách chính xác để lấy tile đầu tiên trên con đường chính (Main Path)
        Tile startTile = generator.CurrentDungeon.MainPathTiles[0];
        PlayerSpawn playerSpawnPoint = startTile.GetComponentInChildren<PlayerSpawn>();

        // Nếu tìm thấy điểm spawn
        if (playerSpawnPoint != null)
        {
            Vector3 spawnPosition = playerSpawnPoint.transform.position;
            Debug.Log("Đã tìm thấy PlayerSpawn tại vị trí: " + spawnPosition);

            // --- LOGIC DỊCH CHUYỂN ĐÃ ĐƯỢC NÂNG CẤP ---

            // Ưu tiên dịch chuyển bằng NavMeshAgent nếu có
            NavMeshAgent agent = playerToTeleport.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                // Warp là cách dịch chuyển NavMeshAgent tức thời và an toàn nhất
                bool success = agent.Warp(spawnPosition); 
                if(success)
                    Debug.Log("Đã dịch chuyển người chơi bằng NavMeshAgent.Warp()!");
                else
                    Debug.LogError("NavMeshAgent.Warp() thất bại! Vị trí spawn có thể không nằm trên NavMesh.");

                return; // Kết thúc hàm sau khi dịch chuyển xong
            }

            // Nếu không có NavMeshAgent, thử với CharacterController
            CharacterController cc = playerToTeleport.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false; // Tắt controller
                playerToTeleport.transform.position = spawnPosition; // Dịch chuyển
                cc.enabled = true;  // Bật lại controller
                Debug.Log("Đã dịch chuyển người chơi bằng cách tắt/bật CharacterController!");
                return;
            }

            // Nếu không có cả hai, dùng cách dịch chuyển thông thường
            playerToTeleport.transform.position = spawnPosition;
            Debug.Log("Đã dịch chuyển người chơi bằng transform.position!");
        }
        else
        {
            Debug.LogError("LỖI: Dungeon đã tạo xong nhưng không tìm thấy 'PlayerSpawn' trong Tile bắt đầu!");
        }
    }
}