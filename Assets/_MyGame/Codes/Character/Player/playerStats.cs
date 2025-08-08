using UnityEngine;

//[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats", order = 1)]
public class PlayerStats : ScriptableObject
{
    [Header("Player Attributes")]
    public float health = 100f;
    public float mana = 100f;
    public float attackDamage = 10f; // Sát thương cơ bản khi tấn công

    public float coin;
    public float exp;

    [SerializeField] public float maxDamagePerHit = 100f; // Giá trị mặc định

    public float MaxDamagePerHit => maxDamagePerHit;
}