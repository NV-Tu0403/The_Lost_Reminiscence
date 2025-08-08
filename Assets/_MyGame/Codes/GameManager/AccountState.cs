
/// <summary>
/// lúc không tự đăng nhập được hoặc không có tài khoản hiện tại
/// </summary>
public class NoCurrentAccountState : StateAccount
{
    public NoCurrentAccountState(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, AccountStateType.NoCurrentAccount)
    {
    }

    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.Login:
                //_coreEvent.triggerLogin();
                //_stateMachine.SetState(new LoggedInState(_stateMachine, _coreEvent));
                break;
            case UIActionType.Register:
                //_coreEvent.triggerRegister();
                break;
                //default:
                //    throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}

/// <summary>
/// Trạng thái khi đã có Current Account (đang chơi) và không kết nối được đến máy chủ.
/// </summary>
public class NoConnectToServerState : StateAccount
{
    public NoConnectToServerState(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, AccountStateType.NoConnectToServer)
    {
    }
    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.ConnectToServer:
                break;
            case UIActionType.Logout:
                break;
                //default:
                //    throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}

/// <summary>
/// Trạng thái khi đang kết nối đến máy chủ.
/// </summary>
public class ConectingServer : StateAccount
{
    public ConectingServer(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, AccountStateType.ConectingServer)
    {
    }
    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.ConnectingToServer:
                //_stateMachine.SetState(new NoConnectToServerState(_stateMachine, _coreEvent));
                break;
                //default:
                //    throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}

/// <summary>
/// Trạng thái khi đã kết nối đến máy chủ. (đã có Current Account)
/// </summary>
public class HaveConnectToServer : StateAccount
{
    public HaveConnectToServer(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, AccountStateType.HaveConnectToServer)
    {
    }

    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.Logout:
                //_coreEvent.triggerLogout();
                break;
            case UIActionType.SyncFileSave:
                //_coreEvent.triggerSyncFileSave();
                break;
                //default:
                //    throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}

