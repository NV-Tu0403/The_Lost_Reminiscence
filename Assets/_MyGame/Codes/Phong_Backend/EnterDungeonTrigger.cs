using UnityEngine;
using DunGen;
using DunGen.DungeonCrawler;
using UnityEngine.AI;
using System.Collections; // Cần thêm namespace này để sử dụng Coroutine

public class EnterDungeonTrigger : MonoBehaviour
{
    [Header("DunGen Settings")]
    [Tooltip("Kéo GameObject chứa component RuntimeDungeon vào đây.")]
    public RuntimeDungeon dungeonGenerator;

    [Header("Player Settings")]
    [Tooltip("Kéo GameObject Player vào đây, hoặc để trống để script tự tìm theo tag 'Player'.")]
    public GameObject playerToTeleport;

    private bool hasBeenTriggered = false;

    private void Awake()
    {
        if (playerToTeleport == null)
        {
            playerToTeleport = GameObject.FindGameObjectWithTag("Player");

            if (playerToTeleport != null)
            {
                Debug.Log("EnterDungeonTrigger đã tự động tìm thấy Player: " + playerToTeleport.name);
            }
            else
            {
                Debug.LogError("EnterDungeonTrigger không thể tự động tìm thấy Player! Hãy chắc chắn nhân vật của bạn có tag là 'Player'.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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

        Tile startTile = generator.CurrentDungeon.MainPathTiles[0];
        PlayerSpawn playerSpawnPoint = startTile.GetComponentInChildren<PlayerSpawn>();

        if (playerSpawnPoint != null)
        {
            Vector3 spawnPosition = playerSpawnPoint.transform.position;
            Debug.Log("Đã tìm thấy PlayerSpawn tại vị trí: " + spawnPosition);

            // Bắt đầu một Coroutine để dịch chuyển người chơi sau một khoảng trễ nhỏ
            StartCoroutine(TeleportPlayerCoroutine(spawnPosition));
        }
        else
        {
            Debug.LogError("LỖI: Dungeon đã tạo xong nhưng không tìm thấy 'PlayerSpawn' trong Tile bắt đầu!");
        }
    }

    // Coroutine để xử lý dịch chuyển
    private IEnumerator TeleportPlayerCoroutine(Vector3 spawnPosition)
    {
        // Chờ đến cuối frame hiện tại để đảm bảo NavMesh đã được đăng ký hoàn toàn
        yield return new WaitForEndOfFrame();

        NavMeshAgent agent = playerToTeleport.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Tắt agent đi trước khi warp để tránh lỗi, sau đó bật lại
            agent.enabled = false;
            playerToTeleport.transform.position = spawnPosition;
            agent.enabled = true;

            // Kiểm tra lại xem agent có thực sự nằm trên NavMesh không sau khi di chuyển
            if (agent.isOnNavMesh)
            {
                Debug.Log("Đã dịch chuyển người chơi thành công và agent đang ở trên NavMesh!");
            }
            else
            {
                Debug.LogError("Dịch chuyển thất bại, người chơi không nằm trên NavMesh sau khi di chuyển. Hãy kiểm tra lại vị trí PlayerSpawn và cấu hình NavMesh.");
            }
            yield break; // Thoát khỏi coroutine
        }

        // Fallback cho trường hợp không có NavMeshAgent
        CharacterController cc = playerToTeleport.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            playerToTeleport.transform.position = spawnPosition;
            cc.enabled = true;
            Debug.Log("Đã dịch chuyển người chơi bằng cách tắt/bật CharacterController!");
            yield break;
        }

        playerToTeleport.transform.position = spawnPosition;
        Debug.Log("Đã dịch chuyển người chơi bằng transform.position!");
    }
}