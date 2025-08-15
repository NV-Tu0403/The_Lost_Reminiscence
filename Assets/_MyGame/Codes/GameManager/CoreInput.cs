using System;
using UnityEngine;

public struct CoreInputType
{
    public KeyCoreInputType InputType;
}

public class CoreInput : CoreEventListenerBase
{
    [SerializeField] private InputActionMapping inputActionMapping; // Tham chiếu đến ScriptableObject

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // Đăng ký sự kiện để tìm player và đặt vị trí
        FindPlayer();
       
    }

    private void Update()
    {
        Mapping();
        SetPlayerToZeroPosition();
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
        //if (Input.GetMouseButtonDown(0)) // Chuột trái
        //{
        //    GetInput(new CoreInputType { InputType = KeyCoreInputType.MouseLeft });
        //    return;
        //}
        //if (Input.GetMouseButtonDown(1)) // Chuột phải
        //{
        //    GetInput(new CoreInputType { InputType = KeyCoreInputType.MouseRight });
        //    return;
        //}

        try
        {
            // quét KeyCode
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
        catch (Exception)
        {

            throw;
        }

    }

    private void GetInput(CoreInputType input)
    {
        try
        {
            var currentState = (Core.Instance?.CurrentCoreState) ?? "";
            if (!Enum.TryParse(currentState, out CoreStateType coreStateType))
            {
                Debug.LogError($"[CoreInput] Invalid CoreStateType: {currentState}");
                return;
            }

            if (inputActionMapping.TryGetActions(input.InputType, coreStateType, out var actions))
            {
                foreach (var action in actions)
                {
                    ExecuteAction(action);
                    //Debug.Log($"[CoreInput] Action: {action} for Input: {input.InputType} in State: {coreStateType}");
                }
            }
            else
            {
                //Debug.LogWarning($"[CoreInput] No actions found for Input: {input.InputType} in State: {coreStateType}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CoreInput] Error processing input {input.InputType}: {e.Message}");
        }
    }

    private void ExecuteAction(UIActionType action)
    {
        if (Core.Instance == null)
        {
            Debug.LogWarning("Core.Instance is null. Cannot forward action.");
            return;
        }

        Core.Instance?.HandleAction(action);
    }

    private Transform player;
    // hàm tìm player theo tag Player
    public void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("[CoreInput] Player not found with tag 'Player'.");
        }
        else
        {
            //Debug.Log("[CoreInput] Player found.");
        }
    }

    // hàm khi nhấn nút v thì đặt player vào tọa độ 0,0,0
    public void SetPlayerToZeroPosition()
    {
        if (player == null)
        {
            FindPlayer(); // Tìm player nếu chưa có
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (player != null)
            {
                player.transform.position = Vector3.zero;
                Debug.Log("[CoreInput] Player position set to (0, 0, 0)");
            }
            else
            {
                Debug.LogWarning("[CoreInput] Player is null. Cannot set position.");
            }
        }
    }
}
