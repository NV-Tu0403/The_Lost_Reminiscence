using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _MyGame.Codes.GameEventSystem
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
        [NonSerialized] public Action OnFinish;
    }
}