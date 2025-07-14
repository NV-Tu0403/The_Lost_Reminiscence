using System.Linq;
using UnityEngine;
using System.Collections;

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
    public GameObject MainMenu;
    public GameObject menuCamera;
    public GameObject characterCamera;

    /// <summary>
    /// Các đối tượng sẽ được bật khi ở Scene Menu
    /// </summary>
    public GameObject[] ObjOnMenu;

    /// <summary>
    /// Các đối tượng sẽ được tắt khi ở Scene Core
    /// </summary>
    public GameObject[] ObjOffMenu;

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
        //TryInitializeCamera();
    }

    private void Start()
    {
        SetUpCamera();
        StartCoroutine(ActiveObjMenuCoroutine(true));
    }

    private void Update()
    {
        UpdateStateFix();
        TryInitializeCamera();
    }

    public override void RegisterEvent(CoreEvent e)
    {

        e.OnNewSession += NewSession;
        e.OnContinueSession += ContinueSession;
        e.OnSavePanel += E_OnSavePanel;

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
        e.OnSavePanel -= E_OnSavePanel;

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
    private void TryInitializeCamera()
    {

        if (menuCamera == null)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            menuCamera = all.FirstOrDefault(obj => obj.name == "MenuCameras");
            Debug.LogWarning("MenuCamera found!");
        }
        if (characterCamera == null)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            characterCamera = all.FirstOrDefault(obj => obj.name == "CharacterCamera(Clone)");
            Debug.LogWarning("CharacterCamera found!");
        }

    }

    /// <summary>
    /// Thiết lập mặc định của camera (menu/Player).
    /// </summary>
    private void SetUpCamera()
    {
        if (menuCamera != null && characterCamera != null)
        {
            if (!menuCamera.activeSelf)
            {
                menuCamera.SetActive(true);
            }
            else
            {
                characterCamera.gameObject.SetActive(false);
            }
        }
    }

    // debug state name
    private void UpdateCurrentCoreState(CoreStateType stateType)
    {
        CurrentCoreState = stateType.ToString();
        //Debug.Log($"[Core] CurrentCoreState updated to: {CurrentCoreState}");
    }

    private void UpdateStateFix()
    {
        if (_stateMachine.CurrentStateType != null)
        {
            Debug.Log($"[Core] Current State: {_stateMachine.CurrentStateType}");
            CurrentCoreState = _stateMachine.CurrentStateType.ToString();
        }
    }

    public void HandleAction(UIActionType action)
    {
        _stateMachine.HandleAction(action);
    }

    /// <summary>
    /// Coroutine để bật/tắt các đối tượng menu cần/ ko cần.
    /// true: bật obj menu cần
    /// false: tắt obj menu ko cần
    /// </summary>
    /// <param name="oke"></param>
    /// <returns></returns>
    private IEnumerator ActiveObjMenuCoroutine(bool oke)
    {
        if (oke)
        {
            foreach (var obj in ObjOnMenu)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
            foreach (var obj in ObjOffMenu)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
        else
        {
            foreach (var obj in ObjOnMenu)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
            foreach (var obj in ObjOffMenu)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        yield return new WaitForSeconds(2f); // Đợi 2 giây

        // TODO: thêm logic sau khi chờ nếu cần, ví dụ:
        // Debug.Log("Đã chờ 2 giây xong.");
    }

    private void NewSession()
    {
        // vì hiện tại Event NewSession không được gọi bới Core Input nên SetState ở đây cho NewSession
        _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
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
        if (CurrentCoreState != CoreStateType.InMainMenuState.ToString())
        {
            _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
            _coreEvent.triggerTurnOffMenu();
        }
    }

    private void E_OnSavePanel()
    {
        UIPage05.Instance.RefreshSaveSlots();
    }

    private void SaveSession()
    {
        Debug.Log("Session saved.");
    }

    private void QuitSession()
    {
        _stateMachine.SetState(new InMainMenuState(_stateMachine, _coreEvent));
        StartCoroutine(ActiveObjMenuCoroutine(true));
        _coreEvent.triggerTurnOnMenu();
    }

    private void TurnOnMenu()
    {
        menuCamera.SetActive(true);
        MainMenu.SetActive(true);
        characterCamera.SetActive(false);

    }

    private void TurnOffMenu()
    {
        menuCamera.SetActive(false);
        MainMenu.SetActive(false);
        StartCoroutine(ActiveObjMenuCoroutine(false));
        characterCamera.SetActive(true);
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
