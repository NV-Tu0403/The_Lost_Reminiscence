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

    [Header("Punishment")]
    [Tooltip("Kéo Prefab của con ma (DucHon) vào đây")]
    public GameObject ghostPrefab;

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
            Debug.Log("Bạn cần tìm một mảnh tranh để đặt vào đây.");
            return;
        }

        if (heldPicture.pictureID == requiredPictureID)
        {
            // ĐÚNG
            Debug.Log("CHÍNH XÁC! Bạn đã lắp đúng mảnh tranh!");
            isSolved = true;
            PlaceAndSolve(heldPicture);
            interactor.ClearHeldPicture();
        }
        else
        {
            // SAI
            Debug.Log("SAI RỒI! Mảnh tranh này không khớp. Một linh hồn đã được triệu hồi!");
            
            SpawnPunishmentGhost(interactor.transform);

            interactor.ClearHeldPicture(true);
            GetComponent<Collider>().enabled = false;
        }
    }

    private void SpawnPunishmentGhost(Transform playerTransform)
    {
        if (ghostPrefab == null)
        {
            Debug.LogError("Chưa gán Ghost Prefab vào InteractablePainting!");
            return;
        }

        // Tính toán vị trí spawn ở phía sau người chơi 3 mét, và cao hơn 1 mét
        Vector3 spawnPosition = playerTransform.position - playerTransform.forward * 3f + Vector3.up * 1f;
        
        // Tạo con ma và cho nó quay mặt về phía người chơi
        Quaternion spawnRotation = Quaternion.LookRotation(playerTransform.position - spawnPosition);
        Instantiate(ghostPrefab, spawnPosition, spawnRotation);
    }

    private void PlaceAndSolve(CollectiblePicture picture)
    {
        // Hiện Mảnh Tranh lên và đặt nó vào đúng vị trí trong khung
        picture.transform.SetParent(pictureSlot.parent);
        picture.transform.position = pictureSlot.position;
        picture.transform.rotation = pictureSlot.rotation;
        picture.transform.localScale = pictureSlot.localScale;
        picture.gameObject.SetActive(true);

        // Thông báo cho PuzzleManager
        if (PaintingPuzzleController.Instance != null)
        {
            PaintingPuzzleController.Instance.OnPaintingSolved();
        }

        // Bắt đầu hiệu ứng tan biến
        StartCoroutine(DissolveEffect(picture.GetComponent<Renderer>()));
    }

    private IEnumerator DissolveEffect(Renderer pictureRenderer)
    {
        GetComponent<Collider>().enabled = false;
        isSolved = true;
        
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
        
        // Sau khi hiệu ứng kết thúc, tắt hẳn GameObject đi
        gameObject.SetActive(false);
    }
}