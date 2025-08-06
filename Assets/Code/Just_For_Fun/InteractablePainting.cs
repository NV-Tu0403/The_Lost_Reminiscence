using UnityEngine;
using System.Collections;

public class InteractablePainting : MonoBehaviour, IInteractable
{
    [Tooltip("ID của Mảnh Tranh mà khung này cần để hoàn thành.")]
    public string requiredPictureID = "Default_ID";

    [Tooltip("Hiệu ứng tan biến sẽ kéo dài trong bao lâu (giây)")]
    public float dissolveDuration = 1.5f;

    private Renderer frameRenderer;
    private Renderer pictureRenderer; // Renderer của Mảnh Tranh sẽ được đặt vào
    private Transform pictureSlot; // Vị trí để đặt Mảnh Tranh vào

    private bool isSolved = false;

    private void Awake()
    {
        frameRenderer = GetComponent<Renderer>();
        pictureSlot = transform.Find("Cube"); // Tìm vị trí con tên là "Cube"

        // Ban đầu, tắt hình ảnh mẫu đi
        if (pictureSlot != null)
        {
            pictureRenderer = pictureSlot.GetComponent<Renderer>();
            pictureSlot.gameObject.SetActive(false); 
        }
    }

    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (isSolved || interactor == null) return;

        // Lấy mảnh tranh mà người chơi đang cầm
        CollectiblePicture heldPicture = interactor.GetHeldPicture();

        // Nếu người chơi không cầm gì, không làm gì cả
        if (heldPicture == null)
        {
            Debug.Log("Bạn cần tìm một mảnh tranh để đặt vào đây.");
            return;
        }

        // --- KIỂM TRA ĐÚNG/SAI ---
        if (heldPicture.pictureID == requiredPictureID)
        {
            // ĐÚNG!
            Debug.Log("CHÍNH XÁC! Bạn đã lắp đúng mảnh tranh!");
            isSolved = true;
            PlaceAndSolve(heldPicture);
            interactor.ClearHeldPicture(); // Xóa mảnh tranh khỏi tay người chơi
        }
        else
        {
            // SAI!
            Debug.Log("SAI RỒI! Mảnh tranh này không khớp.");
            interactor.TakePuzzleDamage();
            interactor.ClearHeldPicture(true); // Xóa và phá hủy mảnh tranh giả
        }
    }

    private void PlaceAndSolve(CollectiblePicture picture)
    {
        // Hiện Mảnh Tranh lên và đặt nó vào đúng vị trí trong khung
        picture.transform.SetParent(pictureSlot.parent);
        picture.transform.position = pictureSlot.position;
        picture.transform.rotation = pictureSlot.rotation;
        picture.transform.localScale = pictureSlot.localScale;
        picture.gameObject.SetActive(true);

        // Lấy Renderer của Mảnh Tranh vừa đặt vào
        pictureRenderer = picture.GetComponent<Renderer>();

        // Thông báo cho PuzzleManager
        if (PaintingPuzzleController.Instance != null)
        {
            PaintingPuzzleController.Instance.OnPaintingSolved();
        }

        // Bắt đầu hiệu ứng tan biến
        StartCoroutine(DissolveEffect());
    }

    private IEnumerator DissolveEffect()
    {
        // Tắt Collider ngay lập tức để không tương tác lại được
        GetComponent<Collider>().enabled = false;
        
        Material frameMat = frameRenderer.material;
        Material pictureMat = pictureRenderer.material;
        float elapsedTime = 0f;

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveDuration);

            frameMat.SetFloat("_DissolveAmount", dissolveAmount);
            if (pictureMat != null)
            {
                pictureMat.SetFloat("_DissolveAmount", dissolveAmount);
            }
            yield return null;
        }
        
        // Sau khi hiệu ứng kết thúc, tắt hẳn GameObject đi
        gameObject.SetActive(false);
    }
}