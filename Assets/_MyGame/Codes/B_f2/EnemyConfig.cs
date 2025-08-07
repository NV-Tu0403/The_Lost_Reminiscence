using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/EnemyConfig", order = 1)]
public class EnemyConfig : ScriptableObject
{
    [Header("Map Settings")]
    public Vector2 mapSize = new Vector2(100f, 100f); // Kích thước bản đồ (x, z)

    [Header("Skill 1 Settings")]
    public float skill1Damage = 10f; // Sát thương của Skill 1
    public float skill1Interval = 1f; // Khoảng cách thời gian giữa các lần gây sát thương
    public int skill1TriggerDamageCount = 4; // Số lần nhận sát thương để kích hoạt Skill 1
    public float skill1DamageWindow = 6f; // Thời gian đếm sát thương (6 giây)

    [Header("Skill 2 Settings")]
    public float skill2Damage = 15f; // Sát thương của Skill 2
    public float skill2SphereRadius = 3f; // Bán kính vùng OverlapSphere của Skill 2
    public float skill2Duration = 5f; // Thời gian tồn tại của Skill 2
    public float skill2Interval = 1f; // Khoảng cách thời gian giữa các lần gây sát thương
    public GameObject skill2VFXPrefab; // Prefab hiệu ứng VFX cho Skill 2

    [Header("Phase 1 Settings")]
    public float teleportDistanceThreshold = 5f; // Khoảng cách tối thiểu để dịch chuyển tức thời
    public float teleportDelay = 1f; // Thời gian chờ trước khi dịch chuyển
    public float teleportWaitTime = 5f; // Thời gian chờ sau khi dịch chuyển
    public GameObject teleportVFXPrefab; // Prefab hiệu ứng VFX cho teleport

    [Header("Phase 2 Settings")]
    public float hoverHeight = 5f; // Độ cao bay lơ lững
    public float hoverSpeed = 2f; // Tốc độ bay lơ lững
    public float teleportCooldown = 10f; // Thời gian chờ giữa các lần dịch chuyển ở Phase 2
}