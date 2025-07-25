using UnityEngine;
using Code.Boss.States.Phase1;
using Code.Boss.States.Phase2;
using Code.Boss.States.Shared;

namespace Code.Boss
{
    /// <summary>
    /// Base class cho tất cả các state của boss
    /// </summary>
    public abstract class BossState
    {
        protected BossController bossController;
        protected BossConfig config;
        
        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract void OnTakeDamage();
        public abstract bool CanBeInterrupted();
        
        // Thêm method để kiểm tra state có cho phép boss nhận damage không
        public virtual bool CanTakeDamage()
        {
            // Mặc định: Phase 1 không thể damage trực tiếp, Phase 2 có thể
            return bossController != null && bossController.CurrentPhase >= 2;
        }

        public virtual void Initialize(BossController controller, BossConfig bossConfig)
        {
            bossController = controller;
            config = bossConfig;
        }
    }

    /// <summary>
    /// Finite State Machine cho Boss
    /// </summary>
    public class BossStateMachine
    {
        private BossState currentState;
        private BossController bossController;
        
        public BossState CurrentState => currentState;

        public void Initialize(BossController controller)
        {
            bossController = controller;
        }

        public void ChangeState(BossState newState)
        {
            currentState?.Exit();
            
            BossState previousState = currentState;
            currentState = newState;
            if (currentState != null)
            {
                currentState.Initialize(bossController, bossController.Config); // Set config
                currentState.Enter(); 
            }
            
            // Trigger state change event
            BossEventSystem.Trigger(BossEventType.StateChanged, 
                new BossEventData { stringValue = newState?.GetType().Name });
        }

        public void Update()
        {
            currentState?.Update();
        }

        public void OnTakeDamage()
        {
            currentState?.OnTakeDamage();
        }

        public bool CanInterruptCurrentState()
        {
            return currentState?.CanBeInterrupted() ?? false;
        }
        
        // Thêm method để kiểm tra state hiện tại có cho phép boss nhận damage không
        public bool CanTakeDamage()
        {
            return currentState?.CanTakeDamage() ?? false;
        }
    }
}
