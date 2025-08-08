using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; // Cần thiết để sử dụng List
using System.Linq; // Cần thiết để dùng .FirstOrDefault()

public class PlayerPuzzleInteractor : MonoBehaviour
{
    [Header("Puzzle Stats")]
    public int puzzleHealth = 3;
    
    private IInteractable currentInteractable;
    private NavMeshAgent navMeshAgent;

    // --- NÂNG CẤP TỪ "BÀN TAY" THÀNH "TÚI ĐỒ" ---
    // Sử dụng một List để lưu trữ tất cả các mảnh tranh thật đã nhặt
    private List<CollectiblePicture> collectedPictures = new List<CollectiblePicture>();

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

    // --- CÁC HÀM QUẢN LÝ "TÚI ĐỒ" ---
    public void PickupPicture(CollectiblePicture pictureToPickup)
    {
        collectedPictures.Add(pictureToPickup);
        pictureToPickup.gameObject.SetActive(false);
        Debug.Log($"Đã nhặt: Mảnh tranh '{pictureToPickup.pictureID}'. Tổng số tranh trong túi: {collectedPictures.Count}");
    }

    // Tìm và trả về một bức tranh khớp với ID yêu cầu
    public CollectiblePicture GetPictureByID(string pictureID)
    {
        return collectedPictures.FirstOrDefault(p => p.pictureID == pictureID);
    }

    // Xóa một bức tranh khỏi túi sau khi đã sử dụng
    public void RemovePicture(CollectiblePicture pictureToRemove)
    {
        if (pictureToRemove != null)
        {
            collectedPictures.Remove(pictureToRemove);
        }
    }
    
    // --- HÀM MỚI THEO YÊU CẦU CẢI TIẾN ---
    // Lấy một bức tranh thật BẤT KỲ từ trong túi đồ
    public CollectiblePicture GetAnyRealPicture()
    {
        // Lấy bức tranh đầu tiên tìm thấy trong túi đồ
        return collectedPictures.FirstOrDefault();
    }


    // Các hàm còn lại giữ nguyên...
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactableObject = other.GetComponent<IInteractable>();
        if (interactableObject != null)
        {
            currentInteractable = interactableObject;
            // Debug.Log("Entered interactable zone: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentInteractable != null && other.gameObject == (currentInteractable as MonoBehaviour)?.gameObject)
        {
            currentInteractable = null;
            // Debug.Log("Exited interactable zone.");
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