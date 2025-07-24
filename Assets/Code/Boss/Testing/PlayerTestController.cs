using UnityEngine;
using Code.Boss;

namespace Code.Boss.Testing
{
    /// <summary>
    /// Test controller để giả lập player movement và tương tác với Boss
    /// </summary>
    public class PlayerTestController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        
        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 1f;
        
        [Header("UI Testing")]
        [SerializeField] private bool showDebugUI = true;
        
        private CharacterController characterController;
        private float lastAttackTime;
        private BossManager bossManager;
        private Vector3 moveDirection;
        
        // Input tracking
        private bool isMoving;
        private Vector3 inputDirection;

        private void Start()
        {
            InitializeComponents();
            bossManager = BossManager.Instance;
        }

        private void InitializeComponents()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.radius = 0.5f;
                characterController.height = 2f;
                characterController.center = new Vector3(0, 1, 0);
            }
        }

        private void Update()
        {
            HandleMovementInput();
            HandleAttackInput();
            HandleFaSkillInput();
            
            MovePlayer();
        }

        private void HandleMovementInput()
        {
            // WASD movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            inputDirection = new Vector3(horizontal, 0, vertical).normalized;
            isMoving = inputDirection.magnitude > 0.1f;
            
            if (isMoving)
            {
                // Rotate player to face movement direction
                Vector3 lookDirection = inputDirection;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }

        private void HandleAttackInput()
        {
            // Space or Left Mouse Button to attack
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && CanAttack())
            {
                PerformAttack();
            }
        }

        private void HandleFaSkillInput()
        {
            // Q - Radar Skill
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("[Player Test] Requesting Fa Radar Skill");
                FaBossIntegration.NotifyFaSkillUsed("Radar", true);
            }
            
            // E - Second Skill (example)
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("[Player Test] Using Fa Second Skill");
                FaBossIntegration.NotifyFaSkillUsed("SecondSkill", true);
            }
            
            // R - Third Skill (example)
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[Player Test] Using Fa Third Skill");
                FaBossIntegration.NotifyFaSkillUsed("ThirdSkill", true);
            }
        }

        private void MovePlayer()
        {
            if (isMoving)
            {
                moveDirection = inputDirection * moveSpeed;
            }
            else
            {
                moveDirection = Vector3.zero;
            }
            
            // Apply gravity
            moveDirection.y -= 9.81f * Time.deltaTime;
            
            // Move the character
            if (characterController != null)
            {
                characterController.Move(moveDirection * Time.deltaTime);
            }
            else
            {
                transform.Translate(moveDirection * Time.deltaTime, Space.World);
            }
        }

        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            Debug.Log("[Player Test] Player attacking!");
            
            // Check if there's a boss or decoy in range
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            
            foreach (var hit in hits)
            {
                // Check for Boss
                var boss = hit.GetComponent<BossController>();
                if (boss != null)
                {
                    Debug.Log("[Player Test] Hit Boss directly!");
                    if (bossManager != null)
                    {
                        bossManager.PlayerAttackBoss();
                    }
                    return;
                }
                
                // Check for Decoy
                var decoy = hit.GetComponent<DecoyBehavior>();
                if (decoy != null)
                {
                    Debug.Log($"[Player Test] Hit {(decoy.IsReal ? "Real" : "Fake")} Decoy!");
                    if (bossManager != null)
                    {
                        bossManager.PlayerAttackDecoy(hit.gameObject, decoy.IsReal);
                    }
                    return;
                }
            }
            
            Debug.Log("[Player Test] Attack missed - no target in range");
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== PLAYER TEST CONTROLS ===");
            GUILayout.Label("WASD: Move");
            GUILayout.Label("Space/LMB: Attack");
            GUILayout.Label("Q: Fa Radar Skill");
            GUILayout.Label("E: Fa Second Skill");
            GUILayout.Label("R: Fa Third Skill");
            GUILayout.Space(10);
            GUILayout.Label($"Position: {transform.position}");
            GUILayout.Label($"Moving: {isMoving}");
            GUILayout.Label($"Can Attack: {CanAttack()}");
            GUILayout.EndArea();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
