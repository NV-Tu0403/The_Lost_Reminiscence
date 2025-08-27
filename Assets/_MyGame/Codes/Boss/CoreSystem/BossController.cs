using System.Collections.Generic;
using _MyGame.Codes.Boss.States.Phase1;
using _MyGame.Codes.Boss.States.Phase2;
using Code.Boss;
using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

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
        private BossStateMachine _stateMachine;
        private BossHealthSystem _healthSystem;
        private BossSoulManager _soulManager;
        private BossUIManager _uiManager;
        
        // Current Phase
        private int _currentPhase = 1;
        private readonly List<GameObject> _currentDecoys = new List<GameObject>();
        
        // Initial position and rotation for reset
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        // FMOD: cached looping instances
        private EventInstance _heartbeatInstance;
        private bool _heartbeatActive = false;
        // Single Boss BGM instance (runs across all phases)
        private EventInstance _spawnBGMInstance;
        private bool _spawnBGMActive = false;
        // Phase BGM instances
        private EventInstance _phase1BGMInstance;
        private bool _phase1BGMActive = false;
        private EventInstance _phase2BGMInstance;
        private bool _phase2BGMActive = false;
        // Phase change follow VFX instance
        private GameObject _phaseChangeFollowEffectInstance;
        
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
        public int CurrentPhase => _currentPhase;
        public BossHealthSystem HealthSystem => _healthSystem;
        public BossSoulManager SoulManager => _soulManager;
        public List<GameObject> CurrentDecoys => _currentDecoys;
        
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
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            
            InitializeSystems();
            StartBoss();
        }

        private void Update()
        {
            _stateMachine?.Update();
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
            _stateMachine = new BossStateMachine();
            _stateMachine.Initialize(this);
            
            // Initialize Health System
            _healthSystem = new BossHealthSystem(bossConfig.maxHealthPerPhase);
            _healthSystem.OnHealthChanged += OnHealthChanged;
            _healthSystem.OnPhaseHealthDepleted += OnPhaseCompleted;
            
            // Initialize Soul Manager
            _soulManager = new BossSoulManager(this);
            
            // Initialize UI Manager
            _uiManager = new BossUIManager(this);
            
            // Setup NavMesh
            if (navMeshAgent == null) return;
            navMeshAgent.speed = bossConfig.moveSpeed;
            navMeshAgent.angularSpeed = bossConfig.rotationSpeed;
        }

        private void StartBoss()
        {
            BossEventSystem.Trigger(BossEventType.BossSpawned);
            // Start spawn BGM
            StartSpawnBGM();
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
            switch (_currentPhase)
            {
                case 1:
                    // Chuyển sang state chuyển phase để hiển thị hiệu ứng
                    ChangeState(new _MyGame.Codes.Boss.States.Shared.PhaseChangeState());
                    break;
                case 2:
                    DefeatBoss();
                    break;
            }
        }

        public void ChangeToPhase(int newPhase)
        {
            _currentPhase = newPhase;
            _healthSystem.ResetPhaseHealth();
            
            // Clear any existing decoys
            ClearDecoys();
            
            // Trigger phase change event
            BossEventSystem.Trigger(BossEventType.PhaseChanged, new BossEventData(newPhase));
            OnPhaseChanged?.Invoke(newPhase);

            switch (newPhase)
            {
                // Start/Stop phase music as needed
                case 1:
                    // Ensure phase 2 music is off (debug path back to P1)
                    StopPhase2BGM();
                    StartPhase1BGM();
                    break;
                case 2:
                    // Ensure music is correct even when jumping directly to P2 (debug)
                    StopPhase1BGM();
                    StartPhase2BGM();
                    break;
            }

            switch (newPhase)
            {
                case 1:
                    _stateMachine.ChangeState(new IdleState());
                    break;
                case 2:
                    _stateMachine.ChangeState(new AngryState());
                    break;
            }
        }

        public void TakeDamage(int damage = 1)
        {
            Debug.Log($"[BossController] TakeDamage called - checking CanTakeDamage()...");
            // Kiểm tra state có cho phép nhận damage không TRƯỚC KHI trừ máu
            if (_stateMachine.CanTakeDamage())
            {
                Debug.Log($"[BossController] CanTakeDamage = TRUE - Boss will take {damage} damage");
                _healthSystem.TakeDamage(damage);
                _stateMachine.OnTakeDamage();
                
                BossEventSystem.Trigger(BossEventType.BossTakeDamage, new BossEventData(damage));
                
                // FMOD: damage SFX
                PlayFMODOneShot(bossConfig.fmodAudioConfig.damageEvent);
            }
            else
            {
                _stateMachine.OnTakeDamage();
            }
        }

        // Method riêng cho damage từ decoys - bypass CanTakeDamage() check
        public void TakeDamageFromDecoy(int damage = 1)
        {
            _healthSystem.TakeDamage(damage);
            BossEventSystem.Trigger(BossEventType.BossTakeDamage, new BossEventData(damage));
            
            // FMOD: damage SFX
            PlayFMODOneShot(bossConfig.fmodAudioConfig.damageEvent);
        }

        public void ChangeState(BossState newState)
        {
            newState.Initialize(this, bossConfig);
            _stateMachine.ChangeState(newState);
        }

        public void AddDecoy(GameObject decoy)
        {
            _currentDecoys.Add(decoy);
        }

        public void RemoveDecoy(GameObject decoy)
        {
            _currentDecoys.Remove(decoy);
        }

        public void ClearDecoys()
        {
            foreach (var decoy in _currentDecoys)
            {
                if (decoy != null)
                    Destroy(decoy);
            }
            _currentDecoys.Clear();
        }

        private void DefeatBoss()
        {
            // Change to cook state for phase 2
            ChangeState(new CookState());
            
            // FMOD: defeat SFX
            PlayFMODOneShot(bossConfig.fmodAudioConfig.defeatEvent);
            
            // Stop all BGMs
            StopAllBossBgMs();
            
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
            if (_heartbeatActive) return;
            var evt = bossConfig.fmodAudioConfig.heartbeatEvent;
            if (evt.IsNull) return;
            _heartbeatInstance = RuntimeManager.CreateInstance(evt);
            // Use recommended overload with GameObject to avoid obsolete warning
            RuntimeManager.AttachInstanceToGameObject(_heartbeatInstance, gameObject);
            _heartbeatInstance.start();
            _heartbeatActive = true;
        }

        public void StopHeartbeatLoop()
        {
            if (!_heartbeatActive) return;
            _heartbeatInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _heartbeatInstance.release();
            _heartbeatActive = false;
        }

        // --- Boss BGM control ---
        private void StartSpawnBGM()
        {
            if (_spawnBGMActive) return;
            var evt = bossConfig.fmodAudioConfig.bossSpawnBGMEvent;
            if (evt.IsNull) return;
            _spawnBGMInstance = RuntimeManager.CreateInstance(evt);
            RuntimeManager.AttachInstanceToGameObject(_spawnBGMInstance, gameObject);
            _spawnBGMInstance.start();
            _spawnBGMActive = true;
        }

        private void StopSpawnBGM()
        {
            if (!_spawnBGMActive) return;
            _spawnBGMInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _spawnBGMInstance.release();
            _spawnBGMActive = false;
        }

        // Expose phase music controls for PhaseChangeState
        private void StartPhase1BGM()
        {
            if (_phase1BGMActive) return;
            var evt = bossConfig.fmodAudioConfig.bossPhase1BGMEvent;
            if (evt.IsNull) return;
            _phase1BGMInstance = RuntimeManager.CreateInstance(evt);
            RuntimeManager.AttachInstanceToGameObject(_phase1BGMInstance, gameObject);
            _phase1BGMInstance.start();
            _phase1BGMActive = true;
        }

        private void StopPhase1BGM()
        {
            if (!_phase1BGMActive) return;
            _phase1BGMInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _phase1BGMInstance.release();
            _phase1BGMActive = false;
        }

        private void StartPhase2BGM()
        {
            if (_phase2BGMActive) return;
            var evt = bossConfig.fmodAudioConfig.bossPhase2BGMEvent;
            if (evt.IsNull) return;
            _phase2BGMInstance = RuntimeManager.CreateInstance(evt);
            RuntimeManager.AttachInstanceToGameObject(_phase2BGMInstance, gameObject);
            _phase2BGMInstance.start();
            _phase2BGMActive = true;
        }

        private void StopPhase2BGM()
        {
            if (!_phase2BGMActive) return;
            _phase2BGMInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _phase2BGMInstance.release();
            _phase2BGMActive = false;
        }

        private void StopAllBossBgMs()
        {
            StopSpawnBGM();
            StopPhase1BGM();
            StopPhase2BGM();
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
            if (_healthSystem != null)
            {
                _healthSystem.OnHealthChanged -= OnHealthChanged;
                _healthSystem.OnPhaseHealthDepleted -= OnPhaseCompleted;
            }
            
            // Unsubscribe events
            BossEventSystem.Unsubscribe(BossEventType.PlayerDefeated, OnPlayerDefeatedEvent);
            
            // Stop any looping FMOD instances
            StopHeartbeatLoop();
            StopAllBossBgMs();
            
            // Destroy follow effect if exists
            DestroyPhaseChangeFollowEffect();
            
            ClearDecoys();
        }

        /// <summary>
        /// Reset boss về trạng thái ban đầu (dùng khi restart game)
        /// </summary>
        public void ResetBoss()
        {
            Debug.Log("[BossController] Resetting boss to initial state");
            
            // Reset health through health system
            _healthSystem.ResetPhaseHealth();
            
            // Reset phase
            _currentPhase = 1;
            
            // Reset position
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
                transform.position = _initialPosition;
                transform.rotation = _initialRotation;
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
            StopAllBossBgMs();
            
            // Destroy follow effect if exists
            DestroyPhaseChangeFollowEffect();
            
            // Clear soul manager
            if (_soulManager != null)
            {
                _soulManager.DestroyAllSouls();
            }
            
            Debug.Log($"[BossController] Boss reset - Phase: {_currentPhase}");
        }

        // --- Phase change follow effect ---
        public void SpawnPhaseChangeFollowEffect()
        {
            if (_phaseChangeFollowEffectInstance != null) return;
            var prefab = bossConfig != null ? bossConfig.phaseChangeFollowEffectPrefab : null;
            if (prefab == null)
            {
                Debug.LogWarning("[BossController] phaseChangeFollowEffectPrefab is not assigned in BossConfig");
                return;
            }
            _phaseChangeFollowEffectInstance = Instantiate(prefab, transform.position, transform.rotation, transform);
            _phaseChangeFollowEffectInstance.transform.localPosition = Vector3.zero;
        }

        private void DestroyPhaseChangeFollowEffect()
        {
            if (_phaseChangeFollowEffectInstance == null) return;
            Destroy(_phaseChangeFollowEffectInstance);
            _phaseChangeFollowEffectInstance = null;
        }

        private void OnPlayerDefeatedEvent(BossEventData data)
        {
            // Stop looping audio and pause boss AI when player is defeated
            StopHeartbeatLoop();
            StopAllBossBgMs();
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
            enabled = false; // stop Update()
        }
    }
}
