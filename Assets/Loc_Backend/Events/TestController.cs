using UnityEngine;

public class TestController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    private float verticalVelocity;
    private CharacterController controller;

    [Header("Look")]
    public float mouseSensitivity = 100f;

    [Header("Interaction")]
    public CameraController cameraController;
    // Biến mới để lưu bức tranh có thể tương tác
    private InteractablePainting currentInteractable;

    [Header("Stats")]
    public int health = 3;

    [Header("Control")]
    public bool isActive = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraController == null)
        {
            Debug.LogError("Chưa gán CameraController cho Player!");
        }
    }

    void Update()
    {
        if (!isActive) return;

        HandleLook();
        HandleMovement();

        // --- LOGIC TƯƠNG TÁC MỚI ---
        // Nếu người chơi nhấn E và đang ở trong vùng của một bức tranh
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            // Gọi hàm Interact của bức tranh đó
            currentInteractable.Interact(this);
        }
    }
    
    // --- HÀM MỚI ---
    // Hàm này được gọi bởi InteractablePainting khi người chơi đi vào vùng trigger
    public void SetInteractable(InteractablePainting painting)
    {
        currentInteractable = painting;
        Debug.Log("Đã vào vùng tương tác của: " + painting.gameObject.name);
        // Ở đây bạn có thể hiện UI thông báo như "[E] to Interact"
    }

    // --- HÀM MỚI ---
    // Hàm này được gọi bởi InteractablePainting khi người chơi đi ra
    public void ClearInteractable(InteractablePainting painting)
    {
        // Chỉ xóa tham chiếu nếu nó đúng là bức tranh người chơi vừa rời khỏi
        if (currentInteractable == painting)
        {
            currentInteractable = null;
            Debug.Log("Đã rời khỏi vùng tương tác.");
        }
    }

    // Các hàm còn lại giữ nguyên...
    public void TakeDamage()
    {
        health--;
        Debug.Log("Người chơi bị trừ máu! Máu còn lại: " + health);
        if (cameraController != null)
        {
            cameraController.ShakeCamera();
        }
        if (health <= 0)
        {
            Debug.Log("GAME OVER!");
            isActive = false;
        }
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(h, 0, v).normalized;
        Vector3 moveDirection = transform.TransformDirection(inputDirection);

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        controller.Move(moveDirection * moveSpeed * Time.deltaTime + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }
}