using Duckle;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// lưu trữ danh sách sự kiện.
/// </summary>
[CreateAssetMenu(fileName = "EventDatabase", menuName = "Events/EventDatabase")]
public class EventDatabase : ScriptableObject
{
    public List<BaseEventData> events;

    /// <summary>
    /// Lấy danh sách tất cả các sự kiện trong database.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BaseEventData GetEventById(string id)
    {
        return events.Find(e => e.eventId == id);
    }

    /// <summary>
    /// Lấy tên của tất cả các sự kiện.
    /// </summary>
    public string[] GetAllEvenID()
    {
        return events.Select(e => e.eventId).ToArray();
    }
}

/// <summary>
/// định nghĩa các sự kiện trong game
/// </summary>
[System.Serializable]
public class BaseEventData
{
    public string eventId;
    public EventType_Dl type;
    [TextArea] public string description;
}

/// <summary>
/// Định nghĩa các hành động cho từng loại sự kiện.
/// </summary>
public interface IEventAction
{
    void Execute(BaseEventData data);
}


