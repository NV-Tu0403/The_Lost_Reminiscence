using UnityEngine;

public class PaintingPuzzleController : MonoBehaviour
{
    // Sử dụng Singleton để các script khác dễ dàng truy cập
    public static PaintingPuzzleController Instance { get; private set; }

    [Header("Puzzle Settings")]
    [Tooltip("Số lượng tranh đúng cần tìm để hoàn thành")]
    public int paintingsToWin = 3;

    [Header("References")]
    [Tooltip("Kéo đối tượng 'Portal' (phần hiệu ứng) vào đây")]
    public GameObject portalVisualObject;

    private int correctPaintingsFound = 0;

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Ban đầu, hãy tắt portal đi
        if (portalVisualObject != null)
        {
            portalVisualObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Portal Visual Object is not assigned in the PaintingPuzzleController!");
        }
    }

    // Hàm này được gọi bởi mỗi bức tranh khi nó được giải đúng
    public void OnPaintingSolved()
    {
        correctPaintingsFound++;
        Debug.Log($"Correct painting found! Progress: {correctPaintingsFound} / {paintingsToWin}");

        if (correctPaintingsFound >= paintingsToWin)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        Debug.Log("PUZZLE COMPLETE! The portal is now active.");
        if (portalVisualObject != null)
        {
            portalVisualObject.SetActive(true);
        }
    }

    // --- HÀM MỚI ĐỂ SỬA LỖI ---
    // Hàm này được ProgressionManager gọi để đồng bộ trạng thái hoặc dùng trong dev mode.
    public void ForceCompletePuzzle(string puzzleId)
    {
        // Hiện tại chúng ta chỉ có 1 puzzle nên sẽ kích hoạt portal luôn.
        // puzzleId có thể dùng trong tương lai nếu bạn có nhiều puzzle khác nhau.
        Debug.Log($"Forcing puzzle completion for '{puzzleId}'. Activating portal.");
        CompletePuzzle();
    }
}