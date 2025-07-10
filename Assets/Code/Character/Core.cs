using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

/// <summary>
/// Trung tâm điều phối toàn bộ các State của Core game.
/// Căn cứ vào CoreState để điều tiết các Event.
/// CoreState cần được cập nhật cẩn thận khi Event được kích hoạt (CoreState là bộ quy tắc để ràng buộc các Event
/// , Giúp mọi thứ tách bạch dể hiểu và dể theo dõi hơn)
/// </summary>
public class Core : CoreEventListenerBase
{
    public static Core Instance { get; private set; }
    private StateMachine _stateMachine;

    public bool IsOffline { get; private set; } = true;     // Mặc định là online khi khởi động                                               

    public string CurrentCoreState;
    public GameObject menuCamera;
    public GameObject MainMenu;


    protected override void Awake()
    {
     
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //else
        //{
        //    Destroy(gameObject);
        //}
        base.Awake();
        _stateMachine = new StateMachine();
        _stateMachine.SetState(new InMainMenuState(_stateMachine, _coreEvent));
        InitializeMenuCamera();
    }
    public override void RegisterEvent(CoreEvent e)
    {

        e.OnNewSession += NewSession;
        e.OnContinueSession += ContinueSession;

        e.OnPausedSession += PauseSession;
        e.OnResumedSession += ResumeSession;
        e.OnSaveSession += SaveSession;
        e.OnQuitSession += QuitSession;

        e.OnChangeState += UpdateCurrentCoreState;

        e.TurnOnMenu += TurnOnMenu;
        e.TurnOffMenu += TurnOffMenu;

        e.OnQuitGame += QuitGame;
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.OnNewSession -= NewSession;
        e.OnContinueSession -= ContinueSession;

        e.OnPausedSession -= PauseSession;
        e.OnResumedSession -= ResumeSession;
        e.OnSaveSession -= SaveSession;
        e.OnQuitSession -= QuitSession;

        e.OnChangeState -= UpdateCurrentCoreState;

        e.TurnOnMenu -= TurnOnMenu;
        e.TurnOffMenu -= TurnOffMenu;

        e.OnQuitGame -= QuitGame;
    }

    /// <summary>
    /// đảm bảo rằng MenuCamera đã được khởi tạo và kích hoạt.
    /// </summary>
    private void InitializeMenuCamera()
    {
        if (menuCamera == null)
        {
            menuCamera = GameObject.Find("MenuCamera");
            if (menuCamera == null)
            {
                menuCamera = Resources.Load<GameObject>("Prefab Loaded/MenuCamera");
                //Debug.Log("MenuCamera loaded from Resources.");
            }
            if (!menuCamera.activeSelf)
            {
                menuCamera.SetActive(true);
                //Debug.Log("MenuCamera initialized and activated successfully.");
            }
        }
    }

    private void Update()
    {
        // Kiểm tra trạng thái hiện tại của StateMachine và xử lý hành động từ người dùng
        if (_stateMachine.CurrentStateType != null)
        {
            Debug.Log($"[Core] Current State: {_stateMachine.CurrentStateType}");
        }
    }

    // debug state name
    private void UpdateCurrentCoreState(CoreStateType stateType)
    {
        CurrentCoreState = stateType.ToString();
        //Debug.Log($"[Core] CurrentCoreState updated to: {CurrentCoreState}");
    }

    public void HandleAction(UIActionType action)
    {
        _stateMachine.HandleAction(action);
    }

    private void NewSession()
    {
        _coreEvent.triggerTurnOffMenu();
    }

    private void ContinueSession()
    {
        // vì hiện tại Event OnContinue không được gọi bới Core Input nên SetState ở đây cho Continue
        _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
        _coreEvent.triggerTurnOffMenu();
    }

    private void PauseSession()
    {
        _coreEvent.triggerTurnOnMenu();// => (Event)TurnOnMenu -> turnOnMenu();
    }

    private void ResumeSession()
    {
        _coreEvent.triggerTurnOffMenu();
    }

    private void SaveSession()
    {
        // Logic to save the current session
        Debug.Log("Session saved.");
    }

    private void QuitSession()
    {
        _coreEvent.triggerTurnOnMenu();
    }

    private void TurnOnMenu()
    {
        menuCamera.SetActive(true);
        MainMenu.SetActive(true);
    }

    private void TurnOffMenu()
    {
        menuCamera.SetActive(false);
        MainMenu.SetActive(false);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
    }
}

/// <summary>
/// Bộ đếm thời gian.
/// </summary>
public class Timer
{
    public float _startTime;
    public bool _isCounting;

    /// <summary>
    /// Cập nhật bộ đếm thời gian.
    /// </summary>
    /// <param name="isCount"></param>
    /// <returns></returns>
    public float UpdateTimer(bool isCount)
    {
        if (isCount && !_isCounting) // Nếu bắt đầu đếm
        {
            _startTime = Time.time;
            _isCounting = true;
        }
        else if (!isCount && _isCounting) // Nếu dừng đếm
        {
            _isCounting = false;
            return Time.time - _startTime; // Trả về thời gian đã đếm
        }
        return 0f; // Nếu không đếm thì trả về 0
    }
}


// ----------------------------------------------------LUỒNG DỮ LIỆU:--------------------------------------------------

// - 

// ----------------------------------------------------TỐI ƯU:--------------------------------------------------

// - Attribute + Reflection	Tự động hóa 100%, ít lỗi	(Dùng Reflection, cần setup kỹ)
// -

// ----------------------------------------------------NOTE:--------------------------------------------------

// -
