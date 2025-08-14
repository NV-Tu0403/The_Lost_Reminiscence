using UnityEngine;
using System.Collections.Generic; // Cần thiết để sử dụng List

public class PaintingPuzzleController : MonoBehaviour
{
    public static PaintingPuzzleController Instance { get; private set; }

    [Header("Puzzle Settings")]
    [Tooltip("Số lượng tranh đúng cần tìm để hoàn thành")]
    public int paintingsToWin = 3;

    [Header("References")]
    [Tooltip("Kéo đối tượng 'Exit Portal' vào đây")]
    public GameObject exitPortalObject;

    // Danh sách để lưu tất cả các khung tranh đã được giải
    private List<InteractablePainting> solvedPaintings = new List<InteractablePainting>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (exitPortalObject != null) exitPortalObject.SetActive(false);
        else Debug.LogError("Exit Portal Object is not assigned!");
    }

    // Khung tranh sẽ gọi hàm này để "đăng ký" khi được giải
    public void RegisterSolvedPainting(InteractablePainting painting)
    {
        if (!solvedPaintings.Contains(painting))
        {
            solvedPaintings.Add(painting);
        }

        // --- LOGIC KIỂM TRA ĐƯỢC CHUYỂN VÀO ĐÂY ---
        // Sử dụng solvedPaintings.Count thay cho biến đếm cũ
        Debug.Log($"Correct painting found! Progress: {solvedPaintings.Count} / {paintingsToWin}");

        if (solvedPaintings.Count >= paintingsToWin)
        {
            CompletePuzzle();
        }
    }
    
    // --- XÓA BỎ HOÀN TOÀN HÀM OnPaintingSolved() BỊ THỪA ---

    private void CompletePuzzle()
    {
        Debug.Log("PUZZLE COMPLETE! Triggering dissolve effect for all paintings.");

        foreach (var painting in solvedPaintings)
        {
            painting.StartDissolve();
        }

        if (exitPortalObject != null)
        {
            exitPortalObject.SetActive(true);
        }
    }

    public void ForceCompletePuzzle(string puzzleId)
    {
        Debug.Log($"Forcing puzzle completion for '{puzzleId}'. Activating portal.");
        CompletePuzzle();
    }
}