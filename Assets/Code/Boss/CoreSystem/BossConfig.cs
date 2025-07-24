using System;
using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Cấu hình toàn bộ boss - File config duy nhất để điều chỉnh toàn bộ boss
    /// </summary>
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Boss/Boss Configuration")]
    public class BossConfig : ScriptableObject
    {
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
        
        [Header("State Randomization")]
        public bool enableRandomStates = true;
        public float[] stateWeights = { 1f, 1f, 1f, 1f }; // Idle, Lure, Mock, Decoy
    }

    [Serializable]
    public class Phase2Config
    {
        [Header("State Durations")]
        public float angryMoveDuration = 5f;
        public float fearZoneCastTime = 2f;
        public float fearZoneDuration = 8f;
        public float fearZoneActivationTime = 3f;
        public float screamCastTime = 3f;
        public float screamDuration = 5f;
        public float cookStateDuration = 3f;
        
        [Header("Angry State")]
        public float angryMoveSpeed = 3f;
        public float circleRadius = 10f;
        
        [Header("Fear Zone")]
        public float fearZoneRadius = 3f;
        public float visionBlurIntensity = 0.7f;
        
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
    }

    [Serializable]
    public class UIConfig
    {
        [Header("Health Bar")]
        public Vector2 bossHealthPosition = new Vector2(0f, 0.8f); // Screen space
        public Vector2 playerHealthPosition = new Vector2(0f, -0.8f);
        public Vector2 healthBarSize = new Vector2(300f, 30f);
        
        [Header("Skill Cast Bar")]
        public Vector2 skillCastBarOffset = new Vector2(0f, -50f); // Offset from boss health
        public Vector2 skillCastBarSize = new Vector2(200f, 20f);
        
        [Header("Colors")]
        public Color bossHealthColor = Color.red;
        public Color playerHealthColor = Color.green;
        public Color skillCastColor = Color.yellow;
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
}
