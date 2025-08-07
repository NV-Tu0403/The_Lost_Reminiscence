using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DuckLe.CharacterInput))]

public class RigidbodyNavMeshBridge : MonoBehaviour
{
    [Tooltip("Tốc độ di chuyển của nhân vật")]
    public float moveSpeed = 5.0f;
    [Tooltip("Tốc độ xoay của nhân vật")]
    public float rotationSpeed = 720f; // Tăng giá trị để xoay nhanh hơn

    private Rigidbody rb;
    private NavMeshAgent agent;
    private DuckLe.CharacterInput characterInput;
    private Camera mainCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        characterInput = GetComponent<DuckLe.CharacterInput>();

        agent.updatePosition = false;
        agent.updateRotation = false; // Tắt hoàn toàn việc xoay của Agent
        agent.updateUpAxis = false;

        mainCamera = Camera.main; // Cache camera để tối ưu
    }

    void FixedUpdate()
    {
        // 1. Đồng bộ vị trí
        agent.nextPosition = rb.position;

        // 2. Lấy Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = Vector3.zero;

        // 3. Tính toán hướng di chuyển dựa trên Camera
        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            moveDirection = (camForward.normalized * v + camRight.normalized * h).normalized;
        }
        else // Fallback nếu không có camera
        {
            moveDirection = new Vector3(h, 0, v).normalized;
        }

        // 4. Áp dụng di chuyển
        Vector3 desiredVelocity = moveDirection * moveSpeed;
        agent.velocity = desiredVelocity;
        
        Vector3 agentCalculatedVelocity = agent.velocity;
        agentCalculatedVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = agentCalculatedVelocity;

        // 5. Xử lý xoay nhân vật
        HandleRotation(moveDirection);
    }

    private void HandleRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.01f) // Chỉ xoay khi có di chuyển
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }
}