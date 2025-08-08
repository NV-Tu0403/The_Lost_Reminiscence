using UnityEngine;

// Xóa RequireComponent NavMeshAgent vì không dùng nữa
[RequireComponent(typeof(Collider))] 
public class ChasingGhost : MonoBehaviour
{
    [Tooltip("Tốc độ bay của con ma")]
    public float moveSpeed = 3f;

    [Tooltip("Độ cao lơ lửng so với vị trí của người chơi")]
    public float hoverHeight = 1.5f;

    [Tooltip("Tốc độ xoay của con ma để nhìn về phía người chơi")]
    public float rotationSpeed = 5f;

    [Tooltip("Thời gian tồn tại của con ma (giây)")]
    public float lifeTime = 15f;

    [Tooltip("Thời gian giữa mỗi lần trừ máu (giây)")]
    public float damageCooldown = 2f;

    private Transform playerTarget;
    private float lastDamageTime = -99f;


    void Start()
    {
        // Tìm đối tượng Player bằng Tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy đối tượng Player! Hãy chắc chắn nhân vật của bạn có tag 'Player'.");
            Destroy(gameObject); // Tự hủy nếu không tìm thấy người chơi
            return;
        }

        // Tự động hủy con ma sau một khoảng thời gian
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (playerTarget != null)
        {
            // --- LOGIC DI CHUYỂN MỚI ---
            // 1. Xác định vị trí đích: là vị trí của người chơi nhưng ở độ cao lơ lửng
            Vector3 targetPosition = playerTarget.position + Vector3.up * hoverHeight;

            // 2. Di chuyển mượt mà về phía đích
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 3. Xoay mượt mà để nhìn về phía người chơi
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // Hàm OnTriggerEnter vẫn giữ nguyên
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time > lastDamageTime + damageCooldown)
        {
            PlayerPuzzleInteractor playerInteractor = other.GetComponent<PlayerPuzzleInteractor>();
            if (playerInteractor != null)
            {
                Debug.Log("Ghost touched the player!");
                playerInteractor.TakePuzzleDamage();
                lastDamageTime = Time.time;
            }
        }
    }
}