using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Interface tích hợp với hệ thống kỹ năng của Fa
    /// </summary>
    public class FaBossIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool enableFaIntegration = true;
        
        private BossManager bossManager;
        
        // Events để thông báo cho Fa system
        public static System.Action<string> OnRequestFaSkill;
        public static System.Action<int> OnSoulCountChanged;
        public static System.Action<bool> OnBossVulnerable; // Khi boss có thể bị tấn công

        private void Start()
        {
            bossManager = BossManager.Instance;
            
            if (enableFaIntegration)
            {
                RegisterForBossEvents();
            }
        }

        private void RegisterForBossEvents()
        {
            // Lắng nghe các sự kiện boss để thông báo cho Fa
            BossEventSystem.Subscribe(BossEventType.SoulSpawned, OnSoulSpawned);
            BossEventSystem.Subscribe(BossEventType.SoulDestroyed, OnSoulDestroyed);
            BossEventSystem.Subscribe(BossEventType.DecoyStarted, OnDecoyStarted);
            BossEventSystem.Subscribe(BossEventType.ScreamStarted, OnScreamStarted);
            BossEventSystem.Subscribe(BossEventType.RequestRadarSkill, OnRadarSkillRequested);
        }

        private void OnSoulSpawned(BossEventData data)
        {
            // Thông báo cho Fa rằng có soul mới được spawn
            int currentSoulCount = FindObjectsOfType<SoulBehavior>().Length;
            OnSoulCountChanged?.Invoke(currentSoulCount);
            
            // Nếu có >= 2 souls, suggest Fa use Radar skill
            if (currentSoulCount >= 2)
            {
                SuggestRadarSkill();
            }
        }

        private void OnSoulDestroyed(BossEventData data)
        {
            int currentSoulCount = FindObjectsOfType<SoulBehavior>().Length - 1; // -1 vì soul sắp bị destroy
            OnSoulCountChanged?.Invoke(currentSoulCount);
        }

        private void OnDecoyStarted(BossEventData data)
        {
            // Boss bắt đầu cast Decoy - đây là thời điểm boss có thể bị tấn công
            OnBossVulnerable?.Invoke(true);
        }

        private void OnScreamStarted(BossEventData data)
        {
            // Boss bắt đầu cast Scream - đây cũng là thời điểm boss có thể bị tấn công
            OnBossVulnerable?.Invoke(true);
        }

        private void OnRadarSkillRequested(BossEventData data)
        {
            SuggestRadarSkill();
        }

        private void SuggestRadarSkill()
        {
            // Gửi signal cho Fa system để suggest sử dụng Radar skill
            OnRequestFaSkill?.Invoke("Radar");
            
            Debug.Log("Suggesting Fa to use Radar skill to clear souls");
        }

        // Method để Fa system gọi khi skill được sử dụng
        public static void NotifyFaSkillUsed(string skillName, bool success = true)
        {
            if (BossManager.Instance != null)
            {
                BossManager.Instance.OnFaSkillCompleted(skillName, success);
            }
        }

        // Method để Fa system kiểm tra trạng thái boss
        public static bool IsBossVulnerable()
        {
            if (BossManager.Instance == null) return false;
            
            string currentState = BossManager.Instance.GetCurrentBossState();
            return currentState == "DecoyState" || currentState == "ScreamState";
        }

        // Method để Fa system kiểm tra số lượng soul
        public static int GetCurrentSoulCount()
        {
            return FindObjectsOfType<SoulBehavior>().Length;
        }

        // Method để Fa system kiểm tra phase hiện tại
        public static int GetCurrentBossPhase()
        {
            return BossManager.Instance?.GetCurrentBossPhase() ?? 0;
        }

        private void OnDestroy()
        {
            BossEventSystem.Unsubscribe(BossEventType.SoulSpawned, OnSoulSpawned);
            BossEventSystem.Unsubscribe(BossEventType.SoulDestroyed, OnSoulDestroyed);
            BossEventSystem.Unsubscribe(BossEventType.DecoyStarted, OnDecoyStarted);
            BossEventSystem.Unsubscribe(BossEventType.ScreamStarted, OnScreamStarted);
            BossEventSystem.Unsubscribe(BossEventType.RequestRadarSkill, OnRadarSkillRequested);
        }
    }

    /// <summary>
    /// Example integration với Fa skill system
    /// </summary>
    public static class FaSkillIntegrationExample
    {
        // Ví dụ cách Fa system có thể tích hợp
        public static void InitializeFaIntegration()
        {
            // Đăng ký lắng nghe boss events
            FaBossIntegration.OnRequestFaSkill += HandleFaSkillRequest;
            FaBossIntegration.OnSoulCountChanged += HandleSoulCountChanged;
            FaBossIntegration.OnBossVulnerable += HandleBossVulnerabilityChanged;
        }

        private static void HandleFaSkillRequest(string skillName)
        {
            Debug.Log($"Boss system is requesting Fa to use skill: {skillName}");
            
            // Fa system logic here
            // Example:
            // if (FaSkillManager.CanUseSkill(skillName))
            // {
            //     FaSkillManager.UseSkill(skillName, OnFaSkillComplete);
            // }
        }

        private static void HandleSoulCountChanged(int soulCount)
        {
            Debug.Log($"Soul count changed to: {soulCount}");
            
            // Fa system có thể update UI hoặc AI logic dựa trên số soul
        }

        private static void HandleBossVulnerabilityChanged(bool isVulnerable)
        {
            Debug.Log($"Boss vulnerability changed to: {isVulnerable}");
            
            // Fa system có thể update UI để hiển thị cơ hội tấn công
        }

        // Callback khi Fa skill hoàn thành
        private static void OnFaSkillComplete(string skillName, bool success)
        {
            // Thông báo lại cho boss system
            FaBossIntegration.NotifyFaSkillUsed(skillName, success);
        }

        public static void CleanupFaIntegration()
        {
            FaBossIntegration.OnRequestFaSkill -= HandleFaSkillRequest;
            FaBossIntegration.OnSoulCountChanged -= HandleSoulCountChanged;
            FaBossIntegration.OnBossVulnerable -= HandleBossVulnerabilityChanged;
        }
    }
}
