using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
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
    
    // Xóa biến damageCooldown vì không cần nữa

    private Transform playerTarget;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy đối tượng Player! Hãy chắc chắn nhân vật của bạn có tag 'Player'.");
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        if (playerTarget != null)
        {
            Vector3 targetPosition = playerTarget.position + Vector3.up * hoverHeight;
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            Vector3 directionToPlayer = playerTarget.position - transform.position;
            directionToPlayer.y = 0;

            if (directionToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // --- THAY ĐỔI LOGIC KHI VA CHẠM ---
    private void OnTriggerEnter(Collider other)
    {
        // Chỉ cần kiểm tra va chạm với Player
        if (other.CompareTag("Player"))
        {
            PlayerPuzzleInteractor playerInteractor = other.GetComponent<PlayerPuzzleInteractor>();
            if (playerInteractor != null)
            {
                Debug.Log("Ghost caught the player!");
                
                // Gọi hàm dịch chuyển người chơi về điểm xuất phát
                playerInteractor.TeleportToStart();

                // Sau khi bắt được người chơi, con ma sẽ tự biến mất
                Destroy(gameObject);
            }
        }
    }
}