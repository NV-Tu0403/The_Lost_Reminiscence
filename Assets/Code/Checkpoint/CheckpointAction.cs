using System;
using Code.GameEventSystem;
using UnityEngine;

namespace Code.Checkpoint
{
    public class CheckpointAction : IEventAction
    {
        public void Execute(BaseEventData data)
        { 
            // Phát sự kiện "Checkpoint" để PlayerRespawnManager lắng nghe và lưu checkpoint
            EventBus.Publish("Checkpoint", data);
            Debug.Log("[CheckpointAction] Checkpoint triggered!");

            // Gọi callback để phát event progression tiếp theo (nếu có)
            data.onFinish?.Invoke();
        }
    }
}