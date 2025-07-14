using System;
using UnityEngine;

/// <summary>
/// Chứa các sự kiện.
/// </summary>
public class CoreEvent : MonoBehaviour
{
    public static CoreEvent Instance { get; private set; }

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

    #region sự kiện A1

    public event Action OnNewSession;
    public event Action OnContinueSession;
    public event Action OnQuitGame;

    public event Action OnSavePanel;
    public event Action OnUserPanel;

    #endregion

    #region sự kiện A2

    public event Action OnBack;

    public event Action OnPausedSession;
    public event Action OnResumedSession;
    public event Action OnSaveSession;
    public event Action OnQuitSession;

    #endregion

    #region sự kiện A3

    public event Action OnSelectSaveItem;
    public event Action TurnOnMenu;
    public event Action TurnOffMenu;

    #endregion

    #region sự kiện A4

    public event Action<CoreStateType> OnChangeState;
    public event Action OnKeyDown;

    #endregion

    //--------------------------------------------------------------------------------------------

    #region trigger sự kiện A1

    public void triggerNewSession() => OnNewSession?.Invoke();
    public void triggerContinueSession() => OnContinueSession?.Invoke();
    public void triggerQuitGame() => OnQuitGame?.Invoke();

    public void triggerSavePanel() => OnSavePanel?.Invoke();
    public void triggerUserPanel() => OnUserPanel?.Invoke();

    #endregion

    #region trigger sự kiện A2

    public void triggerBack() => OnBack?.Invoke();

    public void triggerPausedSession() => OnPausedSession?.Invoke();
    public void triggerResumedSession() => OnResumedSession?.Invoke();
    public void triggerSaveSession() => OnSaveSession?.Invoke();
    public void triggerQuitSession() => OnQuitSession?.Invoke();

    #endregion

    #region trigger sự kiện A3

    public void triggerSelectSaveItem() => OnSelectSaveItem?.Invoke();
    public void triggerTurnOnMenu() => TurnOnMenu?.Invoke();
    public void triggerTurnOffMenu() => TurnOffMenu?.Invoke();

    #endregion

    #region trigger sự kiện A4

    public void TriggerChangeState(CoreStateType stateType) => OnChangeState?.Invoke(stateType);
    public void triggerKeyDown() => OnKeyDown?.Invoke();

    #endregion
}

/// <summary>
/// Giao diện bắt buộc khi triển khai CoreEvent.
/// </summary>
public interface ICoreEvent
{
    void RegisterEvent(CoreEvent e);
    void UnregisterEvent(CoreEvent e);
}

/// <summary>
/// Khi một đối tượng kế thừa lớp này được khởi tạo (Awake), nó sẽ đảm bảo chỉ có một instance duy nhất của CoreEvent tồn tại trong scene (theo mẫu Singleton).
/// Giúp đối tượng kế thừa nó không cần lặp lại code khởi tạo hay quản lý singleton. 
/// Khi kế thừa, bạn chỉ cần cài đặt chi tiết hai hàm RegisterEvent và UnregisterEvent để xử lý sự kiện mong muốn.
/// </summary>
public abstract class CoreEventListenerBase : MonoBehaviour, ICoreEvent
{
    protected CoreEvent _coreEvent;

    protected virtual void Awake()
    {
        EnsureCoreEventInstance();
        RegisterEvent(_coreEvent);
    }
    protected virtual void OnDestroy()
    {
        UnregisterEvent(_coreEvent);
    }

    /// <summary>
    /// Đảm bảo rằng CoreEvent đã được khởi tạo và có một instance duy nhất trong scene.
    /// </summary>
    private void EnsureCoreEventInstance()
    {
        if (CoreEvent.Instance == null)
        {
            new GameObject("CoreEvent").AddComponent<CoreEvent>();
        }
        _coreEvent = CoreEvent.Instance;
    }

    public abstract void RegisterEvent(CoreEvent e);
    public abstract void UnregisterEvent(CoreEvent e);
}


// ----------------------------------------------------LUỒNG DỮ LIỆU:--------------------------------------------------

// -

// ----------------------------------------------------TỐI ƯU:--------------------------------------------------

// - Dùng UnityEvent nếu muốn sự kiện xuất hiện trong Inspector để kết nối không cần code.
// - Dùng enum +Dictionary < EventType, Action > để quản lý số lượng lớn sự kiện một cách động.

// ----------------------------------------------------NOTE:--------------------------------------------------

// - 