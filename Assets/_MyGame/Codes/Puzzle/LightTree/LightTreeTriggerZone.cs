using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using Code.Trigger;
using UnityEngine;

namespace Code.Puzzle.LightTree
{
    public class LightTreeTriggerZone : TriggerZone
    {
        [SerializeField] private UISpirit uiSpirit;
        
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Player");
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
            if (uiSpirit != null) uiSpirit.Hide();
        }

        protected override void OnTriggered(Collider other)
        {
            // Kiểm tra điều kiện progression
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[PlayerTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }

            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
            
            if (uiSpirit != null) uiSpirit.Show();
        }
    }
}