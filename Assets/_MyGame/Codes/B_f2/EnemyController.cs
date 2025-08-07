using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyPhase
{
    Phase1,
    Phase2, 
    Phase3
}

public enum EnemySkill
{
    Skill_01,
    Skill_02,
    Skill_03,
    Skill_04
}

public class EnemyController : PlayerEventListenerBase
{
    [Header("Enemy Settings")]
    [SerializeField] private string currentPhaseDisplay; // Hiển thị trạng thái phase trong Inspector

    [SerializeField] private EnemyConfig config; // Tham chiếu đến file config
    [SerializeField] private float maxHealth = 100f; // Máu tối đa
    [SerializeField] private float currentHealth; // Máu hiện tại
    [SerializeField] private float detectionRadius = 10f; // Bán kính quét
    [SerializeField] private LayerMask detectionLayer; // Layer được phép quét
    [SerializeField] private float checkInterval = 0.5f; // Tần suất quét

    [Header("Movement Settings")]
    [SerializeField] private float stopDistance = 2f; // Khoảng cách dừng lại khi gần Player
    [SerializeField] private float MoveSpeed = 2f; // Tốc độ di chuyển
    [SerializeField] private float rotationSpeed = 5f; // Tốc độ xoay về hướng Player

    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.75f; // Ngưỡng chuyển sang Phase 2 (75% HP)
    [SerializeField] private float phase3Threshold = 0.50f; // Ngưỡng chuyển sang Phase 3 (50% HP)

    [Header("References")]
    [SerializeField] private GameObject targetCharacter; // Tham chiếu đến đối tượng có layer "Character"
    private Rigidbody _rigidbody;
    private Animator _animator; // Nếu có animation

    private EnemyPhase currentPhase = EnemyPhase.Phase1; // Trạng thái hiện tại
    private float lastCheckTime; // Thời gian lần quét cuối
    private int damageCount; // Đếm số lần nhận sát thương
    private float damageWindowTimer; // Bộ đếm thời gian cho cửa sổ nhận sát thương
    private bool isSkill1Active; // Trạng thái Skill 1
    private float skill1Timer; // Bộ đếm thời gian cho Skill 1
    private List<Skill2Point> skill2Points = new List<Skill2Point>(); // Lưu các điểm của Skill 2
    private float teleportCooldownTimer; // Bộ đếm thời gian cho dịch chuyển ở Phase 2

    private class Skill2Point
    {
        public Vector3 position;
        public GameObject vfxInstance;
        public float timer;
    }

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        currentHealth = maxHealth; // Khởi tạo máu
        currentPhaseDisplay = currentPhase.ToString(); // Khởi tạo giá trị hiển thị phase
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

        FaceTarget();

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

        // Cập nhật bộ đếm sát thương
        if (damageWindowTimer > 0)
        {
            damageWindowTimer -= Time.deltaTime;
            if (damageWindowTimer <= 0)
            {
                damageCount = 0; // Reset bộ đếm nếu hết thời gian
            }
        }

        // Cập nhật Skill 1
        if (isSkill1Active)
        {
            skill1Timer -= Time.deltaTime;
            if (skill1Timer <= 0)
            {
                if (targetCharacter != null && Vector3.Distance(transform.position, targetCharacter.transform.position) <= stopDistance)
                {
                    PlayerEvent.Instance.TriggerTakeOutDamage(gameObject, config.skill1Damage, targetCharacter);
                }
                skill1Timer = config.skill1Interval;
            }
        }

