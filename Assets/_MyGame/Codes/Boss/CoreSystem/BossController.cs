using System.Collections.Generic;
using _MyGame.Codes.Boss.States.Phase1;
using _MyGame.Codes.Boss.States.Phase2;
using Code.Boss;
using UnityEngine;
using UnityEngine.AI;
using FMODUnity; // Added for FMOD integration
using FMOD.Studio; // Added for FMOD EventInstance
using STOP_MODE = FMOD.Studio.STOP_MODE; // Alias FMOD Studio STOP_MODE

namespace _MyGame.Codes.Boss.CoreSystem
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
        
        // Initial position and rotation for reset
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        
        // FMOD: cached looping instances
        private EventInstance heartbeatInstance;
        private bool heartbeatActive = false;
        
        // Animator parameter hashes
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        
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
            // Listen for player defeat to pause boss systems and stop audio loops
            BossEventSystem.Subscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
        }

        private void Start()
        {
            // Store initial position and rotation for reset
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            
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
            if (bossConfig == null)
            {
                Debug.LogError("BossConfig is not assigned! Please assign a valid BossConfig.");
                return;
            }
            
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
            BossEventSystem.Trigger(BossEventType.BossSpawned);
            // Debug: chọn phase khi test bằng enum
            switch (bossConfig.debugStartPhase)
            {
                case BossDebugPhase.Phase1:
                    ChangeToPhase(1);
                    break;
                case BossDebugPhase.Phase2:
                    ChangeToPhase(2);
                    break;
                default:
                    ChangeToPhase(1);
                    break;
            }
        }

        private void OnHealthChanged(int newHealth, int maxHealth)
        {
            BossEventSystem.Trigger(BossEventType.HealthChanged, 
                new BossEventData { intValue = newHealth, floatValue = maxHealth });
        }

        private void OnPhaseCompleted()
        {
            switch (currentPhase)
            {
                case 1:
                    ChangeToPhase(2);
                    break;
                case 2:
                    DefeatBoss();
                    break;
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

            switch (newPhase)
            {
                case 1:
                    stateMachine.ChangeState(new IdleState());
                    break;
                case 2:
                    stateMachine.ChangeState(new AngryState());
                    break;
            }
        }

        public void TakeDamage(int damage = 1)
        {
            Debug.Log($"[BossController] TakeDamage called - checking CanTakeDamage()...");
            // Kiểm tra state có cho phép nhận damage không TRƯỚC KHI trừ máu
            if (stateMachine.CanTakeDamage())
            {
                Debug.Log($"[BossController] CanTakeDamage = TRUE - Boss will take {damage} damage");
                healthSystem.TakeDamage(damage);
                stateMachine.OnTakeDamage();
                
                BossEventSystem.Trigger(BossEventType.BossTakeDamage, new BossEventData(damage));
                
                // FMOD: damage SFX
                PlayFMODOneShot(bossConfig.fmodAudioConfig.damageEvent);
            }
            else
            {
                stateMachine.OnTakeDamage();
            }
        }

        // Method riêng cho damage từ decoys - bypass CanTakeDamage() check
        public void TakeDamageFromDecoy(int damage = 1)
        {
            healthSystem.TakeDamage(damage);
            BossEventSystem.Trigger(BossEventType.BossTakeDamage, new BossEventData(damage));
            
            // FMOD: damage SFX
            PlayFMODOneShot(bossConfig.fmodAudioConfig.damageEvent);
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

        private void DefeatBoss()
        {
            // Change to cook state for phase 2
            ChangeState(new CookState());
            
            // FMOD: defeat SFX
            PlayFMODOneShot(bossConfig.fmodAudioConfig.defeatEvent);
            
            // Optional callback for external systems
            OnBossDefeated?.Invoke();
        }

        public void PlayAnimation(string animationName)
        {
            Debug.Log($"[Boss Animation] Playing animation: {animationName}");

            if (animator != null)
            {
                animator.SetTrigger(animationName);
            }
        }

        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            // Legacy AudioClip path (kept for backward compatibility)
            if (audioSource && clip)
            {
                audioSource.PlayOneShot(clip, volume * bossConfig.audioConfig.masterVolume);
            }
        }

        // --- FMOD Helpers ---
        public void PlayFMODOneShot(EventReference evt)
        {
            if (!evt.IsNull)
            {
                RuntimeManager.PlayOneShot(evt, transform.position);
            }
        }

        public void PlayFMODOneShotAtPosition(EventReference evt, Vector3 position)
        {
            if (!evt.IsNull)
            {
                RuntimeManager.PlayOneShot(evt, position);
            }
        }

        // Heartbeat loop control used by FearZone
        public void StartHeartbeatLoop()
        {
            if (heartbeatActive) return;
            var evt = bossConfig.fmodAudioConfig.heartbeatEvent;
            if (evt.IsNull) return;
            heartbeatInstance = RuntimeManager.CreateInstance(evt);
            // Use recommended overload with GameObject to avoid obsolete warning
            RuntimeManager.AttachInstanceToGameObject(heartbeatInstance, gameObject);
            heartbeatInstance.start();
            heartbeatActive = true;
        }

        public void StopHeartbeatLoop()
        {
            if (!heartbeatActive) return;
            heartbeatInstance.stop(STOP_MODE.ALLOWFADEOUT);
            heartbeatInstance.release();
            heartbeatActive = false;
        }

        // --- Animation Parameters for Move States ---
        public void SetMoveDirection(float x, float y)
        {
            if (animator == null) return;
            animator.SetFloat(MoveXHash, x);
            animator.SetFloat(MoveYHash, y);
        }
        public void ResetMoveDirection()
        {
            if (animator == null) return;
            animator.SetFloat(MoveXHash, 0f);
            animator.SetFloat(MoveYHash, 0f);
        }

        // Xử lý va chạm với Bullet
        private void OnTriggerEnter(Collider other)
        {
            if (!enabled) return;
            if (other.CompareTag("Bullet"))
            {
                TakeDamage();
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
            
            // Unsubscribe events
            BossEventSystem.Unsubscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
            
            // Stop any looping FMOD instances
            StopHeartbeatLoop();
            
            ClearDecoys();
            // Removed ClearAllListeners to avoid wiping unrelated listeners globally
        }

        /// <summary>
        /// Reset boss về trạng thái ban đầu (dùng khi restart game)
        /// </summary>
        public void ResetBoss()
        {
            Debug.Log("[BossController] Resetting boss to initial state");
            
            // Reset health through health system
            healthSystem.ResetPhaseHealth();
            
            // Reset phase
            currentPhase = 1;
            
            // Reset position
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                navMeshAgent.enabled = true;
            }
            
            // Clear decoys
            ClearDecoys();
            
            // Reset state machine to initial state
            ChangeToPhase(1);
            
            // Clear any ongoing effects
            StopAllCoroutines();
            
            // Stop any looping FMOD instances
            StopHeartbeatLoop();
            
            // Clear soul manager
            if (soulManager != null)
            {
                soulManager.DestroyAllSouls();
            }
            
            Debug.Log($"[BossController] Boss reset - Phase: {currentPhase}");
        }

        private void OnPlayerDefeatedEvent(BossEventData data)
        {
            // Stop looping audio and pause boss AI when player is defeated
            StopHeartbeatLoop();
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
            enabled = false; // stop Update()
        }
    }
}
