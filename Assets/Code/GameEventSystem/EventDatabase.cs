using System.Collections.Generic;
using System.Linq;
using Duckle;
using UnityEngine;

namespace Code.GameEventSystem
{
    [CreateAssetMenu(fileName = "EventDatabase", menuName = "Events/EventDatabase")]
    public class EventDatabase : ScriptableObject
    {
        public List<BaseEventData> events;
    
        public BaseEventData GetEventById(string id)
        {
            return events.Find(e => e.eventId == id);
        }

        public string[] GetAllEvenID()
        {
            return events.Select(e => e.eventId).ToArray();
        }

        public List<string> GetAllPuzzleEventIds()
        {
            return events.Where(e => e.type == EventType_Dl.Puzzle).Select(e => e.eventId).ToList();
        }
    }
    
    [System.Serializable]
    public class BaseEventData
    {
        public string eventId;
        public EventType_Dl type;
        [TextArea] public string description;
        [System.NonSerialized] public System.Action onFinish;
    }
}