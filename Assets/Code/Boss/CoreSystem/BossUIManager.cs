using UnityEngine;

namespace Code.Boss
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
                playerHealthBar.Initialize(3); // Default player health
                Debug.Log("Player Health Bar found and initialized");
            }
            else
            {
                Debug.LogWarning("PlayerHealthBar not found in scene! Please add PlayerHealthBar component to a UI GameObject.");
            }
        }

        // Methods để show/hide UI
        // public void ShowBossHealthBar(bool show)
        // {
        //     if (bossHealthBar != null)
        //         bossHealthBar.gameObject.SetActive(show);
        // }
        //
        // public void ShowSkillCastBar(bool show)
        // {
        //     if (skillCastBar != null)
        //         skillCastBar.gameObject.SetActive(show);
        // }
        //
        // public void ShowPlayerHealthBar(bool show)
        // {
        //     if (playerHealthBar != null)
        //         playerHealthBar.gameObject.SetActive(show);
        // }
    }
}
