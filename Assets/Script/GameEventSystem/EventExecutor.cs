using System;
using System.Collections.Generic;
using Duckle;
using Loc_Backend.Scripts;
using UnityEngine;

namespace Script.GameEventSystem
{
    public class EventExecutor : MonoBehaviour
    {
        public static EventExecutor Instance { get; private set; } 
        
        [SerializeField] private EventDatabase database;

        private Dictionary<EventType_Dl, IEventAction> handlers;

        private void Awake()
        {
            handlers = new Dictionary<EventType_Dl, IEventAction>
            {
                { EventType_Dl.Dialogue, new DialogueAction() },
            };
        }

        /// <summary>
        /// Lấy BaseEventData từ EventDatabase qua ID.
        /// </summary>
        public BaseEventData GetEventDataById(string id)
        {
            return database.GetEventById(id);
        }

        /// <summary>
        /// TriggerEvent được gọi ở TriggerZone
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            // 1. Tìm dữ liệu trong EventDatabase
            BaseEventData data = database.GetEventById(eventId);
            if (data == null)
            {
                Debug.LogWarning($"[EventExecutor] Không tìm thấy eventId = '{eventId}' trong EventDatabase!");
                return;
            }

            // 2. Lấy handler dựa trên data.type (vd: Dialogue, Cutscene,…)
            if (handlers.TryGetValue(data.type, out IEventAction action))
            {
                action.Execute(data);
            }
            else
            {
                Debug.LogWarning($"[EventExecutor] Không có handler cho EventType = {data.type}");
            }
        }
    }
    
    public class DialogueAction : IEventAction
    {
        private string _eventIdCurrent;
        
        // Phương thức này sẽ được gọi khi bắt đầu một hội thoại.
        public void Execute(BaseEventData data)
        {
            Debug.Log($"[DialogueAction] Starting dialogue for event: {data.eventId}");
            
            _eventIdCurrent = data.eventId;
            DialogueManager.Instance.StartDialogue(data.eventId, ()=> Finished(data.eventId));
        }

        
        // Phương thức này sẽ được gọi khi hội thoại kết thúc.
        public void Finished(string eventId)
        {
            Debug.Log($"[DialogueAction] Finished dialogue: {eventId}");
            
            // Khi UI Dialogue kết thúc, gọi ngược về EventManager
            EventManager.Instance.OnEventFinished(eventId);
        }
    }
}

