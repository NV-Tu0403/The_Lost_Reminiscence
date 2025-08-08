// File: PortalTeleporter.cs
using UnityEngine;

// Implement "hợp đồng" IInteractable
public class PortalTeleporter : MonoBehaviour, IInteractable
{
    [Tooltip("Kéo Empty Object 'Spawn Point' của bạn vào đây")]
    public Transform destinationPoint;

    // Hàm này sẽ được Player gọi khi tương tác
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