using System;
using UnityEngine;

public struct CoreInputType
{
    public KeyCoreInputType InputType;
}

public class CoreInput : CoreEventListenerBase
{
    [SerializeField] private InputActionMapping inputActionMapping; // Tham chiếu đến ScriptableObject
    private StateMachine _stateMachine;

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new StateMachine();
        _stateMachine.SetState(new InMainMenuState(_stateMachine)); // Khởi tạo trạng thái ban đầu
    }

    private void Update()
    {
        Mapping();
    }

    public override void RegisterEvent(CoreEvent e)
    {
        e.OnKeyDown += Mapping;
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.OnKeyDown -= Mapping;
    }

    private void Mapping()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // Chuyển từ KeyCode sang KeyCoreInputType nếu trùng
                if (Enum.TryParse(keyCode.ToString(), out KeyCoreInputType mappedKey))
                {
                    GetInput(new CoreInputType { InputType = mappedKey });
                    break;
                }
            }
        }
    }

    /// <summary>
    /// nhận vào một CoreInputType và tìm kiếm các hành động tương ứng trong InputActionMapping(Config).
    /// </summary>
    /// <param name="coreInputType"></param>
    private void GetInput(CoreInputType coreInputType)
    {
        if (inputActionMapping != null && inputActionMapping.TryGetActions(coreInputType.InputType, out var actions))
        {
            foreach (var action in actions)
            {
                Debug.Log($"[CoreInput] Triggered UIAction: {action} for Key: {coreInputType.InputType}");

                ExecuteAction(action);
            }
        }
        else
        {
            Debug.LogWarning($"[CoreInput] No actions found for input type: {coreInputType.InputType}");
        }
    }

    private void ExecuteAction(UIActionType action)
    {
        _stateMachine.HandleAction(action);
    }

}
