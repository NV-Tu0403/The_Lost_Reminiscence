using Tu_Develop.Import.Scripts;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/FaAgent Event Channel")]
public class FaAgentEventChannel : ScriptableObject
{
    public UnityAction<FaAgent> OnFaAgentReady;

    public void RaiseEvent(FaAgent faAgent)
    {
        OnFaAgentReady?.Invoke(faAgent);
    }
}