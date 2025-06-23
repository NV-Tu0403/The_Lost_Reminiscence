using System.Collections.Generic;
using Code.Dialogue;
using Duckle;
using Events.Cutscene.Scripts;
using Script.Cutscene;
using Script.GameEventSystem.EventAction;
using Script.Puzzle;
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
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                return;
            }
            
            // Khoi tạo handlers
            handlers = new Dictionary<EventType_Dl, IEventAction>
            {
                { EventType_Dl.Dialogue, new DialogueAction() },
                { EventType_Dl.Cutscene, new CutsceneAction() },
                { EventType_Dl.Puzzle, new PuzzleAction() } // Thêm handler cho Puzzle
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
