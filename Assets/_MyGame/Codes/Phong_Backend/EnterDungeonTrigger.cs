using UnityEngine;
using DunGen;
using DunGen.DungeonCrawler;
using UnityEngine.AI;

public class EnterDungeonTrigger : MonoBehaviour
{
    [Header("DunGen Settings")]
    [Tooltip("Kéo GameObject chứa component RuntimeDungeon vào đây.")]
    public RuntimeDungeon dungeonGenerator;

    [Header("Player Settings")]
    [Tooltip("Kéo GameObject Player vào đây, hoặc để trống để script tự tìm theo tag 'Player'.")]
    public GameObject playerToTeleport; // Biến này vẫn còn để bạn có thể gán thủ công nếu muốn

    private bool hasBeenTriggered = false;

    // Sử dụng hàm Awake() để tìm người chơi ngay khi scene bắt đầu
    private void Awake()
    {
        // Nếu playerToTeleport chưa được gán trong Inspector
        if (playerToTeleport == null)
        {
            // Tự động tìm GameObject có tag là "Player"
            playerToTeleport = GameObject.FindGameObjectWithTag("Player");

            if (playerToTeleport != null)
            {
                Debug.Log("EnterDungeonTrigger đã tự động tìm thấy Player: " + playerToTeleport.name);
            }
            else
            {
                // Báo lỗi nếu không tìm thấy để bạn biết và sửa
                Debug.LogError("EnterDungeonTrigger không thể tự động tìm thấy Player! Hãy chắc chắn nhân vật của bạn có tag là 'Player'.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có phải người chơi đã đi vào không
        if (playerToTeleport != null && other.gameObject == playerToTeleport && !hasBeenTriggered)
        {
            Debug.Log("Player đã đi vào TriggerZone. Bắt đầu tạo Dungeon...");

            hasBeenTriggered = true;
            GetComponent<Collider>().enabled = false;

            dungeonGenerator.Generator.OnGenerationStatusChanged += OnDungeonGenerated;
            dungeonGenerator.Generate();
        }
    }

    private void OnDungeonGenerated(DungeonGenerator generator, GenerationStatus status)
    {
        if (status != GenerationStatus.Complete)
        {
            return;
        }

        Debug.Log("Dungeon đã tạo xong! Chuẩn bị dịch chuyển người chơi.");
        generator.OnGenerationStatusChanged -= OnDungeonGenerated;

        // Đoạn code dịch chuyển người chơi giữ nguyên, không cần thay đổi
        Tile startTile = generator.CurrentDungeon.MainPathTiles[0];
        PlayerSpawn playerSpawnPoint = startTile.GetComponentInChildren<PlayerSpawn>();

        if (playerSpawnPoint != null)
        {
            Vector3 spawnPosition = playerSpawnPoint.transform.position;
            Debug.Log("Đã tìm thấy PlayerSpawn tại vị trí: " + spawnPosition);

            NavMeshAgent agent = playerToTeleport.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                bool success = agent.Warp(spawnPosition);
                if (success)
                    Debug.Log("Đã dịch chuyển người chơi bằng NavMeshAgent.Warp()!");
                else
                    Debug.LogError("NavMeshAgent.Warp() thất bại! Vị trí spawn có thể không nằm trên NavMesh.");
                return;
            }

            CharacterController cc = playerToTeleport.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                playerToTeleport.transform.position = spawnPosition;
                cc.enabled = true;
                Debug.Log("Đã dịch chuyển người chơi bằng cách tắt/bật CharacterController!");
                return;
            }

            playerToTeleport.transform.position = spawnPosition;
            Debug.Log("Đã dịch chuyển người chơi bằng transform.position!");
        }
        else
        {
            Debug.LogError("LỖI: Dungeon đã tạo xong nhưng không tìm thấy 'PlayerSpawn' trong Tile bắt đầu!");
        }
    }
}