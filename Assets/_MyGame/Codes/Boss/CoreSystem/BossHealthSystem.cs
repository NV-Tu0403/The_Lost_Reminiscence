using System;
using UnityEngine;

namespace _MyGame.Codes.Boss.CoreSystem
{
    /// <summary>
    /// Quản lý hệ thống máu của Boss
    /// </summary>
    public class BossHealthSystem
    {
        private int maxHealthPerPhase;
        private int currentHealth;
        private int currentPhase = 1;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealthPerPhase => maxHealthPerPhase;
        public int CurrentPhase => currentPhase;
        public float HealthPercentage => (float)currentHealth / maxHealthPerPhase;
        
        public event Action<int, int> OnHealthChanged; // current, max
        public event Action OnPhaseHealthDepleted;

        public BossHealthSystem(int maxHealthPerPhase)
        {
            this.maxHealthPerPhase = maxHealthPerPhase;
            currentHealth = maxHealthPerPhase;
        }

        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealthPerPhase);
            
            if (currentHealth <= 0)
            {
                OnPhaseHealthDepleted?.Invoke();
            }
        }

        public void ResetPhaseHealth()
        {
            currentHealth = maxHealthPerPhase;
            currentPhase++;
            OnHealthChanged?.Invoke(currentHealth, maxHealthPerPhase);
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealthPerPhase, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealthPerPhase);
        }
    }
}
