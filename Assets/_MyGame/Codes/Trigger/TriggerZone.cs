using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Procession;
using _MyGame.Codes.SimpleGuidance;
using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    public abstract class TriggerZone : MonoBehaviour
    {
        [SerializeField] public string eventId;
        
        protected abstract bool IsValidTrigger(Collider other);
        private bool HandleProgression()
        {
            // Kiểm tra điều kiện progression
            if (!ProgressionManager.Instance.CanTrigger(eventId) &&
                !ProgressionManager.Instance.IsWaitingForEvent(eventId))
            {
                Debug.Log($"[{GetType().Name}] Chưa đủ điều kiện để bắt đầu event '{eventId}'.");
                return false;
            }
            
            // Unlock → Trigger → Disable zone
            ProgressionManager.Instance.UnlockProcess(eventId);
            EventExecutor.Instance.TriggerEvent(eventId);
            DisableZone();
            
            return true;
        }

        protected abstract void OnTriggered(Collider other);
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!IsValidTrigger(other)) return;
            
            // Logic guidance được xử lý TẠI ĐÂY - tất cả script con sẽ có guidance tự động
            if (GuidanceManager.Instance != null && !string.IsNullOrEmpty(eventId))
            {
                GuidanceManager.Instance.OnPlayerInteraction(eventId);
            }
            
            // Gọi logic riêng của từng script con
            OnTriggered(other);
        }
        
        protected void DisableZone() => gameObject.SetActive(false);

        protected void HandleDefaultTrigger() {HandleProgression();} 
        
        protected void HandleCustomTrigger(System.Action beforeProgression = null, System.Action afterProgression = null)
        {
            beforeProgression?.Invoke();
            
            if (HandleProgression())
            {
                afterProgression?.Invoke();
            }
        }
    }
}