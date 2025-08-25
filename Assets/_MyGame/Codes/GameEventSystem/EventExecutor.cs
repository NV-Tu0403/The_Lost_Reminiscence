using System.Collections.Generic;
using _MyGame.Codes.Dialogue;
using _MyGame.Codes.Timeline;
using Code.Checkpoint;
using Code.Cutscene;
using Code.Puzzle;
using UnityEngine;

namespace _MyGame.Codes.GameEventSystem
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
                { EventType_Dl.Puzzle, new PuzzleAction() },
                { EventType_Dl.Checkpoint, new CheckpointAction() },
                { EventType_Dl.Timeline, new TimelineAction() },
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
            var data = database.GetEventById(eventId);
            if (data == null)
            {
                Debug.LogWarning($"[EventExecutor] Không tìm thấy eventId = '{eventId}' trong EventDatabase!");
                return;
            }

            // 2. Lấy handler dựa trên data.type (vd: Dialogue, Cutscene,…)
            if (handlers.TryGetValue(data.type, out IEventAction action))
            {
                // Gán callback cho Checkpoint để phát eventId khi hoàn thành
                if (data.type == EventType_Dl.Checkpoint)
                {
                    data.OnFinish = () => EventBus.Publish(eventId, data);
                }
                action.Execute(data);
            }
            else
            {
                Debug.LogWarning($"[EventExecutor] Không có handler cho EventType = {data.type}");
            }
        }
    }
}
