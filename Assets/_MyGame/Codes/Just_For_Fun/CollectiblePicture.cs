using UnityEngine;
using System.Collections;

public class CollectiblePicture : MonoBehaviour, IInteractable
{
    [Header("Picture Logic")]
    [Tooltip("ID định danh cho bức tranh này (chỉ cần cho tranh thật).")]
    public string pictureID = "Default_ID";

    [Tooltip("Đánh dấu nếu đây là một bức tranh giả.")]
    public bool isFake = false;

    [Header("Effects & Punishment")]
    [Tooltip("Thời gian tan biến của tranh giả (giây).")]
    public float dissolveDuration = 1.0f;

    [Tooltip("Kéo Prefab của con ma (DucHon) vào đây (chỉ cần cho tranh giả).")]
    public GameObject ghostPrefab;

    private Renderer pictureRenderer;
    private bool isInteracted = false;

    private void Awake()
    {
        pictureRenderer = GetComponent<Renderer>();
    }

    public void Interact(PlayerPuzzleInteractor interactor)
    {
        if (isInteracted || interactor == null) return;

        isInteracted = true; // Đánh dấu đã tương tác để tránh gọi lại

        if (isFake)
        {
            // Nếu là tranh giả, bắt đầu hiệu ứng tan biến và triệu hồi ma
            Debug.Log("Đây là tranh giả! Một linh hồn đã được giải phóng!");
            StartCoroutine(FakePictureEffect(interactor.transform));
        }
        else
        {
            // Nếu là tranh thật, báo cho Player để nhặt
            interactor.PickupPicture(this);
        }
    }

    private IEnumerator FakePictureEffect(Transform playerTransform)
    {
        // Tắt collider ngay lập tức
        GetComponent<Collider>().enabled = false;

        // Bắt đầu hiệu ứng tan biến
        Material mat = pictureRenderer.material;
        float elapsedTime = 0f;

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveDuration);
            mat.SetFloat("_DissolveAmount", dissolveAmount);
            yield return null;
        }

        // Triệu hồi ma sau khi tan biến xong
        SpawnPunishmentGhost(playerTransform);

        // Hủy đối tượng tranh giả
        Destroy(gameObject);
    }

    private void SpawnPunishmentGhost(Transform playerTransform)
    {
        if (ghostPrefab == null)
        {
            Debug.LogError("Chưa gán Ghost Prefab vào CollectiblePicture giả!");
            return;
        }

        // Vị trí spawn ở phía sau người chơi
        Vector3 spawnPosition = playerTransform.position - playerTransform.forward * 3f + Vector3.up * 1f;
        Quaternion spawnRotation = Quaternion.LookRotation(playerTransform.position - spawnPosition);
        Instantiate(ghostPrefab, spawnPosition, spawnRotation);
    }
}