using System;

/// <summary>
/// Giao diện bắt buộc Khi triển khai State.
/// </summary>
public interface IState
{
    void OnEnter();
    void OnExit();
}

public interface IUIInteractiveState : IState
{
    void HandleAction(UIActionType action);
}

/// <summary>
/// Điều phối các trạng thái trong giao diện người dùng.
/// </summary>
public class StateMachine
{
    /// <summary>
    /// Trạng thái hiện tại của StateMachine.
    /// </summary>
    private IUIInteractiveState currentState;

    /// <summary>
    /// Khởi tạo StateMachine với trạng thái ban đầu.
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(IUIInteractiveState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public void HandleAction(UIActionType action)
    {
        currentState?.HandleAction(action);
    }

    /// <summary>
    /// Lấy trạng thái hiện tại.
    /// </summary>
    public Type CurrentStateType => currentState?.GetType();
}

/// <summary>
/// bộ trạng thái base game
/// Kế thừa lớp này để tạo các trạng thái cụ thể.
/// 
/// logic cụ thể của từng State được triển khai trong HandleAction.
/// </summary>
public abstract class StateBase : IUIInteractiveState
{
    protected readonly StateMachine _stateMachine;
    protected CoreEvent _coreEvent; // vì iem không muống kế thừa CoreEvent nên iem lưu Reference CoreEvent để dùng

    public CoreStateType StateType { get; protected set; } // state enum

    protected StateBase(StateMachine stateMachine, CoreEvent coreEvent, CoreStateType stateType)
    {
        _stateMachine = stateMachine;
        _coreEvent = coreEvent;
        StateType = stateType;
    }

    public virtual void OnEnter()
    {
        //Debug.Log($"[UIState] Entered: {StateType}");
        _coreEvent.TriggerChangeState(StateType);
    }

    public virtual void OnExit()
    {
        //Debug.Log($"[UIState] Exited: {StateType}");
        _coreEvent.TriggerChangeState(StateType);
    }

    // Bắt buộc lớp con triển khai
    public abstract void HandleAction(UIActionType action);
}

/// <summary>
/// bộ trạng thái Account
/// </summary>
public abstract class StateAccount : IUIInteractiveState
{
    protected readonly StateMachine _stateMachine;
    protected CoreEvent _coreEvent;

    public AccountStateType StateType { get; protected set; } // state enum

    protected StateAccount(StateMachine stateMachine, CoreEvent coreEvent, AccountStateType stateType)
    {
        _stateMachine = stateMachine;
        _coreEvent = coreEvent;
        StateType = stateType;
    }

    public void OnEnter()
    {
        _coreEvent.TriggerAccountChangeState(StateType);
    }

    public void OnExit()
    {
       _coreEvent.TriggerAccountChangeState(StateType);
    }

    public abstract void HandleAction(UIActionType action);
}

// ----------------------------------------------------------------------------


public interface ICharacterState : IState
{
    void HandleAction(CharacterActionType type);
}

public class CharStateMachine
{
    /// <summary>
    /// Trạng thái hiện tại của StateMachine.
    /// </summary>
    private ICharacterState currentState;

    /// <summary>
    /// Khởi tạo StateMachine với trạng thái ban đầu.
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(ICharacterState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public void HandleAction(CharacterActionType action)
    {
        currentState?.HandleAction(action);
    }

    /// <summary>
    /// Lấy trạng thái hiện tại.
    /// </summary>
    public Type CurrentStateType => currentState?.GetType();
}

/// <summary>
/// bộ trạng thái của Player.
/// </summary>
public abstract class StatePlayer : ICharacterState
{
    public readonly CharStateMachine _stateMachine;
    public PlayerEvent _playerEvent;

    public CharacterStateType StateType { get; protected set; }

    public StatePlayer(CharStateMachine stateMachine, PlayerEvent playerEvent, CharacterStateType stateType)
    {
        _stateMachine = stateMachine;
        _playerEvent = playerEvent;
        StateType = stateType;
    }
    public virtual void OnEnter()
    {
        _playerEvent.TriggerChangePlayerState(StateType);
    }
    public virtual void OnExit()
    {
        _playerEvent.TriggerChangePlayerState(StateType);
    }
    public abstract void HandleAction(CharacterActionType type);
}

//public abstract class StateEnemy : ICharacterState
//{
//    protected readonly StateMachine _stateMachine;
//    protected PlayerEvent _playerEvent;
//    public StateEnemy(StateMachine stateMachine, PlayerEvent playerEvent)
//    {
//        _stateMachine = stateMachine;
//        _playerEvent = playerEvent;
//    }
//    public virtual void OnEnter()
//    {
//        _playerEvent.TriggerChangePlayerState(StateType);
//    }
//    public virtual void OnExit()
//    {
//        _playerEvent.TriggerChangePlayerState(StateType);
//    }

//    public void HandleAction(CharacterActionType type)
//    {
//        throw new NotImplementedException();
//    }

//}







