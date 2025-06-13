using UnityEngine;

namespace Events.TriggerZone
{
    public abstract class TriggerZone : MonoBehaviour
    {
        [SerializeField] public string eventId;
        protected virtual void DisableZone() => gameObject.SetActive(false);
        protected abstract bool IsValidTrigger(Collider other);

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidTrigger(other)) return;
            OnTriggered(other);
        }
        
        protected abstract void OnTriggered(Collider other);
    }
}