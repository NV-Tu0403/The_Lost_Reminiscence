using UnityEngine;

/// <summary>
/// Có thể tạo nhiều PlayerConfig (ví dụ: cho các chế độ dễ, khó) và gán vào PlayerController tùy nhu cầu.
/// </summary>
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/Stats")]
public class PlayerConfig : ScriptableObject
{
    [Header("Player Attributes")]
    public float health = 100f;
    public float mana = 100f;
    public float attackDamage = 10f; // Sát thương cơ bản khi tấn công
    public float coin;
    public float exp;
    [SerializeField] public float maxDamagePerHit = 100f; // Giá trị mặc định
    public float MaxDamagePerHit => maxDamagePerHit;

    [Header("Movement Settings")]
    public float jumpImpulse = 8.0f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 6.0f;
    public float sprintSpeed = 8.0f;
    public float dashSpeed = 12.0f;
    public float rotationSpeed = 100f;
    public float acceleration = 15.0f;
    public float braking = 15.0f;
    public float dashCooldown = 2.0f;
    public float renderInterpolation = 10f;

    [Header("Camera Action Settings")]
    public float zoomFactor = 8f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float attackDuration = 0.5f;

    [Header("Throwing Settings")]
    public string prefabPath = "Fusion_M3/Prefabs/Item/Capsule";
    public float throwForceMin = 1f;
    public float throwForceMax = 100f;
    public float throwForce = 10f;
    public float throwCooldown = 1f;
    public float maxHoldTime = 1.5f;
}