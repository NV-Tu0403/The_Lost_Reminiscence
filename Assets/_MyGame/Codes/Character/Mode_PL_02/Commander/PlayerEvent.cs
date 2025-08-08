using System;
using System.Data.SqlClient;
using UnityEngine;

public class PlayerEvent : MonoBehaviour
{
    public static PlayerEvent Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of CoreEvent detected. Destroying duplicate instance.");
            Destroy(gameObject);
        }
    }

    #region event A1
    public event Action<CharacterStateType> OnChangePlayerState;

    #endregion

    #region event A2
    public event Action onPlayerDie;
    public event Action OnOlayerRespawn;
    #endregion

    #region event A3
    public event Action<string> OnPlayerIdle;
    public event Action<string> OnPlayerWalk;
    public event Action<string> OnPlayerRun;
    public event Action<string> OnPlayerDash;
    public event Action<string> OnPlayerSprint;
    public event Action<string> OnPlayerCrouch;
    public event Action<string> OnPlayerJump;
    #endregion

    #region event A4
    public event Action OnPlayerAttack;
    public event Action OnPlayerDefend;

    public event Action<GameObject, float, GameObject> OnTakeOutDamage;
    #endregion

    // -----------------------------------------------------------------------------------------------------------

    #region trigger A1
    public void TriggerChangePlayerState(CharacterStateType stateType) => OnChangePlayerState?.Invoke(stateType);

    #endregion

    #region trigger A2
    public void TriggerPlayerDie() => onPlayerDie?.Invoke();
    public void TriggerPlayerRespawn() => OnOlayerRespawn?.Invoke();
    #endregion

    #region trigger A3
    public void TriggerPlayerIdle(string message) => OnPlayerIdle?.Invoke(message);
    public void TriggerPlayerWalk(string message) => OnPlayerWalk?.Invoke(message);
    public void TriggerPlayerRun(string message) => OnPlayerRun?.Invoke(message);
    public void TriggerPlayerDash(string message) => OnPlayerDash?.Invoke(message);
    public void TriggerPlayerSprint(string message) => OnPlayerSprint?.Invoke(message);
    public void TriggerPlayerCrouch(string message) => OnPlayerCrouch?.Invoke(message);
    public void TriggerPlayerJump(string message) => OnPlayerJump?.Invoke(message);
    #endregion

    #region trigger A4
    public void TriggerPlayerAttack() => OnPlayerAttack?.Invoke();
    public void TriggerPlayerDefend() => OnPlayerDefend?.Invoke();

    public void TriggerTakeOutDamage(GameObject attacker, float damage, GameObject target)
    {
        OnTakeOutDamage?.Invoke(attacker, damage, target);
    }

    #endregion

}

public interface IChararacterEvent
{
    void RegisterEvent(PlayerEvent e);
    void UnregisterEvent(PlayerEvent e);
}

public abstract class PlayerEventListenerBase : MonoBehaviour, IChararacterEvent
{
    protected PlayerEvent _playerEvent;

    protected virtual void Awake()
    {
        EnsureCoreEventInstance();
        RegisterEvent(_playerEvent);
    }
    protected virtual void OnDestroy()
    {
        UnregisterEvent(_playerEvent);
    }

    private void EnsureCoreEventInstance()
    {
        if (PlayerEvent.Instance == null)
        {
            new GameObject("PlayerEvent").AddComponent<PlayerEvent>();
        }
        _playerEvent = PlayerEvent.Instance;
    }

    public abstract void RegisterEvent(PlayerEvent e);
    public abstract void UnregisterEvent(PlayerEvent e);
}