using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using _MyGame.Codes.Backend;
using TMPro;
using _MyGame.Codes.Procession;


/// <summary>
/// Điều phối các nghiệp vụ chuyên môn.
/// Đăng kí Logic nghiệp vụ cho các Event ở đây, có thể gọi trigger các sự kiện từ đây (cẩn thận tránh lặp vô hạn).
/// 
/// </summary>
public class ProfessionalSkilMenu : CoreEventListenerBase
{
    public static ProfessionalSkilMenu Instance { get; private set; }

    private Core _core;
    public BackendSync backendSync;
    private CutSceneController cutSceneController;

    private string lastSelectedSaveFolder;
    public string selectedSaveFolder;
    public string SelectedSaveImagePath;

    [Header("BackUp")]
    public string CurrentbackupSavePath; // đường dẫn đến thư mục backup hiện tại
    public string CurrentOriginalSavePath; // đường dẫn đến thư mục gốc của bản backup hiện tại
    public bool CurrentbackupOke; // đánh dấu xem bản backup hiện tại có hợp lệ hay không

    public string SceneDefault = "MainScene";

    [SerializeField] private GameObject[] CallBackObject;

    private string mess = null;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }

        _core = Core.Instance;
        cutSceneController = CutSceneController.Instance;
    }

    private void Start()
    {
        if (_core == null)
        {
            _core = Core.Instance;
        }
        RegisterSaveables();
        CheckUserAccounts();
    }

    public override void RegisterEvent(CoreEvent e)
    {
        e.OnNewSession += () => OnNewGame();
        e.OnContinueSession += () => OnContinueGame(selectedSaveFolder);
        e.OnSaveSession += () => OnSaveSession(lastSelectedSaveFolder);
        e.OnQuitSession += async () => await OnQuitSession(lastSelectedSaveFolder);

        e.OnSelectSaveItem += (path) => OnSelectSave(path);
        e.OnSyncFileSave += async (path) => await BackUpSaveItemAsync(path);
        e.OnOverriceSave += () => SetBackUpSaveItemAsync(); // ghi đè bản backup lên bản gốc nếu có bản backup hợp lệ.

        e.OnLogin += () => PerformLoginCloud();
        e.OnRegister += () => PerformRegisterCould();
        e.OnLogout += async () => await PerformLogout();
        //e.OnConnectToServer += () => PerformLoginCloud ();
        e.OnConnectingToServer += () => PerformOtp();

        //e.OnChangeScene += async (nameScene, pos) => await OnChangeScene(nameScene, pos);
        e.OnChangeScene += (nameScene, pos) => OnChangeScene(nameScene, pos);


    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.OnNewSession -= () => OnNewGame();
        e.OnContinueSession -= () => OnContinueGame(selectedSaveFolder);
        e.OnSaveSession -= () => OnSaveSession(lastSelectedSaveFolder);
        e.OnQuitSession -= async () => await OnQuitSession(lastSelectedSaveFolder);

        e.OnSelectSaveItem -= (path) => OnSelectSave(path);
        e.OnSyncFileSave -= async (path) => await BackUpSaveItemAsync(path);
        e.OnOverriceSave -= () => SetBackUpSaveItemAsync();

        e.OnLogin -= () => PerformLoginCloud();
        e.OnRegister -= () => PerformRegisterCould();
        e.OnLogout -= async () => await PerformLogout();
        //e.OnConnectToServer -= () => PerformLoginCloud();
        e.OnConnectingToServer -= () => PerformOtp();

        e.OnChangeScene -= (nameScene, pos) => OnChangeScene(nameScene, pos);



    }

    #region nghiệp vụ 1

    /// <summary>
    /// Đăng ký các đối tượng có thể lưu trữ dữ liệu vào SaveGameManager.
    /// </summary>
    private void RegisterSaveables()
    {
        SaveGameManager.Instance.RegisterSaveable(PlayTimeManager.Instance);
        SaveGameManager.Instance.RegisterSaveable(PlayerCheckPoint.Instance);
        SaveGameManager.Instance.RegisterSaveable(MapStateSave.Instance);
        SaveGameManager.Instance.RegisterSaveable(ProgressionManager.Instance);
    }

    /// <summary>
    /// Kiểm tra xem có người dùng nào đã đăng nhập và có tài khoản lưu trữ không. Nếu không, hiển thị giao diện đăng nhập.
    /// </summary>
    private void CheckUserAccounts()
    {
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath) ||
            JsonUtility.FromJson<UserAccountData>(File.ReadAllText(userAccountsPath)).Users.Count == 0) // nếu không có tài khoản nào
        {
            lastSelectedSaveFolder = null;
            // set trạng thái không có tài khoản hiện tại
            _core._accountStateMachine.SetState(new NoConnectToServer(_core._accountStateMachine, _coreEvent));
            Debug.Log("[Test] No users found. Showing login panel.");
        }
        else
        {
            PerformAutoLogin(); // tự động đăng nhập nếu có tài khoản
        }
    }

    /// <summary>
    /// Thử tự động đăng nhập 
    /// </summary>
    private void AutoLoginLocal(bool success, string message)
    {
        //if (success)
        //{
        //    Debug.LogError($"[AutoLoginLocal] Auto login failed: {message}");
        //    UiPage06_C.Instance.ShowLogMessage(message);
        //    return;
        //}
        if (_core._userAccountManager.TryAutoLoginLocal(out string Message))
        {
            UiPage06_C.Instance.ShowLogMessage(Message);
            lastSelectedSaveFolder = GetValidLastSaveFolder();
            PlayTimeManager.Instance.StartCounting();

            _core.InitAccountState();

            UiPage06_C.Instance.ActiveObj(true, false, false, false);
        }
        else
        {
            lastSelectedSaveFolder = null;
        }
    }

    /// <summary>
    /// Lấy thư mục lưu trữ hợp lệ gần nhất từ UserAccountManager.
    /// </summary>
    /// <returns></returns>
    private string GetValidLastSaveFolder()
    {
        string lastFileSave = UserAccountManager.Instance.GetLastFileSave();
        if (!string.IsNullOrEmpty(lastFileSave))
        {
            string fileSavePath = Path.Combine(Application.persistentDataPath, "User_DataGame",
                $"FileSave_{UserAccountManager.Instance.CurrentUserBaseName}", lastFileSave);
            if (Directory.Exists(fileSavePath) && Directory.GetFiles(fileSavePath, "*.json").Length > 0)
            {
                return fileSavePath;
            }
        }

        var saves = SaveGameManager.Instance.GetAllSaveFolders(UserAccountManager.Instance.CurrentUserBaseName);
        string latestFolder = saves.Count > 0 ? saves[0].FolderPath : null;
        //ContinueGame_Bt.interactable = !string.IsNullOrEmpty(latestFolder);
        return latestFolder;
    }

    /// <summary>
    /// Làm mới danh sách các thư mục lưu trữ và trả về SaveListContext để cập nhật UI.
    /// </summary>
    /// <returns></returns>
    public SaveListContext RefreshSaveList()
    {

        // Kiểm tra null trước khi sử dụng
        if (UserAccountManager.Instance == null)
        {
            Debug.LogWarning("UserAccountManager.Instance is null! Trying get");
            return new SaveListContext { UserName = null, Saves = new List<SaveFolder>(), IsContinueEnabled = false };
        }
        if (SaveGameManager.Instance == null)
        {
            Debug.LogWarning("SaveGameManager.Instance is null!");
            return new SaveListContext { UserName = UserAccountManager.Instance.currentUserBaseName, Saves = new List<SaveFolder>(), IsContinueEnabled = false };
        }
        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            //Debug.LogError("currentUserBaseName is null or empty!");
            // Wait 1 second and try again
            //StartCoroutine(RetryRefreshSaveListAfterDelay());
            //return new SaveListContext { UserName = null, Saves = new List<SaveFolder>(), IsContinueEnabled = false };
        }

        var saves = SaveGameManager.Instance.GetAllSaveFolders(UserAccountManager.Instance.currentUserBaseName);
        bool isContinueEnabled = saves.Any();
        return new SaveListContext
        {
            UserName = UserAccountManager.Instance.currentUserBaseName,
            Saves = saves.Select(s => new SaveFolder { FolderPath = s.FolderPath, ImagePath = s.ImagePath }).ToList(), // Chỉ lấy đường dẫn thư mục và ảnh
            IsContinueEnabled = isContinueEnabled
        };
    }

    public void RefreshSaveImage(string currentSaveFolder)
    {
        string imagePath = Path.Combine(currentSaveFolder, "screenshot.png");
        SelectedSaveImagePath = File.Exists(imagePath) ? imagePath : null;
        ScreenshotDisplayer.Instance.LoadScreenshotToPlane(SelectedSaveImagePath);
    }

    private IEnumerator RetryRefreshSaveListAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        RefreshSaveList();
    }

    /// <summary>
    /// Chờ cho đến khi Player được khởi tạo và áp dụng vị trí đã lưu.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitUntilPlayerAndApply()
    {
        Transform p = null;
        while (p == null)
        {
            p = GameObject.FindGameObjectWithTag("Player")?.transform;
            yield return null;
        }

        if (PlayerCheckPoint.Instance.ApplyLoadedPositionStrict())
        {
            Debug.Log($" Player posion: {p.position}");
        }else
        {
            StartCoroutine( WaitUntilPlayerAndApply() );
        }
    }
    #endregion

    #region nghiep vụ 2

    public string OnNewGame()
    {

        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            Debug.LogError($"currentUserBaseName == {UserAccountManager.Instance.currentUserBaseName}");
        }

        string newSaveFolder = SaveGameManager.Instance.CreateNewSaveFolder(_core.CurrentAccountName);

        selectedSaveFolder = newSaveFolder;
        lastSelectedSaveFolder = newSaveFolder;

        PlayTimeManager.Instance.ResetSession();
        PlayTimeManager.Instance.StartCounting();

        if (SceneController.Instance == null)
        {
            Debug.LogError("[OnNewGame] SceneController.Instance is null!");
            return null;
        }

        if (PlayerCheckPoint.Instance == null)
        {
            Debug.LogError("[OnNewGame] PlayerCheckPoint.Instance is null!");
            return null;
        }

        string sceneToLoad = SceneDefault;

        Action<string> onFullyReady = null;
        onFullyReady = (loadedSceneName) =>
        {
            if (!string.Equals(loadedSceneName, sceneToLoad, StringComparison.Ordinal)) return;

            // unsubscribe ngay lập tức
            SceneController.Instance.OnSceneFullyReady -= onFullyReady;

            // đảm bảo Player có tồn tại; nếu không, log + thử chờ ngắn (optional)
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogError("[OnNewGame] Player not found after scene fully ready.");
                return;
            }

            ProgressionManager.Instance.InitProgression();

            PlayerCheckPoint.Instance.AssignCameraFromCore();
            PlayerCheckPoint.Instance.SetPlayerTransform(playerGO.transform);
            PlayerCheckPoint.Instance.ResetPlayerPositionWord();

            SaveGameManager.Instance.SaveToFolder(newSaveFolder);
        };

        // subscribe và load
        SceneController.Instance.OnSceneFullyReady += onFullyReady;
        SceneController.Instance.LoadAdditiveScene(sceneToLoad);
        return newSaveFolder;
    }

    public void OnContinueGame(string saveFolder)
    {
        if (string.IsNullOrEmpty(saveFolder) || !Directory.Exists(saveFolder))
        {
            throw new Exception("Invalid save folder!");
        }

        SaveGameManager.Instance.LoadFromFolder(saveFolder);
        string sceneToLoad = PlayerCheckPoint.Instance.CurrentMap;
        if (string.IsNullOrEmpty(sceneToLoad) || sceneToLoad == "Unknown" || sceneToLoad == "Menu")
        {
            sceneToLoad = SceneDefault;
            Debug.LogError($"[OnContinueGame] Invalid scene name '{PlayerCheckPoint.Instance.CurrentMap}', loading default scene '{sceneToLoad}' instead.");
        }

        Action<string> onFullyReady = null;
        onFullyReady = (loadedSceneName) =>
        {
            if (!string.Equals(loadedSceneName, sceneToLoad, StringComparison.Ordinal)) return;
            SceneController.Instance.OnSceneFullyReady -= onFullyReady;

            PlayTimeManager.Instance.StartCounting();
            MapStateSave.Instance.ApplyMapState();
            PlayerCheckPoint.Instance.AssignCameraFromCore();

            StartCoroutine(WaitForPlayerThenApply(3f));
            ProgressionManager.Instance.AfterLoad();
        };

        SceneController.Instance.OnSceneFullyReady += onFullyReady;
        SceneController.Instance.LoadAdditiveScene(sceneToLoad);

        Debug.Log($"[Continue Session] Load Position: {PlayerCheckPoint.Instance.PlayerTransform.position}");
    }

    private IEnumerator WaitForPlayerThenApply(float timeoutSeconds)
    {
        float start = Time.realtimeSinceStartup;
        Transform player = null;
        while ((Time.realtimeSinceStartup - start) < timeoutSeconds)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null) break;
            yield return null;
        }

        if (player == null)
        {
            Debug.LogError("[WaitForPlayerThenApply] Player not found after waiting.");
            yield break;
        }

        PlayerCheckPoint.Instance.ApplyLoadedPositionStrict();
    }

    /// <summary>
    /// nhận vào đường dẫn thư mục để đánh dấu là thư mục đã chọn.
    /// load dữ liệu từ thư mục đó.
    /// </summary>
    /// <param name="folderPath"></param>
    public void OnSelectSave(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        selectedSaveFolder = null;
        lastSelectedSaveFolder = null;
        selectedSaveFolder = folderPath;
        lastSelectedSaveFolder = folderPath;

        SaveGameManager.Instance.LoadFromFolder(folderPath);

        var jsonFileHandler = new JsonFileHandler();                // tạo mới JsonFileHandler để lấy danh sách các file JSON
        var jsonFiles = jsonFileHandler.LoadJsonFiles(folderPath);  // lấy danh sách các file JSON

        //foreach (var (fileName, json) in jsonFiles)
        //{
        //    Debug.Log($"[ProfessionalSkilMenu] OnSelectSave - {fileName}:\n{json}");
        //}

        RefreshSaveImage(folderPath);
    }

    /// <summary>
    /// Xử lý khi người dùng nhấn xóa một thư mục lưu trữ.
    /// </summary>
    /// <param name="folderPath"></param>
    public async Task OnDeleteSave(string folderPath)
    {
        if (SaveGameManager.Instance.DeleteSaveFolder(folderPath))
        {
            if (folderPath == lastSelectedSaveFolder)
            {
                lastSelectedSaveFolder = null;
                selectedSaveFolder = null;
                await UIPage05.Instance.RefreshSaveSlots();
            }
        }
        else
        {
            Debug.LogError($"[OnDeleteSave] Failed to delete save folder: {folderPath}");
        }
    }

    /// <summary>
    /// Lưu tất cả dữ liệu của người dùng hiện tại vào thư mục lưu trữ tương ứng.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void OnSaveSession(string currentSaveFolder)
    {
        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            throw new Exception("No user logged in!");
        }
        //SaveGameManager.Instance.SaveAll(UserAccountManager.Instance.currentUserBaseName);
        if (!string.IsNullOrEmpty(currentSaveFolder))
        {
            try
            {
                SaveGameManager.Instance.SaveToFolder(currentSaveFolder);
                StartCoroutine(CaptureScreenshotToFolder(currentSaveFolder, success =>
                {
                    if (success)
                    {
                        //Core.Instance.TurnOnMenu();
                    }
                    else
                    {
                        Debug.LogWarning("Screenshot capture failed!");
                    }
                }));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OnQuitSession] Failed to save before unloading: {ex.Message}");
            }
        }

    }

    public async Task OnQuitSession(string currentSaveFolder)
    {
        if (!string.IsNullOrEmpty(currentSaveFolder))
        {
            try
            {
                Core.Instance.ActiveMenu(false, false); // tạm tắt menu để chụp ảnh

                SaveGameManager.Instance.SaveToFolder(currentSaveFolder);
                StartCoroutine(CaptureScreenshotToFolder(currentSaveFolder, success =>
                {
                    if (success)
                    {
                        SceneController.Instance.UnloadAllAdditiveScenes(() =>
                        {
                            mess = "Đã lưu thành công và chuẩn bị thoát khỏi phiên chơi.";

                        });

                        PlayerCheckPoint.Instance.ResetPlayerPositionWord();
                        // Gọi lại load ảnh tại đây – sau khi ảnh mới đã được chụp xong
                        RefreshSaveImage(currentSaveFolder);
                        Core.Instance.ActiveMenu(true, true); // bật lại menu (hoàn thành QuitSesion)
                    }
                    else
                    {
                        Debug.LogWarning("Screenshot capture failed!");
                    }
                }));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OnQuitSession] Failed to save before unloading: {ex.Message}");
            }
        }
        await UIPage05.Instance.RefreshSaveSlots();
    }

    /// <summary>
    /// Chụp ảnh màn hình kích thước 1024x1024 và lưu vào thư mục truyền vào.
    /// </summary>
    /// <param name="folderPath">Đường dẫn thư mục lưu ảnh</param>
    /// <returns>Coroutine IEnumerator</returns>
    public IEnumerator CaptureScreenshotToFolder(string folderPath, Action<bool> onComplete)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"[CaptureScreenshotToFolder] Folder does not exist: {folderPath}");
            onComplete?.Invoke(false);
            yield break;
        }

        yield return new WaitForSeconds(0.8f);  // đảm bảo UI đã tắt hoàn toàn
        yield return null; // đợi 1 frame

        int width = 1920;
        int height = 1080;
        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Fix: Check for null _core and _core._characterCamera
        if (_core == null || _core._characterCamera == null)
        {
            Debug.LogError("[CaptureScreenshotToFolder] Core or player camera not found.");
            onComplete?.Invoke(false);
            yield break;
        }

        var mainCam = _core._characterCamera;

        if (mainCam == null)
        {
            Debug.LogError("[CaptureScreenshotToFolder] Player camera not found.");
            onComplete?.Invoke(false);
            yield break;
        }

        mainCam.targetTexture = rt;
        mainCam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        mainCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        try
        {
            byte[] bytes = screenShot.EncodeToPNG();
            string screenshotPath = Path.Combine(folderPath, "screenshot.png");
            File.WriteAllBytes(screenshotPath, bytes);
            //Debug.Log($"[CaptureScreenshotToFolder] Screenshot saved to: {screenshotPath}");

            onComplete?.Invoke(true); // hoàn thành
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CaptureScreenshotToFolder] Failed to save screenshot: {ex.Message}");
            onComplete?.Invoke(false);
        }
    }

    public bool OnChangeScene(string nameScene, Vector3 posSpawn)
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("[OnChangeScene] Player not found before change.");
            return false;
        }

        cutSceneController.PlayCutScene(UIActionType.ContinueSession);

        // Unload other additive scenes first (non-blocking callback)
        SceneController.Instance.UnloadAllAdditiveScenes(() =>
        {
            Debug.Log($"[OnChangeScene] Unloaded previous additive scenes.");
        });

        Action<string> onFullyReady = null;
        onFullyReady = (loadedSceneName) =>
        {
            if (!string.Equals(loadedSceneName, nameScene, StringComparison.Ordinal)) return;
            SceneController.Instance.OnSceneFullyReady -= onFullyReady;

            // Move persistent player into the newly loaded scene, then set position
            var newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(nameScene);
            //if (newScene.IsValid())
            //    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(player.gameObject, newScene);

            //player.position = posSpawn;

            Debug.Log($"[OnChangeScene] Player moved and set pos: {player.position}");
        };

        SceneController.Instance.OnSceneFullyReady += onFullyReady;
        SceneController.Instance.LoadAdditiveScene(nameScene);

        PlayerCheckPoint.Instance.ResetPlayerPositionWord();
        return true;
    }

    #endregion

    #region Nghiệp vụ 3

    private void PerformAutoLogin()
    {
        backendSync.OnAutoLogin(AutoLoginLocal);
        _core.InitAccountState();
        UiPage06_C.Instance.ActiveObj(true, false, false, false);
    }

    private void PerformRegisterCould()
    {
        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.ConnectToServer);

        if (inputs.Length < 3)
        {
            UiPage06_C.Instance.ShowLogMessage($"Không đủ input field cho đăng ký (cần ít nhất 3).{inputs.Length}");
            return;
        }

        string userName = inputs[0].text;
        string passWord = inputs[1].text;
        string email = inputs[2].text;

        if (string.IsNullOrWhiteSpace(userName) /*|| string.IsNullOrWhiteSpace(passWord) */|| string.IsNullOrWhiteSpace(email))
        {
            //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu hoặc email trống.");
            UiPage06_C.Instance.ShowLogMessage("du ma... đừng để trống name với pass hoặc email ní..");
            return;
        }

        if (_core.CurrentAccountState == AccountStateType.NoConnectToServer.ToString())
        {
            backendSync.OnRegister(userName, passWord, email, (success, message) =>
            {
                if (success)
                {
                    _core._accountStateMachine.SetState(new ConectingServer(_core._accountStateMachine, _coreEvent));
                    _core._userAccountManager.SetupLocalAccount(userName, out string mess);
                    mess = message;
                    UiPage06_C.Instance.ShowLogMessage($"Đăng ký OTP thành công. Vui lòng kiểm tra email: {email}");
                }
                else
                {
                    mess = message;
                    Debug.LogWarning($"Cloud register failed: {message}");
                }
            });

            foreach (var input in inputs)
            {
                input.text = string.Empty;
            }
        }
        else
        {
            UiPage06_C.Instance.ShowLogMessage("Bạn đã đăng nhập vào máy chủ rồi, không cần đăng ký nữa.");
        }
    }

    /// <summary>
    /// kiểm tra các trường Input.
    /// </summary>
    private void PerformLoginCloud()
    {
        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.ConnectToServer);

        if (inputs.Length < 3)
        {
            UiPage06_C.Instance.ShowLogMessage($"Không đủ input field cho đăng ký (cần ít nhất 3).{inputs.Length}");
            return;
        }

        string userName = inputs[0].text;
        string passWord = inputs[1].text;
        string email = inputs[2].text;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            if (userName == "Guest")
            {
                if (_core._userAccountManager.LoginGuest(userName))
                {
                    mess = "Đăng nhập thành công với tài khoản khách.";
                    return;
                }
                else
                {
                    Debug.LogError($"[PerformLoginCloud] Auto login failed: ");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(userName) /*|| string.IsNullOrWhiteSpace(passWord) */|| string.IsNullOrWhiteSpace(email))
                {
                    //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu hoặc email trống.");
                    UiPage06_C.Instance.ShowLogMessage("Tên đăng nhập hoặc mật khẩu hoặc email trống..");
                    return;
                }
            }



            if (_core.CurrentAccountState == AccountStateType.NoConnectToServer.ToString())
            {
                backendSync.OnLogin(email, passWord, (success, message) =>
                {
                    if (success)
                    {
                        _core._accountStateMachine.SetState(new HaveConnectToServer(_core._accountStateMachine, _coreEvent));
                        _core._userAccountManager.SetupLocalAccount(userName, out string mess);
                        UiPage06_C.Instance.ShowLogMessage("Đăng nhập thành công và tải xuống dữ liệu lưu trữ.");
                        //backendSync.OnDownloadDataFromCloud();
                        _coreEvent.TriggerAccountChangeState(AccountStateType.HaveConnectToServer);
                    }
                    else
                    {
                        UiPage06_C.Instance.ShowLogMessage($"Đăng nhập thất bại: {message}");
                        Debug.LogWarning($"Cloud login failed: {message}");
                    }
                });
            }

            foreach (var input in inputs)
            {
                input.text = string.Empty;
            }
        }
    }

    /// <summary>
    /// kiểm tra các trường Input và gọi hàm backendSync.OnVerifyOtp.
    /// </summary>
    private void PerformOtp()
    {
        string userName = _core._userAccountManager.currentUserBaseName;

        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.ConnectingToServer);

        if (inputs.Length < 1)
        {
            Debug.LogError("Không đủ input field cho login (cần ít nhất 1).");
            return;
        }

        string otp = inputs[0].text;

        if (string.IsNullOrWhiteSpace(otp))
        {
            //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu trống.");
            UiPage06_C.Instance.ShowLogMessage("vl... OTP để trống kìa.");
            return;
        }

        backendSync.OnVerifyOtp(userName, otp, (success, message) =>
        {
            if (success)
            {
                _core._accountStateMachine.SetState(new HaveConnectToServer(_core._accountStateMachine, _coreEvent));
                UiPage06_C.Instance.ShowLogMessage("Xác thực OTP thành công. Tài khoản đã được đồng bộ với máy chủ.");

            }
            else
            {
                UiPage06_C.Instance.ShowLogMessage($"Xác thực OTP thất bại: {message}");
                Debug.LogWarning($"OTP verification failed: {message}");
            }
        });

        foreach (var input in inputs)
        {
            input.text = string.Empty;
        }
    }

    private async Task PerformLogout()
    {
        _core.LogoutAccount();
        lastSelectedSaveFolder = null;
        selectedSaveFolder = null;
        await UIPage05.Instance.RefreshSaveSlots();
    }

    #endregion

    #region Nghiệp vụ 4

    /// <summary>
    /// ghi đè file save gốc bằng file backup với path được trả về bởi CheckBackupSaveAsync
    /// </summary>
    /// <param name="pathSelectSaveFolder"></param>
    /// <returns></returns>
    public bool SetBackUpSaveItemAsync()
    {
        try
        {
            string originalPath = CurrentOriginalSavePath;
            string backupPath = CurrentbackupSavePath;
            bool found = CurrentbackupOke;

            if (!found || string.IsNullOrEmpty(backupPath) || string.IsNullOrEmpty(originalPath))
            {
                Debug.LogError($"[SetBackUpSaveItemAsync] Không tìm thấy bản backup hợp lệ hoặc đường dẫn không hợp lệ: {backupPath} - {originalPath}");
                return false;
            }




            if (CopyDirectory(originalPath, backupPath))
            {
                mess = "Đã khôi phục thành công bản backup.";
            }
            else
            {
                mess = "Lỗi khi khôi phục bản backup. Vui lòng kiểm tra lại.";
                return false;
            }


            Task.Delay(500).Wait();

            if (ClearGetBackupTrayAsync())
            {
                mess += "Đã xóa thư mục GetBackUpTray.";
            }
            else
            {
                mess += "Không thể xóa thư mục GetBackUpTray.";
            }

            UiPage06_C.Instance.ShowLogMessage(mess);

            CurrentOriginalSavePath = null; // reset đường dẫn gốc
            CurrentbackupSavePath = null; // reset đường dẫn backup
            CurrentbackupOke = false; // reset trạng thái backup

            return true;

        }
        catch (Exception e)
        {
            mess = $"Lỗi khi khôi phục backup: {e.Message}";
            return false;
        }
        finally
        {

            //await UIPage05.Instance.RefreshSaveSlots();
            UiPage06_C.Instance.ShowLogMessage(mess);
#if UNITY_EDITOR
            Debug.Log(mess);
            UiPage06_C.Instance.ShowLogMessage(mess);
#endif
        }
    }

    /// <summary>
    /// trả về 2 path là đường dẫn đến thư mục backup và đường dẫn đến thư mục có tên gốc của bản backup.
    /// </summary>
    /// <param name="pathSelectSaveFolder"></param>
    /// <returns></returns>
    public async Task<(bool found, string backupFolderPath, string originalSaveFolderPath)> CheckBackupSaveAsync(SaveListContext Context)
    {
        //mess = "";
        try
        {
            string rootBackupDir = Path.Combine(Application.persistentDataPath, "User_DataGame", "GetBackUpTray");
            Directory.CreateDirectory(rootBackupDir);
            return await Task.Run(() =>
            {
                var saveContext = RefreshSaveList();

                foreach (var save in saveContext.Saves)
                {
                    // Tên thư mục gốc để đối chiếu
                    string originalFolderName = Path.GetFileName(save.FolderPath);

                    // Kiểm tra toàn bộ thư mục backup
                    var backupDirs = Directory.GetDirectories(rootBackupDir);

                    foreach (var backupPath in backupDirs)
                    {
                        var parsed = ParseBackupFolderName(Path.GetFileName(backupPath));
                        if (parsed.originalName == originalFolderName)
                        {
                            mess = $"Tìm thấy bản backup cho:\n\n - {parsed.originalName} \n- {parsed.backupTimestamp}";
                            Debug.Log($"[CheckBackupSaveAsync] Found backup for: {parsed.originalName} at {backupPath}");
                            return (true, save.FolderPath, backupPath);
                        }
                    }
                }

                mess = "Không tìm thấy bản backup nào khớp với các bản save.";
                return (false, null, null);
            });
        }
        catch (Exception e)
        {
            mess = $"Lỗi khi kiểm tra bản backup: {e.Message}";
            return (false, null, null);
        }
        finally
        {
            //Debug.Log(mess);
            UiPage06_C.Instance.ShowLogMessage(mess);
        }
    }

    public async Task<bool> BackUpSaveItemAsync(string folderPath)
    {
        try
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                mess = "WTF :( Chọn bản save đi ní.";
                return false;
            }

            string rootBackupDir = Path.Combine(Application.persistentDataPath, "User_DataGame", "BackUpTray");
            Directory.CreateDirectory(rootBackupDir);

            // đảm bảo BackUpTray rổng
            if (Directory.GetFiles(rootBackupDir).Length > 0 || Directory.GetDirectories(rootBackupDir).Length > 0)
            {
                await ClearBackupTrayAsync();
            }

            // Tên thư mục backup có timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderName = Path.GetFileName(folderPath);
            string backupTargetPath = Path.Combine(rootBackupDir, $"{folderName}_backup_{timestamp}");

            await Task.Run(() =>
            {
                CopyDirectory(folderPath, backupTargetPath);
                Debug.Log($"-----------");

            });

            await Task.Delay(300);

            // upload file save to cloud
            backendSync.OnUpload();
            mess = $"Item save đã được chuyển vào thư mục Backup: {backupTargetPath}";
            return true;
        }
        catch (Exception e)
        {
            mess = $"Lỗi khi backup: {e.Message}";
            return false;
        }
        finally
        {
#if UNITY_EDITOR
            Debug.Log(mess);
            UiPage06_C.Instance.ShowLogMessage(mess);
#else
            UiPage06_C.Instance.ShowLogMessage(mess);
#endif
        }
    }

    /// <summary>
    /// Sao chép toàn bộ nội dung của thư mục nguồn sang thư mục đích, bao gồm cả các thư mục con và tập tin bên trong.
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destDir"></param>
    private bool CopyDirectory(string sourceDir, string destDir)
    {
        try
        {
            // Validate input paths
            if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(destDir))
            {
                Debug.LogError($"Invalid directory paths: sourceDir='{sourceDir}', destDir='{destDir}'");
                return false;
            }

            // Ensure source directory exists
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"Source directory does not exist: {sourceDir}");
                return false;
            }

            // Create destination directory if it doesn't exist
            Directory.CreateDirectory(destDir);
            Debug.Log($"Ensured destination directory exists: {destDir}");

            // Copy all files in the source directory
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                try
                {
                    string destFile = Path.Combine(destDir, Path.GetFileName(filePath));
                    File.Copy(filePath, destFile, true);
                    Debug.Log($"Copied file: {filePath} to {destFile}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to copy file {filePath} to destFile: {e.Message}");
                }
            }

            // Copy all subdirectories in the source directory
            foreach (string dirPath in Directory.GetDirectories(sourceDir))
            {
                try
                {
                    string destSubDir = Path.Combine(destDir, Path.GetFileName(dirPath));
                    CopyDirectory(dirPath, destSubDir);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to copy directory {dirPath} to destSubDir: {e.Message}");
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in CopyDirectory from {sourceDir} to {destDir}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Xoá toàn bộ nội dung trong thư mục BackUpTray và tạo lại thư mục trống.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ClearBackupTrayAsync()
    {
        string rootBackupDir = Path.Combine(Application.persistentDataPath, "User_DataGame", "BackUpTray");

        if (!Directory.Exists(rootBackupDir))
        {
            Debug.LogWarning("[ClearBackupTrayAsync] Backup tray does not exist.");
            return false;
        }

        try
        {
            await Task.Run(() =>
            {
                // Xoá toàn bộ
                Directory.Delete(rootBackupDir, true);
                // Tạo lại folder trống
                Directory.CreateDirectory(rootBackupDir);
            });

            Debug.Log("[ClearBackupTrayAsync] Backup tray cleared successfully.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ClearBackupTrayAsync] Error clearing backup tray: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Xoá toàn bộ nội dung trong thư mục GetBackUpTray và tạo lại thư mục trống.
    /// </summary>
    /// <returns></returns>
    public bool ClearGetBackupTrayAsync()
    {
        string rootBackupDir = Path.Combine(Application.persistentDataPath, "User_DataGame", "GetBackUpTray");
        if (!Directory.Exists(rootBackupDir))
        {
            Debug.LogWarning("[ClearGetBackupTrayAsync] GetBackup tray does not exist.");
            return false;
        }

        // Xoá toàn bộ
        Directory.Delete(rootBackupDir, true);
        // Tạo lại folder trống
        Directory.CreateDirectory(rootBackupDir);

        mess = "[ClearGetBackupTrayAsync] GetBackup tray cleared successfully.";
        return true;

    }

    /// <summary>
    /// Phân tích tên thư mục backup để lấy tên gốc và timestamp.
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public (string originalName, string backupTimestamp) ParseBackupFolderName(string folderName)
    {
        try
        {
            // Regex: match _backup_yyyyMMdd_HHmmss ở cuối
            var match = Regex.Match(folderName, @"^(.*)(_backup_\d{8}_\d{6})$");

            if (match.Success)
            {
                string originalName = match.Groups[1].Value;
                string backupTimestamp = match.Groups[2].Value;
                return (originalName, backupTimestamp);
            }
            else
            {
                return (folderName, string.Empty);
            }
        }
        catch (Exception e)
        {
            mess = $"[ParseBackupFolderName] Regex error: {e.Message}";
            return (folderName, string.Empty);
        }
        finally
        {
            UiPage06_C.Instance.ShowLogMessage(mess);
        }

        //var result = ParseBackupFolderName(folder);

        //Debug.Log($"Original name: {result.originalName}");       // "MySave"
        //Debug.Log($"Backup timestamp: {result.backupTimestamp}"); // "_backup_20250720_160001"
    }

    #endregion

}
