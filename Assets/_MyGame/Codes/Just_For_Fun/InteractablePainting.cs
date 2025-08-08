using UnityEngine;
using System.Collections;

public class InteractablePainting : MonoBehaviour, IInteractable
{
    // Bỏ qua `requiredPictureID` vì không cần nữa, nhưng bạn có thể giữ lại nếu muốn dùng sau
    // [Header("Puzzle Logic")]
    // [Tooltip("ID của Mảnh Tranh mà khung này cần để hoàn thành.")]
    // public string requiredPictureID = "Default_ID";

    [Header("Effects")]
    [Tooltip("Hiệu ứng tan biến sẽ kéo dài trong bao lâu (giây)")]
    public float dissolveDuration = 1.5f;

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

    // --- HÀM INTERACT ĐÃ ĐƯỢC CẬP NHẬT THEO YÊU CẦU ---
    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (isSolved || interactor == null) return;

        // Lấy một bức tranh thật BẤT KỲ từ túi đồ của người chơi
        CollectiblePicture anyRealPicture = interactor.GetAnyRealPicture();

        if (anyRealPicture != null)
        {
            Debug.Log("Bạn đã lắp một mảnh tranh thật vào khung!");
            isSolved = true;
            
            PlaceAndSolve(anyRealPicture);
            interactor.RemovePicture(anyRealPicture);
        }
        else
        {
            Debug.Log("Bạn không có mảnh tranh thật nào để lắp vào đây.");
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