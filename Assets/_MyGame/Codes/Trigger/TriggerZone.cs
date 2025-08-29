using _MyGame.Codes.Guidance;
using _MyGame.Codes.GameEventSystem; 
using _MyGame.Codes.Procession;      
using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    public abstract class TriggerZone : MonoBehaviour
    {
        [SerializeField] public string eventId;
        
        protected virtual void DisableZone() => gameObject.SetActive(false);
        protected abstract bool IsValidTrigger(Collider other);

        protected bool CanProgress()
        {
            if (string.IsNullOrEmpty(eventId)) return false;
            if (ProgressionManager.Instance != null && EventExecutor.Instance != null)
                return ProgressionManager.Instance.CanTrigger(eventId) ||
                       ProgressionManager.Instance.IsWaitingForEvent(eventId);
            Debug.LogWarning($"[{GetType().Name}] ProgressionManager hoặc EventExecutor chưa sẵn sàng.");
            return false;
        }
        
        protected void ExecuteProgression(bool unlockProcess, bool disableAfterTrigger)
        {
            if (unlockProcess)
            {
                ProgressionManager.Instance.UnlockProcess(eventId);
            }
            EventExecutor.Instance.TriggerEvent(eventId);
            if (disableAfterTrigger)
            {
                DisableZone();
            }
        }

        // New convenience wrapper
        protected bool TryExecuteProgression(bool unlockProcess, bool disableAfterTrigger)
        {
            if (!CanProgress()) return false;
            ExecuteProgression(unlockProcess, disableAfterTrigger);
            return true;
        }

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