        // Cập nhật Skill 2
        UpdateSkill2();
    }

    public override void RegisterEvent(PlayerEvent e)
    {
        e.OnTakeOutDamage += TakeDamage; // Đăng ký sự kiện nhận sát thương
    }

    public override void UnregisterEvent(PlayerEvent e)
    {
        e.OnTakeOutDamage -= TakeDamage; // Hủy đăng ký sự kiện nhận sát thương
    }

    #region State Phase
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

    private void SetPhase(EnemyPhase newPhase)
    {
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            currentPhaseDisplay = currentPhase.ToString(); // Cập nhật giá trị hiển thị phase
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

    private void OnPhaseChanged()
    {
        switch (currentPhase)
        {
            case EnemyPhase.Phase1:
                Debug.Log("Phase 1: Normal behavior with teleport");
                isSkill1Active = false; // Tắt Skill 1 khi chuyển phase
                break;
            case EnemyPhase.Phase2:
                Debug.Log("Phase 2: Hovering and Skill 2");
                isSkill1Active = false; // Tắt Skill 1
                break;
            case EnemyPhase.Phase3:
                Debug.Log("Phase 3: Desperate behavior");
                isSkill1Active = false; // Tắt Skill 1
                break;
        }
    }
    #endregion

    #region Action
    private void ScanForTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        targetCharacter = null;
        foreach (Collider hit in hits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Character"))
            {
                targetCharacter = hit.gameObject;
                break;
            }
        }

        if (targetCharacter == null && hits.Length > 0)
        {
            targetCharacter = hits[0].gameObject;
            Debug.Log($"No Character found. Targeting: {targetCharacter.name}");
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        gameObject.SetActive(false);
    }

    private void FaceTarget()
    {
        if (targetCharacter == null) return;

        Vector3 direction = (targetCharacter.transform.position - transform.position).normalized;
        direction.y = 0; // Giữ xoay trên mặt phẳng XZ

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Phase
    private void Phase1Behavior()
    {
        if (targetCharacter == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetCharacter.transform.position);
        if (distanceToTarget > config.teleportDistanceThreshold && distanceToTarget <= detectionRadius)
        {
            Vector3 targetPos = targetCharacter.transform.position + (transform.position - targetCharacter.transform.position).normalized * stopDistance;
            Teleport(config.teleportDelay, targetPos, config.teleportWaitTime);
        }
    }

    private void Phase2Behavior()
    {
        if (targetCharacter == null) return;

        // tắt trọng lực
        if (_rigidbody != null)
        {
            _rigidbody.useGravity = false;
            // khóa 3 trục freeze position
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            // khóa quay
            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        // Bay lơ lững
        Vector3 targetHoverPos = new Vector3(transform.position.x, config.hoverHeight, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetHoverPos, config.hoverSpeed * Time.deltaTime);

        // Dịch chuyển tức thời định kỳ
        if (teleportCooldownTimer <= 0)
        {
            Vector3 targetPos = targetCharacter.transform.position + (transform.position - targetCharacter.transform.position).normalized * stopDistance;
            Teleport(config.teleportDelay, targetPos, config.teleportWaitTime);
            teleportCooldownTimer = config.teleportCooldown;
        }
        else
        {
            teleportCooldownTimer -= Time.deltaTime;
        }

        // Kích hoạt Skill 2
        if (skill2Points.Count == 0) // Chỉ kích hoạt khi không có Skill 2 đang hoạt động
        {
            Skill_02();
        }
    }

    private void Phase3Behavior()
    {
        //if (targetCharacter != null)
        //{
        //    MoveTowardsTarget(MoveSpeed); // Quay lại di chuyển bình thường
        //}

        // tắt trọng lực
        if (_rigidbody != null)
        {
            _rigidbody.useGravity = true;
            // bỏ khóa 3 trục freeze position
            _rigidbody.constraints = RigidbodyConstraints.None;
        }

    }
    #endregion

    #region Move
    private void MoveTowardsTarget(float speed)
    {
        if (targetCharacter == null) return;

        Vector3 direction = (targetCharacter.transform.position - transform.position).normalized;
        direction.y = 0;

        float distanceToTarget = Vector3.Distance(transform.position, targetCharacter.transform.position);
        if (distanceToTarget > stopDistance)
        {
            Vector3 moveVector = direction * speed * Time.deltaTime;
            if (!Physics.Raycast(transform.position, direction, moveVector.magnitude + 0.1f, LayerMask.GetMask("Environment")))
            {
                transform.position += moveVector;
            }
            else
            {
                Debug.Log("Blocked by obstacle");
            }
        }

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    private void Teleport(float timeDelay, Vector3 targetPos, float timeWait)
    {
        StartCoroutine(TeleportCoroutine(timeDelay, targetPos, timeWait));

    }

    private IEnumerator TeleportCoroutine(float timeDelay, Vector3 targetPos, float timeWait)
    {
        // Bật VFX tại vị trí hiện tại
        if (config.teleportVFXPrefab != null)
        {
            Instantiate(config.teleportVFXPrefab, transform.position, Quaternion.identity);
        }

        // Chờ timeDelay và tắt Enemy
        yield return new WaitForSeconds(timeDelay);
        gameObject.SetActive(false);

        // Đặt vị trí mới
        transform.position = targetPos;

        // Bật VFX tại vị trí đích
        if (config.teleportVFXPrefab != null)
        {
            Instantiate(config.teleportVFXPrefab, transform.position, Quaternion.identity);
        }

        // Bật lại Enemy
        gameObject.SetActive(true);

        // Chờ timeWait
        yield return new WaitForSeconds(timeWait);
    }
    #endregion

    #region Attack
    public void TakeDamage(GameObject Attacker, float damage, GameObject target)
    {
        if (target.name != gameObject.name || Attacker.name == gameObject.name) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{gameObject.name} nhận {damage} damage từ {Attacker.name}. Current health: {currentHealth}");

        // Đếm sát thương cho Skill 1
        if (currentPhase == EnemyPhase.Phase1)
        {
            damageCount++;
            damageWindowTimer = config.skill1DamageWindow;

            if (damageCount >= config.skill1TriggerDamageCount)
            {
                Skill_01();
                damageCount = 0; // Reset sau khi kích hoạt
            }
        }

        UpdatePhase();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Skill_01()
    {
        isSkill1Active = true;
        skill1Timer = config.skill1Interval;
        Debug.Log("Skill 1 activated: Continuous damage within stopDistance");
    }

    private void Skill_02()
    {
        if (currentPhase != EnemyPhase.Phase2) return;

        // Lấy vị trí Player và tạo 5 điểm ngẫu nhiên
        Vector3 playerPos = targetCharacter != null ? targetCharacter.transform.position : transform.position;
        skill2Points.Clear();

        // Thêm vị trí Player
        skill2Points.Add(CreateSkill2Point(playerPos));

        // Tạo 5 điểm ngẫu nhiên
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-config.mapSize.x / 2, config.mapSize.x / 2),
                0,
                Random.Range(-config.mapSize.y / 2, config.mapSize.y / 2)
            );
            skill2Points.Add(CreateSkill2Point(randomPos));
        }

        Debug.Log("Skill 2 activated: Created damage zones");
    }

    private Skill2Point CreateSkill2Point(Vector3 position)
    {
        Skill2Point point = new Skill2Point
        {
            position = position,
            timer = config.skill2Duration
        };

        // Tạo hiệu ứng VFX
        if (config.skill2VFXPrefab != null)
        {
            point.vfxInstance = Instantiate(config.skill2VFXPrefab, position, Quaternion.identity);
        }

        return point;
    }

    private void UpdateSkill2()
    {
        if (skill2Points.Count == 0) return;

        for (int i = skill2Points.Count - 1; i >= 0; i--)
        {
            Skill2Point point = skill2Points[i];
            point.timer -= Time.deltaTime;

            // Kiểm tra Player trong vùng OverlapSphere
            if (targetCharacter != null)
            {
                Collider[] hits = Physics.OverlapSphere(point.position, config.skill2SphereRadius, detectionLayer);
                foreach (Collider hit in hits)
                {
                    if (hit.gameObject == targetCharacter)
                    {
                        point.timer -= Time.deltaTime; // Giảm timer để gây sát thương mỗi giây
                        if (point.timer <= 0)
                        {
                            PlayerEvent.Instance.TriggerTakeOutDamage(gameObject, config.skill2Damage, targetCharacter);
                            point.timer = config.skill2Interval; // Reset timer cho lần gây sát thương tiếp theo
                        }
                        break;
                    }
                }
            }

            // Xóa điểm khi hết thời gian
            if (point.timer <= 0)
            {
                if (point.vfxInstance != null)
                {
                    Destroy(point.vfxInstance);
                }
                skill2Points.RemoveAt(i);
            }
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Vẽ các vùng Skill 2
        Gizmos.color = Color.red;
        foreach (Skill2Point point in skill2Points)
        {
            Gizmos.DrawWireSphere(point.position, config.skill2SphereRadius);
        }
    }
}

public static class LayerMaskExtensions
{
    public static bool IsLayerInMask(this int layerMask, int layer)
    {
        return (layerMask & (1 << layer)) != 0;
    }
}