using System;
using UnityEngine;

/// <summary>
/// Giao diện bắt buộc Khi triển khai State.
/// </summary>
public interface IState
{
    void OnEnter();
    void OnExit();

    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.    
    /// </summary>
    /// <param name="action"></param>
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
    private IState currentState;

    /// <summary>
    /// Khởi tạo StateMachine với trạng thái ban đầu.
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(IState newState)
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
/// Lớp Cơ sở cho các trạng thái.
/// Kế thừa lớp này để tạo các trạng thái cụ thể.
/// 
/// logic cụ thể của từng State được triển khai trong HandleAction.
/// </summary>
public abstract class StateBase : CoreEventListenerBase , IState
{
    protected readonly StateMachine _stateMachine;

    protected StateBase(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public virtual void OnEnter()
    {
        Debug.Log($"[UIState] Entered: {GetType().Name}");
    }

    public virtual void OnExit()
    {
        Debug.Log($"[UIState] Exited: {GetType().Name}");
    }

    // Bắt buộc lớp con triển khai
    public abstract void HandleAction(UIActionType action);
}

// ----------------------------------------------Các trạng thái cụ thể (cần tách ra)------------------------------------------------------

/// <summary>
/// Trạng thái khi đang ở trong menu chính.
/// </summary>
public class InMainMenuState : StateBase
{
    public InMainMenuState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.Back:
            case UIActionType.QuitSesion:
                _coreEvent.triggerBack();
                _stateMachine.SetState(new InSessionState(_stateMachine));
                break;

            case UIActionType.QuitGame:
                _coreEvent.triggerQuitGame();
                break;
        }
    }

    public override void RegisterEvent(CoreEvent e)
    {
        throw new NotImplementedException();
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// trạng thái khi đang trong phiên chơi.
/// </summary>
public class InSessionState : StateBase
{
    public InSessionState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void HandleAction(UIActionType action)
    {
        if (action == UIActionType.OpenMenu)
        {
            _coreEvent.triggerTurnOnMenu();
            _stateMachine.SetState(new InMainMenuState(_stateMachine));
        }
    }

    public override void RegisterEvent(CoreEvent e)
    {
        throw new NotImplementedException();
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        throw new NotImplementedException();
    }
}


