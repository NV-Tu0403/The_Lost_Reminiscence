using UnityEngine;
using System.Collections;

public enum EnemyPhase
{
    Phase1, // Trạng thái mặc định (HP cao)
    Phase2, // Trạng thái khi HP giảm (50%-75%)
    Phase3  // Trạng thái khi HP thấp (<50%)
}

public class EnemyController : PlayerEventListenerBase
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f; // Máu tối đa
    [SerializeField] private float currentHealth; // Máu hiện tại
    [SerializeField] private float detectionRadius = 10f; // Bán kính quét
    [SerializeField] private LayerMask detectionLayer; // Layer được phép quét
    [SerializeField] private float checkInterval = 0.5f; // Tần suất quét

    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.75f; // Ngưỡng chuyển sang Phase 2 (75% HP)
    [SerializeField] private float phase3Threshold = 0.50f; // Ngưỡng chuyển sang Phase 3 (50% HP)

    [Header("References")]
    [SerializeField] private GameObject targetCharacter; // Tham chiếu đến đối tượng có layer "Character"
    private Rigidbody _rigidbody;
    private Animator _animator; // Nếu có animation

    private EnemyPhase currentPhase = EnemyPhase.Phase1; // Trạng thái hiện tại
    private float lastCheckTime; // Thời gian lần quét cuối

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        currentHealth = maxHealth; // Khởi tạo máu
    }

    private void Start()
    {
        // Đảm bảo LayerMask bao gồm layer "Character"
        if (!detectionLayer.value.IsLayerInMask(LayerMask.NameToLayer("Character")))
        {
            Debug.LogWarning("Layer 'Character' is not included in detectionLayer. Please add it in the Inspector.");
        }

        UpdatePhase(); // Khởi tạo trạng thái ban đầu
    }

    private void Update()
    {
        // Quét định kỳ để tìm mục tiêu
        if (Time.time - lastCheckTime >= checkInterval)
        {
            ScanForTargets();
            lastCheckTime = Time.time;
        }

        // Cập nhật hành vi theo phase
        switch (currentPhase)
        {
            case EnemyPhase.Phase1:
                Phase1Behavior();
                break;
            case EnemyPhase.Phase2:
                Phase2Behavior();
                break;
            case EnemyPhase.Phase3:
                Phase3Behavior();
                break;
        }
    }

    public override void RegisterEvent(PlayerEvent e)
    {
        e.OnTakeOutDamage += TakeDamage; // Đăng ký sự kiện nhận sát thương
    }

    public override void UnregisterEvent(PlayerEvent e)
    {
        e.OnTakeOutDamage -= TakeDamage; // Hủy đăng ký sự kiện nhận sát thương
    }

    // Quét các đối tượng trong bán kính detectionRadius
    private void ScanForTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        // Ưu tiên tìm đối tượng có layer "Character"
        targetCharacter = null;
        foreach (Collider hit in hits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Character"))
            {
                targetCharacter = hit.gameObject;
                Debug.Log($"Found Character: {targetCharacter.name}");
                break; // Ưu tiên Character, dừng quét khi tìm thấy
            }
        }

        // Nếu không tìm thấy Character, có thể lưu tham chiếu đến đối tượng khác (nếu cần)
        if (targetCharacter == null && hits.Length > 0)
        {
            targetCharacter = hits[0].gameObject; // Lấy đối tượng đầu tiên nếu không có Character
            Debug.Log($"No Character found. Targeting: {targetCharacter.name}");
        }
    }

    // Cập nhật trạng thái dựa trên lượng máu
    private void UpdatePhase()
    {
        float healthPercentage = currentHealth / maxHealth;

        if (healthPercentage > phase2Threshold)
        {
            SetPhase(EnemyPhase.Phase1);
        }
        else if (healthPercentage > phase3Threshold)
        {
            SetPhase(EnemyPhase.Phase2);
        }
        else
        {
            SetPhase(EnemyPhase.Phase3);
        }
    }

    // Đặt trạng thái và xử lý thay đổi
    private void SetPhase(EnemyPhase newPhase)
    {
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            Debug.Log($"Enemy switched to {currentPhase}");

            // Cập nhật animation nếu có
            if (_animator != null)
            {
                _animator.SetInteger("Phase", (int)currentPhase);
            }

            // Thực hiện các hành động khi chuyển phase
            OnPhaseChanged();
        }
    }

    // Hàm xử lý khi chuyển phase
    private void OnPhaseChanged()
    {
        // Có thể thêm logic đặc biệt khi chuyển phase, ví dụ: thay đổi tốc độ, kích hoạt skill
        switch (currentPhase)
        {
            case EnemyPhase.Phase1:
                Debug.Log("Phase 1: Normal behavior");
                break;
            case EnemyPhase.Phase2:
                Debug.Log("Phase 2: Aggressive behavior");
                break;
            case EnemyPhase.Phase3:
                Debug.Log("Phase 3: Desperate behavior");
                break;
        }
    }

    // Hành vi cho từng phase
    private void Phase1Behavior()
    {
        // Ví dụ: Di chuyển chậm, tấn công cơ bản
        if (targetCharacter != null)
        {
            MoveTowardsTarget(2f); // Tốc độ chậm
            // Thêm logic tấn công nếu cần
        }
    }

    private void Phase2Behavior()
    {
        // Ví dụ: Di chuyển nhanh hơn, tấn công mạnh hơn
        if (targetCharacter != null)
        {
            MoveTowardsTarget(4f); // Tốc độ nhanh hơn
            // Thêm logic tấn công mạnh hơn
        }
    }

    private void Phase3Behavior()
    {
        // Ví dụ: Di chuyển rất nhanh, hành vi liều lĩnh
        if (targetCharacter != null)
        {
            MoveTowardsTarget(6f); // Tốc độ rất nhanh
            // Thêm logic tấn công liều lĩnh
        }
    }

    // Di chuyển tới mục tiêu
    private void MoveTowardsTarget(float speed)
    {
        if (targetCharacter == null) return;

        Vector3 direction = (targetCharacter.transform.position - transform.position).normalized;
        direction.y = 0; // Giữ di chuyển trên mặt phẳng XZ

        // Kiểm tra va chạm trước khi di chuyển
        Vector3 moveVector = direction * speed * Time.deltaTime;
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, direction, out hit, moveVector.magnitude + 0.1f, LayerMask.GetMask("Environment")))
        {
            transform.position += moveVector;
        }
        else
        {
            Debug.Log("Blocked by obstacle: " + hit.collider.name);
        }

        // Xoay về phía mục tiêu
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    // Nhận sát thương và cập nhật phase
    public void TakeDamage(GameObject Attacker, float damage, GameObject target)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Enemy took {damage} damage from {Attacker.name}. Current health: {currentHealth}");

        UpdatePhase();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

// Extension method để kiểm tra layer trong LayerMask
public static class LayerMaskExtensions
{
    public static bool IsLayerInMask(this int layerMask, int layer)
    {
        return (layerMask & (1 << layer)) != 0;
    }
}