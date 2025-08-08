using UnityEngine;
using UnityEngine.AI;

public class PlayerPuzzleInteractor : MonoBehaviour
{
    [Header("Puzzle Stats")]
    public int puzzleHealth = 3;
    
    private IInteractable currentInteractable;
    private NavMeshAgent navMeshAgent;

    // --- PHẦN MỚI: "Bàn tay" của người chơi ---
    private CollectiblePicture heldPicture; // Lưu trữ mảnh tranh đang cầm

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }

    // --- CÁC HÀM QUẢN LÝ "BÀN TAY" MỚI ---
    public void PickupPicture(CollectiblePicture pictureToPickup)
    {
        if (heldPicture != null)
        {
            Debug.Log("Bạn đang cầm một mảnh tranh khác rồi!");
            return;
        }

        heldPicture = pictureToPickup;
        heldPicture.gameObject.SetActive(false); // Ẩn mảnh tranh khỏi thế giới
        Debug.Log($"Đã nhặt: Mảnh tranh '{heldPicture.pictureID}'");
    }

    public CollectiblePicture GetHeldPicture()
    {
        return heldPicture;
    }

    public void ClearHeldPicture(bool destroy = false)
    {
        if (heldPicture != null && destroy)
        {
            // Dùng khi lắp sai, mảnh tranh giả bị phá hủy
            Destroy(heldPicture.gameObject); 
        }
        heldPicture = null;
    }


    // Các hàm còn lại giữ nguyên...
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactableObject = other.GetComponent<IInteractable>();
        if (interactableObject != null)
        {
            currentInteractable = interactableObject;
            Debug.Log("Entered interactable zone: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentInteractable != null && other.gameObject == (currentInteractable as MonoBehaviour)?.gameObject)
        {
            currentInteractable = null;
            Debug.Log("Exited interactable zone.");
        }
    }
    
    public void TeleportPlayer(Vector3 destination)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
            transform.position = destination;
            navMeshAgent.enabled = true;
        }
        else
        {
            transform.position = destination;
        }
    }
    
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