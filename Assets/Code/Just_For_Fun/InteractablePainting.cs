// File: InteractablePainting.cs
using UnityEngine;

// Implement "hợp đồng" IInteractable
public class InteractablePainting : MonoBehaviour, IInteractable
{
    [Tooltip("Đánh dấu nếu đây là bức tranh đúng")]
    public bool isCorrectAnswer = false;

    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (interactor == null) return;

        if (isCorrectAnswer)
        {
            Debug.Log("CHÍNH XÁC! Bạn đã giải được câu đố!");
            
            if (PaintingPuzzleController.Instance != null)
            {
                PaintingPuzzleController.Instance.OnPaintingSolved();
            }

            // --- THAY ĐỔI Ở ĐÂY ---
            // Tắt toàn bộ GameObject (bao gồm cả khung và tranh con)
            // thay vì chỉ tắt MeshRenderer của khung.
            gameObject.SetActive(false);
            // --------------------
        }
        else
        {
            Debug.Log("SAI RỒI! Hãy thử lại.");
            interactor.TakePuzzleDamage();
        }
    }
}