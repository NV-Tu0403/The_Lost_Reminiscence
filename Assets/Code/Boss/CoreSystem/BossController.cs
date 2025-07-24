using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Code.Boss.States.Phase1;
using Code.Boss.States.Phase2;
using Code.Boss.States.Shared;

namespace Code.Boss
{
    /// <summary>
    /// Controller chính của Boss - quản lý FSM, health, phases
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BossConfig bossConfig;
        
        [Header("Components")]
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform navMeshCenter;
        
        // Core Systems
        private BossStateMachine stateMachine;
        private BossHealthSystem healthSystem;
        private BossSoulManager soulManager;
        private BossUIManager uiManager;
        
        // Current Phase
        private int currentPhase = 1;
        private List<GameObject> currentDecoys = new List<GameObject>();
        
        // Public Properties
        public BossConfig Config => bossConfig;
        public NavMeshAgent NavAgent => navMeshAgent;
        public Animator BossAnimator => animator;
        public AudioSource AudioSource => audioSource;
        public Transform Player => player;
        public Transform NavMeshCenter => navMeshCenter;
        public int CurrentPhase => currentPhase;
        public BossHealthSystem HealthSystem => healthSystem;
        public BossSoulManager SoulManager => soulManager;
        public List<GameObject> CurrentDecoys => currentDecoys;
        
        // Events
        public System.Action<int> OnPhaseChanged;
        public System.Action OnBossDefeated;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeSystems();
            StartBoss();
        }

        private void Update()
        {
            stateMachine?.Update();
        }

        private void InitializeComponents()
        {
            // Auto-find components if not assigned
            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();
            
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void InitializeSystems()
        {
            // Initialize FSM
            stateMachine = new BossStateMachine();
            stateMachine.Initialize(this);
            
            // Initialize Health System
            healthSystem = new BossHealthSystem(bossConfig.maxHealthPerPhase);
            healthSystem.OnHealthChanged += OnHealthChanged;
            healthSystem.OnPhaseHealthDepleted += OnPhaseCompleted;
            
            // Initialize Soul Manager
            soulManager = new BossSoulManager(this);
            
            // Initialize UI Manager
            uiManager = new BossUIManager(this);
            
            // Setup NavMesh
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = bossConfig.moveSpeed;
                navMeshAgent.angularSpeed = bossConfig.rotationSpeed;
            }
        }

        private void StartBoss()
        {
            // Trigger boss spawned event
            BossEventSystem.Trigger(BossEventType.BossSpawned);
            
            // Start with Phase 1
            ChangeToPhase(1);
        }

        private void OnHealthChanged(int newHealth, int maxHealth)
        {
            BossEventSystem.Trigger(BossEventType.HealthChanged, 
                new BossEventData { intValue = newHealth, floatValue = maxHealth });
        }

        private void OnPhaseCompleted()
        {
            if (currentPhase == 1)
            {
                ChangeToPhase(2);
            }
            else if (currentPhase == 2)
            {
                DefeatBoss();
            }
        }

        public void ChangeToPhase(int newPhase)
        {
            currentPhase = newPhase;
            healthSystem.ResetPhaseHealth();
            
            // Clear any existing decoys
            ClearDecoys();
            
            // Trigger phase change event
            BossEventSystem.Trigger(BossEventType.PhaseChanged, new BossEventData(newPhase));
            OnPhaseChanged?.Invoke(newPhase);
            
            // Change to appropriate starting state
            if (newPhase == 1)
            {
                stateMachine.ChangeState(new IdleState());
            }
            else if (newPhase == 2)
            {
                stateMachine.ChangeState(new AngryState());
            }
        }

        public void TakeDamage(int damage = 1)
        {
            healthSystem.TakeDamage(damage);
            stateMachine.OnTakeDamage();
            
            BossEventSystem.Trigger(BossEventType.BossTakeDamage, new BossEventData(damage));
            
            // Play damage sound
            if (audioSource && bossConfig.audioConfig.damageSound)
            {
                audioSource.PlayOneShot(bossConfig.audioConfig.damageSound, 
                    bossConfig.audioConfig.sfxVolume);
            }
        }

        public void ChangeState(BossState newState)
        {
            newState.Initialize(this, bossConfig);
            stateMachine.ChangeState(newState);
        }

        public void AddDecoy(GameObject decoy)
        {
            currentDecoys.Add(decoy);
        }

        public void RemoveDecoy(GameObject decoy)
        {
            currentDecoys.Remove(decoy);
        }

        public void ClearDecoys()
        {
            foreach (var decoy in currentDecoys)
            {
                if (decoy != null)
                    Destroy(decoy);
            }
            currentDecoys.Clear();
        }

        public void InterruptCurrentSkill()
        {
            if (stateMachine.CanInterruptCurrentState())
            {
                BossEventSystem.Trigger(BossEventType.SkillInterrupted);
                
                // Return to appropriate state based on phase
                if (currentPhase == 1)
                {
                    ChangeState(new IdleState());
                }
                else
                {
                    ChangeState(new AngryState());
                }
            }
        }

        private void DefeatBoss()
        {
            // Trigger defeat events
            BossEventSystem.Trigger(BossEventType.BossDefeated);
            OnBossDefeated?.Invoke();
            
            // Change to cook state for phase 2
            ChangeState(new CookState());
            
            // Play defeat sound
            if (audioSource && bossConfig.audioConfig.defeatSound)
            {
                audioSource.PlayOneShot(bossConfig.audioConfig.defeatSound, 
                    bossConfig.audioConfig.sfxVolume);
            }
        }

        public void PlayAnimation(string animationName)
        {
            // Replace animation calls with debug logs since no assets are available
            Debug.Log($"[Boss Animation] Playing animation: {animationName}");
            
            // Uncomment when animation assets are available:
            // if (animator != null)
            // {
            //     animator.SetTrigger(animationName);
            // }
        }

        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (audioSource && clip)
            {
                audioSource.PlayOneShot(clip, volume * bossConfig.audioConfig.masterVolume);
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= OnHealthChanged;
                healthSystem.OnPhaseHealthDepleted -= OnPhaseCompleted;
            }
            
            ClearDecoys();
            BossEventSystem.ClearAllListeners();
        }

        private void OnDrawGizmosSelected()
        {
            if (bossConfig != null)
            {
                // Draw decoy spawn radius
                Gizmos.color = Color.yellow;
                DrawWireCircle(transform.position, bossConfig.phase1.decoySpawnRadius);
                
                // Draw soul spawn radius
                Gizmos.color = Color.magenta;
                DrawWireCircle(transform.position, bossConfig.soulConfig.soulSpawnRadius);
                
                // Draw phase 2 circle radius
                if (navMeshCenter != null)
                {
                    Gizmos.color = Color.red;
                    DrawWireCircle(navMeshCenter.position, bossConfig.phase2.circleRadius);
                }
            }
        }

        /// <summary>
        /// Helper method to draw wire circle in XZ plane
        /// </summary>
        private void DrawWireCircle(Vector3 center, float radius)
        {
            int segments = 32;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius, 
                    0, 
                    Mathf.Sin(angle) * radius
                );
                
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
