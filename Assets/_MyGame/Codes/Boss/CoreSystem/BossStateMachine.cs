namespace _MyGame.Codes.Boss.CoreSystem
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
        private BossState _currentState;
        private BossController _bossController;

        public void Initialize(BossController controller)
        {
            _bossController = controller;
        }

        public void ChangeState(BossState newState)
        {
            _currentState?.Exit();
            
            var previousState = _currentState;
            _currentState = newState;
            if (_currentState != null)
            {
                _currentState.Initialize(_bossController, _bossController.Config); // Set config
                _currentState.Enter(); 
            }
            
            // Trigger state change event
            BossEventSystem.Trigger(BossEventType.StateChanged, 
                new BossEventData { stringValue = newState?.GetType().Name });
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void OnTakeDamage()
        {
            _currentState?.OnTakeDamage();
        }

        public bool CanTakeDamage()
        {
            return _currentState?.CanTakeDamage() ?? false;
        }
        
        public bool CanBeInterrupted()
        {
            return _currentState?.CanBeInterrupted() ?? true; 
        }
    }
}
