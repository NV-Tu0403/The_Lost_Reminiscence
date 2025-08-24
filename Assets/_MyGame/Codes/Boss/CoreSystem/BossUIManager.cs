using _MyGame.Codes.Boss.UI;
using Code.Boss;
using UnityEngine;

namespace _MyGame.Codes.Boss.CoreSystem
{
    /// <summary>
    /// Quản lý UI của Boss
    /// </summary>
    public class BossUIManager
    {
        private BossController bossController;
        private BossHealthBar bossHealthBar;
        private BossSkillCastBar skillCastBar;
        private PlayerHealthBar playerHealthBar;
        private Notification defeatNotification;
        
        
        public BossUIManager(BossController controller)
        {
            bossController = controller;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Tìm và khởi tạo UI components đã được gán trong scene
            FindAndInitializeUIComponents();
        }

        private void FindAndInitializeUIComponents()
        {
            // Tìm BossHealthBar trong scene
            bossHealthBar = Object.FindFirstObjectByType<BossHealthBar>();
            if (bossHealthBar != null)
            {
                bossHealthBar.Initialize(bossController);
                Debug.Log("Boss Health Bar found and initialized");
            }
            else
            {
                Debug.LogWarning("BossHealthBar not found in scene! Please add BossHealthBar component to a UI GameObject.");
            }

            // Tìm BossSkillCastBar trong scene
            skillCastBar = Object.FindFirstObjectByType<BossSkillCastBar>();
            if (skillCastBar != null)
            {
                skillCastBar.Initialize(bossController);
                Debug.Log("Boss Skill Cast Bar found and initialized");
            }
            else
            {
                Debug.LogWarning("BossSkillCastBar not found in scene! Please add BossSkillCastBar component to a UI GameObject.");
            }

            // Tìm PlayerHealthBar trong scene
            playerHealthBar = Object.FindFirstObjectByType<PlayerHealthBar>();
            if (playerHealthBar != null)
            {
                playerHealthBar.Initialize(3, bossController.Config); 
                Debug.Log("Player Health Bar found and initialized");
            }
            else
            {
                Debug.LogWarning("PlayerHealthBar not found in scene! Please add PlayerHealthBar component to a UI GameObject.");
            }

            // Tìm BossDefeatNotification trong scene
            defeatNotification = Object.FindFirstObjectByType<Notification>();
            if (defeatNotification != null)
            {
                Debug.Log("Boss Defeat Notification found and initialized");
            }
            else
            {
                Debug.LogWarning("BossDefeatNotification not found in scene! Please add BossDefeatNotification component to a UI GameObject.");
            }
        }
    }
}
