using UnityEngine;

public class InteractablePainting : MonoBehaviour
{
    [Tooltip("Đánh dấu nếu đây là bức tranh đúng")]
    public bool isCorrectAnswer = false;

    // Hàm Interact không thay đổi, vẫn được gọi bởi người chơi
    public void Interact(TestController player)
    {
        if (isCorrectAnswer)
        {
            Debug.Log("CHÍNH XÁC! Bạn đã giải được câu đố!");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("SAI RỒI! Hãy thử lại.");
            player.TakeDamage();
        }
    }

    // --- PHẦN MỚI ---
    // Hàm này tự động được gọi khi có một Collider khác đi vào Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        TestController player = other.GetComponent<TestController>();
        if (player != null)
        {
            // Nếu đúng là Player, báo cho Player biết nó có thể tương tác với Bức tranh này
            player.SetInteractable(this);
        }
    }

    // Hàm này tự động được gọi khi Collider kia đi ra khỏi Trigger
    private void OnTriggerExit(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        TestController player = other.GetComponent<TestController>();
        if (player != null)
        {
            // Báo cho Player biết nó đã ra khỏi vùng tương tác của Bức tranh này
            player.ClearInteractable(this);
        }
    }
}