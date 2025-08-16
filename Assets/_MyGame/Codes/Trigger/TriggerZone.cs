using UnityEngine;
using _MyGame.Codes.Guidance;

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
            
            // Thông báo guidance system về tương tác
            if (GuidanceManager.Instance != null && !string.IsNullOrEmpty(eventId))
            {
                GuidanceManager.Instance.OnPlayerInteraction(eventId);
            }
            
            OnTriggered(other);
        }

        protected abstract void OnTriggered(Collider other);
    }
}