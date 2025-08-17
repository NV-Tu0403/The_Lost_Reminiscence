using UnityEngine;
using DunGen;
using DunGen.DungeonCrawler;
using System.Collections;

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

            // Bắt đầu Coroutine để gọi hàm dịch chuyển từ PlayerController
            StartCoroutine(TeleportPlayerWithController(spawnPosition));
        }
        else
        {
            Debug.LogError("LỖI: Dungeon đã tạo xong nhưng không tìm thấy 'PlayerSpawn' trong Tile bắt đầu!");
        }
    }

    // Coroutine mới để gọi hàm TeleportTo từ PlayerController
    private IEnumerator TeleportPlayerWithController(Vector3 spawnPosition)
    {
        // Chờ đến cuối frame để đảm bảo NavMesh đã được bake và đăng ký hoàn toàn
        yield return new WaitForEndOfFrame();

        PlayerController_02 playerController = playerToTeleport.GetComponent<PlayerController_02>();

        if (playerController != null)
        {
            // Yêu cầu player controller tự dịch chuyển chính nó
            playerController.TeleportTo(spawnPosition);
        }
        else
        {
            Debug.LogError("Không tìm thấy component 'PlayerController_02' trên Player. Không thể dịch chuyển!");
        }
    }
}