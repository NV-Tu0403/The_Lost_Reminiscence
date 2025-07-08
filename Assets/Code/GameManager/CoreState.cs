
/// <summary>
/// Trạng thái khi đang ở trong menu chính.
/// 
/// vd: khi curentstate đang là InMainMenuState và nhấn NewSession => thông báo event triggerNewSession và chuyển sang InSessionState.
/// </summary>
public class InMainMenuState : StateBase
{
    public InMainMenuState(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, CoreStateType.InMainMenu)
    {
    }

    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.QuitGame:
                break;
            case UIActionType.Back:
                break;

            case UIActionType.NewSession:
                _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
                break;
            case UIActionType.ContinueSession:
                _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
                break;
            case UIActionType.TutorialSession:
                _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
                break;

            case UIActionType.SavePanel:
                break;
            case UIActionType.UserPanel:
                break;
        }
    }
}

/// <summary>
/// trạng thái khi đang trong phiên chơi.
/// </summary>
public class InSessionState : StateBase
{
    public InSessionState(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, CoreStateType.InSession)
    {
    }

    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.OpenMenu:
            case UIActionType.PauseSession:
            case UIActionType.QuitSesion:
                _stateMachine.SetState(new PauseSessionState(_stateMachine, _coreEvent));
                break;


        }
    }
}

/// <summary>
/// Trạng thái khi phiên chơi đang tạm dừng.
/// </summary>
public class PauseSessionState : StateBase
{
    public PauseSessionState(StateMachine stateMachine, CoreEvent coreEvent) : base(stateMachine, coreEvent, CoreStateType.PauseSession)
    {
    }
    public override void HandleAction(UIActionType action)
    {
        switch (action)
        {
            case UIActionType.SaveSesion:
                _coreEvent.triggerSaveSession();
                break;
            case UIActionType.QuitSesion:
                _coreEvent.triggerQuitSession();
                _stateMachine.SetState(new InMainMenuState(_stateMachine, _coreEvent));
                break;
            case UIActionType.ResumeSession:
                _coreEvent.triggerResumedSession(); // dừng pause
                _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
                break;
        }
    }
}