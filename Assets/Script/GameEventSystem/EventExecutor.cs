using System.Collections.Generic;
using UnityEngine;
using Duckle;
using System;

/// <summary>
/// Quản lý và thực thi các sự kiện trong game.
/// dùng để kích hoạt sự kiện dựa trên ID.
/// </summary>
public class EventExecutor : MonoBehaviour
{
    [SerializeField] private EventDatabase database;

    private Dictionary<EventType_Dl, IEventAction> handlers;

    private void Awake()
    {
        handlers = new Dictionary<EventType_Dl, IEventAction>
        {
            { EventType_Dl.Cutscene, new CutsceneAction() },
            { EventType_Dl.ChangeMap, new ChangeMapAction() },
            { EventType_Dl.Dialogue, new DialogueAction() },
            { EventType_Dl.Trap, new TrapAction() }
        };
    }

    /// <summary>
    /// Gọi sự kiện dựa trên ID.
    /// </summary>
    /// <param name="id"></param>
    public void TriggerEvent(string id)
    {
        BaseEventData data = database.GetEventById(id); // Lấy sự kiện từ database
        if (data == null)
        {
            Debug.LogWarning($"[EventExecutor] Event '{id}' not found.");
            return;
        }

        if (Enum.TryParse(data.type.ToString(), out EventType_Dl convertedType))
        {
            if (handlers.TryGetValue(convertedType, out var action)) // Lấy handler tương ứng với loại sự kiện
            {
                action.Execute(data); // Thực thi hành động
            }
            else
            {
                Debug.LogWarning($"[EventExecutor] No handler for type {convertedType}");
            }
        }
        else
        {
            Debug.LogWarning($"[EventExecutor] Unable to convert EventType '{data.type}' to EventType_Dl.");
        }
    }
}

/// <summary>
/// Định nghĩa các hành động cho từng loại sự kiện Cutscene.
/// </summary>
public class CutsceneAction : IEventAction
{
    public void Execute(BaseEventData data)
    {
        //Debug.Log($"[Cutscene] Playing: {data.eventId}");
        EventManager.Instance.PlayEvent(data.eventId);
    }
}

/// <summary>
/// Định nghĩa hành động cho sự kiện thay đổi bản đồ.
/// </summary>
public class ChangeMapAction : IEventAction
{
    public void Execute(BaseEventData data)
    {
        Debug.Log($"[MapChange] Loading scene for event: {data.eventId}");
        EventManager.Instance.PlayEvent(data.eventId);
    }
}

public class DialogueAction : IEventAction
{
    public void Execute(BaseEventData data)
    {
        EventManager.Instance.PlayEvent(data.eventId);
    }
}

public class TrapAction : IEventAction
{
    public void Execute(BaseEventData data)
    {
        EventManager.Instance.PlayEvent(data.eventId);
    }
}
