using UnityEngine;

public class CollectiblePicture : MonoBehaviour, IInteractable
{
    [Tooltip("ID định danh cho bức tranh này. Phải khớp với ID của Khung Tranh tương ứng.")]
    public string pictureID = "Default_ID";

    // Hàm này được Player gọi khi tương tác (nhặt)
    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (interactor != null)
        {
            // Báo cho Player biết để nhặt mảnh tranh này
            interactor.PickupPicture(this);
        }
    }
}