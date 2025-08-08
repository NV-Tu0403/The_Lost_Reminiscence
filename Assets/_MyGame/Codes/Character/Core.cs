using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using Code.Backend;

/// <summary>
/// Trung tâm điều phối toàn bộ các State của Core game.
/// Căn cứ vào CoreState để điều tiết các Event.
/// CoreState cần được cập nhật cẩn thận khi Event được kích hoạt (CoreState là bộ quy tắc để ràng buộc các Event
/// , Giúp mọi thứ tách bạch dể hiểu và dể theo dõi hơn)
/// </summary>
public class Core : CoreEventListenerBase
{
    public static Core Instance { get; private set; }

    public StateMachine _stateMachine;
    public StateMachine _accountStateMachine;
    public UserAccountManager _userAccountManager;
    private CameraZoomController _cameraZoomController;
    public BackendSync backendSync;

    #region biến cần thiết

    #region Lộc thêm cờ để kiểm soát trạng thái của game
    public bool IsCutscenePlaying { get; set; } = false;
    public bool IsDialoguePlaying { get; set; } = false;
    public bool IsDevMode { get; set; } = false;

    #endregion

    public bool IsOffline { get; private set; } = true;
    public bool IsDebugMode = false;

    [Header("State")]
    public string CurrentAccountName;
    public string CurrentAccountState;
    public string CurrentCoreState;

    [Header("Menu")]
    public GameObject MainMenu;

    /// <summary>
    /// Các đối tượng sẽ được bật khi ở Scene Menu
    /// </summary>
    public GameObject[] ObjOnMenu;

    /// <summary>
    /// Các đối tượng sẽ được tắt khi ở Scene Core
    /// </summary>
    public GameObject[] ObjOffMenu;

    [Header("Camera")]
    public GameObject menuCameraObj;
    public GameObject characterCameraObj;
    public Camera _characterCamera;

    private string mess = null;

    #endregion

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        base.Awake();

        if (_userAccountManager == null)
        {
            _userAccountManager = UserAccountManager.Instance;
        }

