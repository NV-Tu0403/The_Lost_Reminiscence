using System.Collections.Generic;
using Duckle;
using Events.Cutscene.Scripts;
using Events.Dialogue.Scripts;
using Script.GameEventSystem.EventAction;
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
                { EventType_Dl.Cutscene, new CutsceneAction() }
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
}

