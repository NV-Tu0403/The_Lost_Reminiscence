
namespace Code.Boss
{
    /// <summary>
    /// Base class cho tất cả các state của boss
    /// </summary>
    public abstract class BossState
    {
        protected BossController BossController;
        protected BossConfig Config;
        
        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract void OnTakeDamage();
        public abstract bool CanBeInterrupted();
        
        public virtual bool CanTakeDamage()
        {
            return BossController != null && BossController.CurrentPhase >= 2;
        }

        public virtual void Initialize(BossController controller, BossConfig bossConfig)
        {
            BossController = controller;
            Config = bossConfig;
        }
    }

    /// <summary>
    /// Finite State Machine cho Boss
    /// </summary>
    public class BossStateMachine
    {
        private BossState currentState;
        private BossController bossController;

        public void Initialize(BossController controller)
        {
            bossController = controller;
        }

        public void ChangeState(BossState newState)
        {
            currentState?.Exit();
            
            var previousState = currentState;
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

        public bool CanTakeDamage()
        {
            return currentState?.CanTakeDamage() ?? false;
        }
        
        public bool CanBeInterrupted()
        {
            return currentState?.CanBeInterrupted() ?? true; 
        }
    }
}
