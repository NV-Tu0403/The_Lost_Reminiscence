
/// <summary>
/// khi ở Guest
/// </summary>
public class NoConnectToServer : StateAccount
{
    public NoConnectToServer(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, AccountStateType.NoConnectToServer)
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
/// đang không ở Guest
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

