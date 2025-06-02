using System.Collections.Generic;
using System.Linq;
using Duckle;
using UnityEngine;

namespace Script.GameEventSystem
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
    }
    
    [System.Serializable]
    public class BaseEventData
    {
        public string eventId;
        public EventType_Dl type;
        [TextArea] public string description;
    }

    public interface IEventAction
    {
        void Execute(BaseEventData data);
    }
}