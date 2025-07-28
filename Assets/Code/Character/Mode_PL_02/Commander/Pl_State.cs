
//    public class Pl_State : MonoBehaviour
//{
//}

public class IdleState_1 : StatePlayer
{
    public IdleState_1(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Idle)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Walk:
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạys
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class WalkState : StatePlayer
{
    public WalkState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Walk)
    {
    }

    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class RunState : StatePlayer
{
    public RunState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Run)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                // _stateMachine.SetState(new IdleState(_stateMachine, _coreEvent));
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class DashState : StatePlayer
{
    public DashState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Dash)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class SprintState : StatePlayer
{
    public SprintState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Sprint)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                // _stateMachine.SetState(new IdleState(_stateMachine, _coreEvent));
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class CrouchState : StatePlayer
{
    public CrouchState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Crouch)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class JumpState : StatePlayer
{
    public JumpState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Jump)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class AttackState : StatePlayer
{
    public AttackState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Attack)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                // _stateMachine.SetState(new IdleState(_stateMachine, _coreEvent));
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class DefendState : StatePlayer
{
    public DefendState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Defend)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                // _stateMachine.SetState(new IdleState(_stateMachine, _coreEvent));
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class DieState : StatePlayer
{
    public DieState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Die)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}

public class RespawnState : StatePlayer
{
    public RespawnState(CharStateMachine stateMachine, PlayerEvent playerEvent) : base(stateMachine, playerEvent, CharacterStateType.Respawn)
    {
    }
    /// <summary>
    /// Xử lý hành động từ giao diện người dùng.
    /// </summary>
    /// <param name="action"></param>
    public override void HandleAction(CharacterActionType type)
    {
        switch (type)
        {
            case CharacterActionType.Idle:
                // _stateMachine.SetState(new IdleState(_stateMachine, _coreEvent));
                _stateMachine.SetState(new IdleState_1(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Walk:
                // Thực hiện hành động đi bộ
                _stateMachine.SetState(new WalkState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Run:
                // Thực hiện hành động chạy
                _stateMachine.SetState(new RunState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Jump:
                // Thực hiện hành động nhảy
                _stateMachine.SetState(new JumpState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Dash:
                // Thực hiện hành động lướt
                _stateMachine.SetState(new DashState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Sprint:
                // Thực hiện hành động chạy nhanh
                _stateMachine.SetState(new SprintState(_stateMachine, _playerEvent));
                break;
            case CharacterActionType.Crouch:
                // Thực hiện hành động cúi người
                _stateMachine.SetState(new CrouchState(_stateMachine, _playerEvent));
                break;
        }
    }
}


