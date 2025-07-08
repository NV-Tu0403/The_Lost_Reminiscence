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
public abstract class StateBase : IState
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
        Debug.Log($"[UIState] Entered: {StateType}");
        _coreEvent.TriggerChangeState(StateType);
    }

    public virtual void OnExit()
    {
        Debug.Log($"[UIState] Exited: {StateType}");
        _coreEvent.TriggerChangeState(StateType);
    }

    // Bắt buộc lớp con triển khai
    public abstract void HandleAction(UIActionType action);
}












