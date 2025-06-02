using System;
using System.Collections.Generic;
using Duckle;
using UnityEngine;

namespace Script.GameEventSystem
{
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

    public class CutsceneAction : IEventAction
    {
        public void Execute(BaseEventData data)
        {
            //Debug.Log($"[Cutscene] Playing: {data.eventId}");
            EventManager.Instance.PlayEvent(data.eventId);
        }
    }

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
            // Completed 
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
}