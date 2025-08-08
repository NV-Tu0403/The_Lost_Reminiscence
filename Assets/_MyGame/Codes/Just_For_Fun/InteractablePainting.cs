using UnityEngine;
using System.Collections;

public class InteractablePainting : MonoBehaviour, IInteractable
{
    [Header("Puzzle Logic")]
    [Tooltip("ID của Mảnh Tranh mà khung này cần để hoàn thành.")]
    public string requiredPictureID = "Default_ID";

    [Header("Effects")]
    [Tooltip("Hiệu ứng tan biến sẽ kéo dài trong bao lâu (giây)")]
    public float dissolveDuration = 1.5f;

    // Xóa biến GameObject ghostPrefab vì không cần nữa

    private Renderer frameRenderer;
    private Transform pictureSlot;
    private bool isSolved = false;

    private void Awake()
    {
        frameRenderer = GetComponent<Renderer>();
        pictureSlot = transform.Find("Cube");
        if (pictureSlot != null)
        {
            pictureSlot.gameObject.SetActive(false);
        }
    }

    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (isSolved || interactor == null) return;

        CollectiblePicture heldPicture = interactor.GetHeldPicture();

        if (heldPicture == null)
        {
            Debug.Log("Bạn đang không cầm mảnh tranh nào cả.");
            return;
        }

        // Logic kiểm tra và xử lý khi sai đã được chuyển sang CollectiblePicture
        // Khung tranh giờ chỉ quan tâm đến việc lắp đúng tranh
        if (heldPicture.pictureID == requiredPictureID)
        {
            Debug.Log("CHÍNH XÁC! Bạn đã lắp đúng mảnh tranh!");
            isSolved = true;
            PlaceAndSolve(heldPicture);
            interactor.ClearHeldPicture();
        }
        else
        {
            // Nếu người chơi cầm một mảnh tranh thật nhưng sai vị trí
            Debug.Log("Mảnh tranh này không khớp với khung này.");
            // Không trừ máu, chỉ thông báo để người chơi thử khung khác
        }
    }

    private void PlaceAndSolve(CollectiblePicture picture)
    {
        picture.transform.SetParent(pictureSlot.parent);
        picture.transform.position = pictureSlot.position;
        picture.transform.rotation = pictureSlot.rotation;
        picture.transform.localScale = pictureSlot.localScale;
        picture.gameObject.SetActive(true);

        if (PaintingPuzzleController.Instance != null)
        {
            PaintingPuzzleController.Instance.OnPaintingSolved();
        }
        StartCoroutine(DissolveEffect(picture.GetComponent<Renderer>()));
    }

    private IEnumerator DissolveEffect(Renderer pictureRenderer)
    {
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

        gameObject.SetActive(false);
    }
}