using UnityEngine;
using System.Collections; // Cần thiết để sử dụng Coroutine

public class InteractablePainting : MonoBehaviour, IInteractable
{
    [Tooltip("Đánh dấu nếu đây là bức tranh đúng")]
    public bool isCorrectAnswer = false;

    [Tooltip("Hiệu ứng tan biến sẽ kéo dài trong bao lâu (giây)")]
    public float dissolveDuration = 1.5f;

    // Các biến để lưu trữ Renderer của khung và tranh
    private Renderer frameRenderer;
    private Renderer pictureRenderer;

    // Cờ để đảm bảo hiệu ứng không bị gọi nhiều lần
    private bool isDissolving = false;

    private void Start()
    {
        // Lấy Renderer của chính GameObject này (khungtranh)
        frameRenderer = GetComponent<Renderer>();

        // Tìm và lấy Renderer của GameObject con tên là "Cube" (bức tranh)
        Transform pictureTransform = transform.Find("Cube");
        if (pictureTransform != null)
        {
            pictureRenderer = pictureTransform.GetComponent<Renderer>();
        }
    }

    public void Interact(PlayerPuzzleInteractor interactor)
    {
        // Nếu đang tan biến thì không cho tương tác nữa
        if (isDissolving || interactor == null) return;

        if (isCorrectAnswer)
        {
            Debug.Log("CHÍNH XÁC! Bạn đã giải được câu đố!");

            if (PaintingPuzzleController.Instance != null)
            {
                PaintingPuzzleController.Instance.OnPaintingSolved();
            }

            // Thay vì tắt ngay lập tức, hãy bắt đầu hiệu ứng tan biến
            StartCoroutine(DissolveEffect());
        }
        else
        {
            Debug.Log("SAI RỒI! Hãy thử lại.");
            interactor.TakePuzzleDamage();
        }
    }

    private IEnumerator DissolveEffect()
    {
        isDissolving = true;
        // Tắt Collider ngay lập tức để người chơi không thể tương tác lại
        GetComponent<Collider>().enabled = false;

        // Lấy bản sao của material để không ảnh hưởng đến các tranh khác
        Material frameMat = frameRenderer.material;
        Material pictureMat = pictureRenderer.material;

        float elapsedTime = 0f;

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;

            // Tính toán giá trị tan biến từ 0 đến 1
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveDuration);

            // Cập nhật giá trị "DissolveAmount" cho cả hai material
            // LƯU Ý: Tên "_DissolveAmount" phải khớp với tên Reference trong Shader Graph
            frameMat.SetFloat("_DissolveAmount", dissolveAmount);
            if (pictureMat != null)
            {
                pictureMat.SetFloat("_DissolveAmount", dissolveAmount);
            }

            yield return null; // Chờ đến frame tiếp theo
        }

        // Sau khi hiệu ứng kết thúc, tắt hẳn GameObject đi
        gameObject.SetActive(false);
    }
}