        _stateMachine = new StateMachine();
        _accountStateMachine = new StateMachine();
        _stateMachine.SetState(new InMainMenuState(_stateMachine, _coreEvent));
        InitAccountState();
        //_accountStateMachine.SetState(new NoCurrentAccountState(_accountStateMachine, _coreEvent));
        //TryInitializeCamera();
        _cameraZoomController = CameraZoomController.Instance;
    }

    private void Start()
    {
        //TryInitializeCamera();
        InitAccountState();
        SetUpCamera();
        StartCoroutine(ActiveObjMenu(true));
        //StartCoroutine(RetryUpdateAccountState());
    }

    private void Update()
    {
        UpdateStateFix();
        CheckCurrentAccount();
        TryInitializeCamera();

        if (IsDebugMode)
        {
            ActiveMenu(false, false);
        }

        // Lộc thêm
        if (!IsCutscenePlaying
            && !IsDialoguePlaying
            && !IsDevMode)
        {
            bool ShowCursor = CurrentCoreState != CoreStateType.InSessionState.ToString() || IsDebugMode;
            ActiveMouseCursor(ShowCursor);
        }
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
        e.OnAccountChangeState += UpdateAccountState;

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
        e.OnAccountChangeState -= UpdateAccountState;

        e.OnQuitGame -= QuitGame;
    }

    /// <summary>
    /// Khởi tạo trạng thái tài khoản khi game bắt đầu dựa theo baseName hiện tại.
    /// </summary>
    public void InitAccountState()
    {
        string baseName = _userAccountManager.currentUserBaseName;

        if (!string.IsNullOrEmpty(baseName))
        {
            bool isSynced = _userAccountManager.IsBaseNameSynced(baseName);

            if (isSynced)
            {
                _accountStateMachine.SetState(new HaveConnectToServer(_accountStateMachine, _coreEvent));
                Debug.Log($"[InitAccountState] Tài khoản '{baseName}' đã đồng bộ → HaveConnectToServer");
                backendSync.OnDownloadDataFromCloud();
            }
            else
            {
                _accountStateMachine.SetState(new NoConnectToServerState(_accountStateMachine, _coreEvent));
                Debug.Log($"[InitAccountState] Tài khoản '{baseName}' chưa đồng bộ → NoConnectToServerState");
            }
        }
        else
        {
            _accountStateMachine.SetState(new NoCurrentAccountState(_accountStateMachine, _coreEvent));
            Debug.Log($"[InitAccountState] Không có baseName → NoCurrentAccountState");
        }
    }


    /// <summary>
    /// đảm bảo rằng MenuCamera đã được khởi tạo và kích hoạt.
    /// </summary>
    private void TryInitializeCamera()
    {

        if (characterCameraObj == null)
        {
            var characterCamera = FindObjectsByType<CharacterCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Select(cam => cam.gameObject)
                .FirstOrDefault();
            if (characterCamera != null)
            {
                characterCameraObj = characterCamera.gameObject;
                _characterCamera = characterCamera.GetComponent<Camera>();
            }
            else
            {
                Debug.LogWarning("CharacterCamera not found!");
            }
        }

        if (menuCameraObj == null)
        {
            menuCameraObj = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(obj => obj.name == "MenuCameras");
            if (menuCameraObj == null)
                Debug.LogWarning("MenuCamera not found!");
        }

    }


    /// <summary>
    /// Thiết lập mặc định của camera (menu/Player).
    /// </summary>
    private void SetUpCamera()
    {
        if (menuCameraObj != null && characterCameraObj != null)
        {
            if (!menuCameraObj.activeSelf)
            {
                menuCameraObj.SetActive(true);
            }
            else
            {
                characterCameraObj.gameObject.SetActive(false);
            }
        }
    }

    // debug state name
    private void UpdateCurrentCoreState(CoreStateType stateType)
    {
        CurrentCoreState = stateType.ToString();
        //Debug.Log($"[Core] CurrentCoreState updated to: {CurrentCoreState}");
    }

    private void UpdateAccountState(AccountStateType accountStateType) 
    {
        try
        {
            CurrentAccountState = accountStateType.ToString();
            StartCoroutine(RetryUpdateAccountState(accountStateType));
            mess = "đã cập nhật trạng thái tài khoản: " + CurrentAccountState;
        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}", e);
        }
    }

    private IEnumerator RetryUpdateAccountState(AccountStateType Type)
    {
        yield return new WaitForSeconds(0.1f);
        if (_accountStateMachine.CurrentStateType != null)
        {
            UiPage06_C.Instance.UpdateInfo(CurrentAccountName, PlayTimeManager.Instance.SessionPlayTime.ToString(), Type);
            UiPage06_C.Instance.UpdateTextFields(Type);
        }
    }

    private void UpdateStateFix()
    {
        if (_stateMachine.CurrentStateType != null)
        {
            //Debug.Log($"[Core] Current State: {_stateMachine.CurrentStateType}");
            CurrentCoreState = _stateMachine.CurrentStateType.ToString();
        }
    }

    public void HandleAction(UIActionType action)
    {
        _stateMachine.HandleAction(action);
    }

    /// <summary>
    /// True: bật menu.
    /// False: tắt menu
    /// </summary>
    /// <param name="oke"></param>
    public void ActiveMenu(bool MenuOke, bool ObjOke)
    {
        characterCameraObj.SetActive(!MenuOke);
        menuCameraObj.SetActive(MenuOke);
        MainMenu.SetActive(MenuOke);
        StartCoroutine(ActiveObjMenu(ObjOke));

    }

    /// <summary>
    /// True: bật obj menu cần.
    /// False: tắt obj menu ko cần
    /// </summary>
    /// <param name="oke"></param>
    /// <returns></returns>
    public IEnumerator ActiveObjMenu(bool oke)
    {
        if ((ObjOnMenu != null))
        {
            foreach (var obj in ObjOnMenu)
            {
                if (obj != null)
                    obj.SetActive(oke);
            }
        }
        if (ObjOffMenu != null)
        {
            foreach (var obj in ObjOffMenu)
            {
                if (obj != null)
                    obj.SetActive(!oke);
            }
        }

        yield return new WaitForSeconds(0.2f);
    }

    /// <summary>
    /// True: Bật.
    /// False: Tắt.
    /// </summary>
    /// <param name="oke"></param>
    public void ActiveMouseCursor(bool oke)
    {
        //bool ShowCursor = CurrentCoreState != CoreStateType.InSessionState.ToString() || IsDebugMode;

        if (IsDebugMode)
        {
            oke = false;
        }

        if (oke)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = oke;
            //Debug.Log("Cursor is visible and unlocked.");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !oke;
        }
    }

    #region B1

    private void CheckCurrentAccount()
    {
        if (_userAccountManager.currentUserBaseName != null)
        {
            CurrentAccountName = _userAccountManager.currentUserBaseName;
            //Debug.Log($"Current Account: {CurrentAccountName}");
        }
        else
        {
            CurrentAccountName = null;
            Debug.LogWarning("No current account found.");
        }
    }

    /// <summary>
    /// đăng ký tài khoản mới.
    /// </summary>
    public void RegisterAccount(string userName, string passWord)
    {
        try
        {
            if (_userAccountManager.CreateAccount(userName, passWord, out string errorMessage))
            {
                UiPage06_C.Instance.ShowLogMessage($"Ac '{userName}' tạo thành công.");
                if (!_userAccountManager.IsSynced(userName))
                {
                    LoginAccount(userName, passWord); // tự đăng nhập ngay sau khi đăng ký thành công
                    _accountStateMachine.SetState(new NoConnectToServerState(_accountStateMachine, _coreEvent));
                }
            }
            UiPage06_C.Instance.ShowLogMessage(errorMessage);
        }
        catch (Exception e)
        {
            UiPage06_C.Instance.ShowLogMessage($"Lỗi đăng ký tài khoản: {e.Message}");
            throw new Exception($"[RegisterAccount] Error during account registration: {e.Message}", e);
        }
    }

    /// <summary>
    /// đăng nhập thủ công
    /// </summary>
    public void LoginAccount(string userName, string passWord)
    {
        try
        {
            if (_userAccountManager.Login(userName, passWord, out string errorMessage))
            {
                UiPage06_C.Instance.ShowLogMessage(errorMessage);
                if (!_userAccountManager.IsSynced(userName))
                {
                    _accountStateMachine.SetState(new NoConnectToServerState(_accountStateMachine, _coreEvent));
                }
                else
                {
                    _accountStateMachine.SetState(new HaveConnectToServer(_accountStateMachine, _coreEvent));
                }
            }
            UiPage06_C.Instance.ShowLogMessage(errorMessage);
        }
        catch (Exception e)
        {
            UiPage06_C.Instance.ShowLogMessage($"Lỗi đăng nhập: {e.Message}");
            throw new Exception($"[LoginAccount] Error during login: {e.Message}", e);
        }
    }

    public bool LogoutAccount()
    {
        if (_userAccountManager.Logout(out string errorMessage))
        {
            UiPage06_C.Instance.ShowLogMessage(errorMessage);
            _accountStateMachine.SetState(new NoCurrentAccountState(_accountStateMachine, _coreEvent));
            return true;
        }
        else
        {
            Debug.LogError($"[LogoutAccount] Failed to logout: {errorMessage}");
            return false;
        }
    }

    public void SyncToServer(string userName, string passWord, string email)
    {
        //mess = null;
        try
        {
            // yêu cầu đăng nhập lại trước khi đồng bộ
            if (!_userAccountManager.Login(userName, passWord, out string errorMessage))
            {
                UiPage06_C.Instance.ShowLogMessage($"Đăng nhập thất bại: {errorMessage}");
                Debug.LogWarning($"Cloud register login check failed: {errorMessage}");
                return;
            }

            // kiểm tra email format (cơ bản) (không phải iem dành việc đâu nah chỉ là iem không muống server phải bỏ thêm băng thông để xử lí mấy lỗi vặt thôi)
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                Debug.LogWarning("Invalid email for cloud register");
                return;
            }

            // yêu cầu đăng ký OTP để đồng bộ Account to Server
            StartCoroutine(backendSync.RequestCloudRegister(userName, passWord, email, (success, message) =>
            {
                if (success)
                {
                    _accountStateMachine.SetState(new ConectingServer(_accountStateMachine, _coreEvent));
                    mess = message;
                    UiPage06_C.Instance.ShowLogMessage($"Đăng ký OTP thành công. Vui lòng kiểm tra email: {email}");
                }
                else
                {
                    mess = message;
                    Debug.LogWarning($"Cloud register failed: {message}");
                }
            }));
        }
        catch (Exception e)
        {
            UiPage06_C.Instance.ShowLogMessage($"Lỗi đăng ký tài khoản trên máy chủ: {e.Message}");
            throw new Exception($"[YsncToServer] Error during cloud registration: {e.Message}", e);
        }
        finally
        {
            if (mess != null)
            {
                UiPage06_C.Instance.ShowLogMessage(mess);
            }
        }
    }

    public void VerifyOTPAccount(string otp, string userName)
    {
        try
        {
            if (string.IsNullOrEmpty(otp))
            {
                UiPage06_C.Instance.ShowLogMessage("OTP không được để trống.");
                //Debug.LogWarning("OTP is null or empty");
                return;
            }

            StartCoroutine(backendSync.VerifyOtp(userName, otp, (success, message) =>
            {
                if (success)
                {
                    UiPage06_C.Instance.ShowLogMessage("Xác thực OTP thành công. Tài khoản đã được đồng bộ với máy chủ.");
                    if (_userAccountManager.MarkAsSynced(userName))
                    {
                        _accountStateMachine.SetState(new HaveConnectToServer(_accountStateMachine, _coreEvent));
                    }
                    else
                    {
                        UiPage06_C.Instance.ShowLogMessage("Không thể đánh dấu tài khoản đã đồng bộ sau khi xác thực OTP.");
                        //Debug.LogWarning("Failed to mark user as synced after OTP verification");
                    }
                }
                else
                {
                    UiPage06_C.Instance.ShowLogMessage($"Xác thực OTP thất bại: {message}");
                    Debug.LogWarning($"OTP verification failed: {message}");
                }
            }));
        }
        catch (Exception e)
        {
            UiPage06_C.Instance.ShowLogMessage($"Lỗi xác thực OTP: {e.Message}");
            throw new Exception($"[OnOtp] Error during OTP verification: {e.Message}", e);
        }
    }

    #endregion

    #region B2

    private void NewSession()
    {
        // vì hiện tại Event NewSession không được gọi bới Core Input nên SetState ở đây cho NewSession
        _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
    }

    private void ContinueSession()
    {
        // vì hiện tại Event OnContinue không được gọi bới Core Input nên SetState ở đây cho Continue
        _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
    }

    private void PauseSession()
    {
        ActiveMenu(true, false);
    }

    private void ResumeSession()
    {
        if (CurrentCoreState != CoreStateType.InMainMenuState.ToString())
        {
            _stateMachine.SetState(new InSessionState(_stateMachine, _coreEvent));
            ActiveMenu(false, false);
        }
    }

    private void E_OnSavePanel()
    {
        _ = UIPage05.Instance.RefreshSaveSlots();
    }

    private void SaveSession()
    {
        Debug.Log("Session saved.");
    }

    private void QuitSession()
    {
        _stateMachine.SetState(new InMainMenuState(_stateMachine, _coreEvent));
        //StartCoroutine(ActiveObjMenu(true));
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

    #endregion
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

//-

//----------------------------------------------------TỐI ƯU: --------------------------------------------------

//-Attribute + Reflection   Tự động hóa 100%, ít lỗi	(Dùng Reflection, cần setup kỹ)
// -

// ----------------------------------------------------NOTE:--------------------------------------------------

// -

