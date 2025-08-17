using UnityEngine;

public class PortalTeleporter : MonoBehaviour, IInteractable
{
    // Chúng ta vẫn giữ biến này để có thể gán thủ công nếu muốn,
    // nhưng giờ nó sẽ được tự động tìm kiếm.
    public Transform destinationPoint;

    // Sử dụng hàm Start() để tự động tìm điểm đến khi portal xuất hiện
    private void Start()
    {
        // Nếu destinationPoint chưa được gán trong Inspector
        if (destinationPoint == null)
        {
            // Tìm đối tượng đầu tiên trong scene có gắn script PortalDestination
            PortalDestination destinationObject = FindObjectOfType<PortalDestination>();

            if (destinationObject != null)
            {
                // Nếu tìm thấy, lấy transform của nó làm điểm đến
                destinationPoint = destinationObject.transform;
                Debug.Log($"Portal '{gameObject.name}' đã tự động tìm thấy điểm đến: '{destinationPoint.name}'.");
            }
            else
            {
                // Nếu không tìm thấy, báo lỗi để bạn biết
                Debug.LogError($"Portal '{gameObject.name}' không thể tìm thấy bất kỳ đối tượng nào có script 'PortalDestination' trong scene!");
            }
        }
    }

    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (destinationPoint != null && interactor != null)
        {
            Debug.Log($"Portal activated! Teleporting player to {destinationPoint.position}");
            interactor.TeleportPlayer(destinationPoint.position);
        }
        else
        {
            Debug.LogError("Destination Point for portal is not set!");
        }
    }
}