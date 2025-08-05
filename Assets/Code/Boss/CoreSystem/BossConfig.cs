using System;
using FMODUnity;
using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Cấu hình toàn bộ boss - File config duy nhất để điều chỉnh toàn bộ boss
    /// </summary>
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Boss/Boss Configuration")]
    public class BossConfig : ScriptableObject
    {
        [Header("Debug")]
        [Tooltip("Chọn phase để test nhanh. None: Bình thường, Phase1: Phase 1, Phase2: Phase 2, ...")]
        public BossDebugPhase debugStartPhase = BossDebugPhase.None;
     
        [Header("Prefab Settings")]
        [Tooltip("Prefab của boss để spawn lại khi cần reset")]
        public GameObject bossPrefab;
        [Tooltip("Prefab memory fragment rớt ra khi boss chết")]
        public GameObject memoryFragmentPrefab;
        [Tooltip("Effect xung quanh memory fragment")]
        public GameObject memoryFragmentEffectPrefab;
        
        [Header("General Boss Settings")]
        public int maxHealthPerPhase = 3;
        public float moveSpeed = 5f;
        public float rotationSpeed = 90f;
        
        [Header("Phase 1 Settings")]
        [Space]
        public Phase1Config phase1;
        
        [Header("Phase 2 Settings")]
        [Space]
        public Phase2Config phase2;
        
        [Header("Soul Settings")]
        [Space]
        public SoulConfig soulConfig;
        
        [Header("UI Settings")]
        [Space]
        public UIConfig uiConfig;
        
        [Header("Audio Settings")]
        [Space]
        public AudioConfig audioConfig;
        
        [Header("FMOD Studio Settings")]
        [Space]
        public FMODAudioConfig fmodAudioConfig;
    }

    [Serializable]
    public class Phase1Config
    {
        [Header("State Durations")]
        public float idleDuration = 2f;
        public float lureDuration = 3f;
        public float mockDuration = 2f;
        public float decoyCastTime = 2f;
        public float decoyDuration = 10f;
        public float soulStateCastTime = 1.5f;
        
        [Header("Lure State")]
        public float lureDistance = 5f;
        public float lureApproachSpeed = 3f;
        public float lureRetreatSpeed = 4f;
        
        [Header("Decoy State")]
        public float decoyMoveSpeed = 2f;
        public int decoyCount = 2; // 1 thật, 1 giả
        public float decoySpawnRadius = 8f;
        public GameObject decoyPrefab; // Prefab cho decoy
        public GameObject decoySpawnEffectPrefab; // Prefab hiệu ứng khi spawn decoy
        public GameObject realDecoyRevealEffectPrefab; // Prefab hiệu ứng khi reveal decoy thật
    }

    [Serializable]
    public class Phase2Config
    {
        [Header("State Durations")]
        public float angryMoveDuration = 5f;
        public float fearZoneCastTime = 2f;
        public float fearZoneDuration = 8f;
        public float screamCastTime = 3f;
        public float screamDuration = 5f;
        public float cookStateDuration = 3f;
        
        [Header("Angry State")]
        public float angryMoveSpeed = 3f;
        public float circleRadius = 10f;
        
        [Header("Fear Zone")]
        public float fearZoneRadius = 3f;
        public GameObject fearZoneCastEffectPrefab; // Prefab hiệu ứng khi cast skill
        public GameObject fearZoneZoneEffectPrefab; // Prefab hiệu ứng khi zone xuất hiện
        
        [Header("Scream State")]
        public float screenShakeIntensity = 1f;
        public float visionShrinkAmount = 0.5f;
    }

    [Serializable]
    public class SoulConfig
    {
        public int maxSouls = 2;
        public float soulMoveSpeed = 4f;
        public float soulSpawnRadius = 15f;
        public float soulFollowDistance = 1f;
        public GameObject soulPrefab;
        public GameObject soulSpawnEffectPrefab; // Prefab hiệu ứng khi spawn soul
    }

    [Serializable]
    public class UIConfig
    {
        [Header("Colors")]
        public Color bossHealthColor = Color.red;
        public Color playerHealthColor = Color.green;
        public Color skillCastColor = Color.yellow;
        
        [Header("Animation Settings")]
        public float uiAnimationSpeed = 1f;                                                                     // Tốc độ animation chung cho UI
        public AnimationCurve uiAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);          // Curve cho smooth animation
    }

    [Serializable]
    public class AudioConfig
    {
        [Header("Phase 1 Audio")]
        public AudioClip mockLaughSound;
        public AudioClip decoySpawnSound;
        public AudioClip soulSpawnSound;
        
        [Header("Phase 2 Audio")]
        public AudioClip screamSound;
        public AudioClip fearZoneSound;
        public AudioClip heartbeatSound;
        
        [Header("General Audio")]
        public AudioClip phaseChangeSound;
        public AudioClip damageSound;
        public AudioClip defeatSound;
        
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float ambientVolume = 0.6f;
    }
    
    [Serializable]
    public class FMODAudioConfig
    {
        [Header("Phase 1 Audio")]
        public EventReference mockLaughEvent;
        public EventReference decoySpawnEvent;
        public EventReference soulSpawnEvent;
        [Header("Phase 2 Audio")]
        public EventReference screamEvent;
        public EventReference fearZoneEvent;
        public EventReference heartbeatEvent;
        [Header("General Audio")]
        public EventReference phaseChangeEvent;
        public EventReference damageEvent;
        public EventReference defeatEvent;
    }

    public enum BossDebugPhase
    {
        None = -1,
        Phase1 = 0,
        Phase2 = 1
    }
}
