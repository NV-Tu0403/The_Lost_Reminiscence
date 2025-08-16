using UnityEngine;

namespace Code.Trigger
{
    public abstract class TriggerZone : MonoBehaviour
    {
        [SerializeField] public string eventId;
        protected virtual void DisableZone() => gameObject.SetActive(false);
        protected abstract bool IsValidTrigger(Collider other);

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!IsValidTrigger(other)) return;
            OnTriggered(other);
        }

        protected abstract void OnTriggered(Collider other);
    }
}