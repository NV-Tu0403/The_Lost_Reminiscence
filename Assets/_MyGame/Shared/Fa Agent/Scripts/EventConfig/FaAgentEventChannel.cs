using UnityEngine;
using UnityEngine.Events;

namespace Tu_Develop.Import.Scripts.EventConfig
{
    [CreateAssetMenu(menuName = "Events/FaAgent Event Channel")]
    public class FaAgentEventChannel : ScriptableObject
    {
        public UnityAction<FaAgent> OnFaAgentReady;

        public FaAgentEventChannel(UnityAction<FaAgent> onFaAgentReady)
        {
            OnFaAgentReady = onFaAgentReady;
        }

        public void RaiseEvent(FaAgent faAgent)
        {
            OnFaAgentReady?.Invoke(faAgent);
        }
    }
}