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

            if (ProgressionManager.Instance.CanTrigger(eventId))
            {
                ProgressionManager.Instance.UnlockProcess(eventId);
                EventExecutor.Instance.TriggerEvent(eventId);    
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[EventTriggerZone] Không thể kích hoạt sự kiện '{eventId}' vì điều kiện chưa được đáp ứng.");
            }
        }
    }
}
