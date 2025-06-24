using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.GameEventSystem
{
    public static class EventBus
    {
        private static Dictionary<string, Action<object>> eventTable = new();

        public static void Subscribe(string eventKey, Action<object> callback)
        {
            if (!eventTable.ContainsKey(eventKey))
                eventTable[eventKey] = delegate { };

            eventTable[eventKey] += callback;
        }

        public static void Unsubscribe(string eventKey, Action<object> callback)
        {
            if (eventTable.ContainsKey(eventKey))
                eventTable[eventKey] -= callback;
        }

        public static void Publish(string eventKey, object data = null)
        {
            if (eventTable.ContainsKey(eventKey))
            {
                eventTable[eventKey]?.Invoke(data);
            }
            else
            {
                Debug.LogWarning($"[EventBus] No listeners for event: {eventKey}");
            }
        }

        public static void ClearAll()
        {
            eventTable.Clear();
        }
    }
}