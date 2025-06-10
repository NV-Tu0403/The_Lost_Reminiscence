using Script.Procession;
using UnityEngine;

namespace Script.GameEventSystem
{
    public class EventTriggerZone : MonoBehaviour
    {
        [SerializeField] public string eventId;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            if (!ProgressionManager.Instance.CanTrigger(eventId) && 
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[EventTriggerZone] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return;
            }

            // Unlock → Trigger → Disable zone
            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
            gameObject.SetActive(false);
        }
    }
}
