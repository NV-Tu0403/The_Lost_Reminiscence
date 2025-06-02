using UnityEngine;

namespace Script.GameEventSystem
{
    public class EventTriggerZone : MonoBehaviour
    {
        [SerializeField] public string eventId;
        [SerializeField] public EventExecutor executor;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                executor.TriggerEvent(eventId);
            }
        }
    }
}
