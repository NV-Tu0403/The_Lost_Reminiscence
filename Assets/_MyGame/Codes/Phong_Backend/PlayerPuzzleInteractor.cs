using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class PlayerPuzzleInteractor : MonoBehaviour
{
    [Header("Puzzle Stats")]
    public int puzzleHealth = 3;
    
    private IInteractable currentInteractable;
    private NavMeshAgent navMeshAgent;

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

    public void PickupPicture(CollectiblePicture pictureToPickup)
    {
        FloatMovement floatScript = pictureToPickup.GetComponent<FloatMovement>();
        if (floatScript != null)
        {
            floatScript.enabled = false;
            Debug.Log("Đã tắt hiệu ứng lơ lửng của mảnh tranh.");
        }

        collectedPictures.Add(pictureToPickup);
        pictureToPickup.gameObject.SetActive(false);
        Debug.Log($"Đã nhặt: Mảnh tranh '{pictureToPickup.pictureID}'. Tổng số tranh trong túi: {collectedPictures.Count}");
    }

    public CollectiblePicture GetPictureByID(string pictureID)
    {
        return collectedPictures.FirstOrDefault(p => p.pictureID == pictureID);
    }

    public void RemovePicture(CollectiblePicture pictureToRemove)
    {
        if (pictureToRemove != null)
        {
            collectedPictures.Remove(pictureToRemove);
        }
    }
    
    public CollectiblePicture GetAnyRealPicture()
    {
        return collectedPictures.FirstOrDefault();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactableObject = other.GetComponent<IInteractable>();
        if (interactableObject != null)
        {
            currentInteractable = interactableObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Dòng code `?.gameObject` sẽ kiểm tra null an toàn trước khi truy cập
        if (currentInteractable != null && other.gameObject == (currentInteractable as MonoBehaviour)?.gameObject)
        {
            currentInteractable = null;
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

    public void TeleportToStart()
    {
        DungeonStartPoint startPoint = FindObjectOfType<DungeonStartPoint>();
        if (startPoint != null)
        {
            Debug.Log("Player has been caught! Teleporting to start...");
            TeleportPlayer(startPoint.transform.position);
        }
        else
        {
            Debug.LogError("Không tìm thấy DungeonStartPoint trong scene! Không thể dịch chuyển người chơi về điểm bắt đầu.");
        }
    }

    // --- HÀM MỚI ĐỂ SỬA LỖI ---
    // Hàm này cho phép các script khác xóa tham chiếu một cách an toàn
    public void ClearCurrentInteractable()
    {
        currentInteractable = null;
    }
}