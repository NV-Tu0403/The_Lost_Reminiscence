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

    [Tooltip("Thời gian giữa mỗi lần trừ máu (giây)")]
    public float damageCooldown = 2f;

    private Transform playerTarget;
    private float lastDamageTime = -99f;
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
            // --- LOGIC DI CHUYỂN ---
            Vector3 targetPosition = playerTarget.position + Vector3.up * hoverHeight;
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // --- LOGIC XOAY ĐÚNG ---
            Vector3 directionToPlayer = playerTarget.position - transform.position;
            directionToPlayer.y = 0; // Chỉ xoay ngang

            if (directionToPlayer.sqrMagnitude > 0.01f)
            {
                // Xoay trục Z+ (phía trước) về hướng người chơi
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

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