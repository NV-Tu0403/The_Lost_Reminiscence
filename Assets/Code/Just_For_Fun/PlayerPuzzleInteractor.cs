// File: PlayerPuzzleInteractor.cs
using UnityEngine;
using UnityEngine.AI; // Cần thiết để dùng NavMeshAgent

public class PlayerPuzzleInteractor : MonoBehaviour
{
    [Header("Puzzle Stats")]
    public int puzzleHealth = 3;

    // THAY ĐỔI: Giờ đây có thể lưu bất kỳ vật thể nào implement IInteractable
    private IInteractable currentInteractable;

    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        // Lấy component NavMeshAgent để dịch chuyển an toàn
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Logic nhấn phím không đổi, nhưng giờ nó hoạt động với cả portal
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }

    // --- LOGIC TRIGGER MỚI, NẰM TRÊN PLAYER ---
    private void OnTriggerEnter(Collider other)
    {
        // Thử lấy "hợp đồng" IInteractable từ vật thể va chạm
        IInteractable interactableObject = other.GetComponent<IInteractable>();

        // Nếu vật thể đó có thể tương tác
        if (interactableObject != null)
        {
            currentInteractable = interactableObject;
            Debug.Log("Entered interactable zone: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Kiểm tra xem có phải vừa đi ra khỏi vùng tương tác hiện tại không
        if (currentInteractable != null && other.gameObject == (currentInteractable as MonoBehaviour).gameObject)
        {
            currentInteractable = null;
            Debug.Log("Exited interactable zone.");
        }
    }

    // --- HÀM DỊCH CHUYỂN MỚI ---
    public void TeleportPlayer(Vector3 destination)
    {
        // Dùng NavMeshAgent.Warp() là cách an toàn và tốt nhất để dịch chuyển
        // nhân vật có dùng NavMesh, tránh bị lỗi và giật lag.
        if (navMeshAgent != null)
        {
            // Tắt agent đi để thay đổi vị trí
            navMeshAgent.enabled = false;
            // Di chuyển transform
            transform.position = destination;
            // Bật lại agent
            navMeshAgent.enabled = true;
            Debug.Log($"Player teleported to {destination}");
        }
        else // Fallback nếu không có NavMeshAgent
        {
            transform.position = destination;
        }
    }

    // Hàm nhận sát thương không đổi
    public void TakePuzzleDamage()
    {
        puzzleHealth--;
        Debug.LogWarning("CHỌN SAI! Máu giải đố còn lại: " + puzzleHealth);

        if (puzzleHealth <= 0)
        {
            Debug.LogError("GAME OVER! Bạn đã hết máu giải đố.");
        }
    }
}