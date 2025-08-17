using UnityEngine;
using System.Collections;

public class InteractablePainting : MonoBehaviour, IInteractable
{
    [Header("Effects")]
    [Tooltip("Hiệu ứng tan biến sẽ kéo dài trong bao lâu (giây)")]
    public float dissolveDuration = 1.5f;

    private Renderer frameRenderer;
    private Transform pictureSlot;
    private Renderer pictureRenderer;
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

        CollectiblePicture anyRealPicture = interactor.GetAnyRealPicture();

        if (anyRealPicture != null)
        {
            Debug.Log("Bạn đã lắp một mảnh tranh thật vào khung!");
            isSolved = true;
            
            GetComponent<Collider>().enabled = false;
            
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
        
        pictureRenderer = picture.GetComponent<Renderer>();

        if (PaintingPuzzleController.Instance != null)
        {
            // Tự "đăng ký" với Puzzle Controller
            PaintingPuzzleController.Instance.RegisterSolvedPainting(this);
            
            // --- XÓA DÒNG GỌI OnPaintingSolved() Ở ĐÂY ---
        }
    }

    public void StartDissolve()
    {
        StartCoroutine(DissolveEffect());
    }

    private IEnumerator DissolveEffect()
    {
        Material frameMat = frameRenderer.material;
        Material pictureMat = (pictureRenderer != null) ? pictureRenderer.material : null;
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