
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Code.Procession;
using Loc_Backend.Scripts;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TMPro;

/// <summary>
/// Điều phối các nghiệp vụ chuyên môn.
/// Đăng kí Logic nghiệp vụ cho các Event ở đây, có thể gọi trigger các sự kiện từ đây (cẩn thận tránh lặp vô hạn).
/// * KHÔNG ĐƯỢC ĐĂNG KÍ HOẶC CHỨA LOGIC CHANGECORESTATE Ở ĐÂY.
/// </summary>
public class ProfessionalSkilMenu : CoreEventListenerBase
{
    public static ProfessionalSkilMenu Instance { get; private set; }

    private Core _core;
    public BackendSync backendSync;

    private string lastSelectedSaveFolder;
    public string selectedSaveFolder;
    public string SelectedSaveImagePath;

    [Header("BackUp")]
    public string CurrentbackupSavePath;
    public string CurrentOriginalSavePath;
    public bool CurrentbackupOke;

    public string SceneDefault = "Phong_scene";

    private string mess = null;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }

        _core = Core.Instance;
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

        e.OnLogin += () => Login();
        e.OnRegister += () => Register();
        e.OnLogout += async () => await Logout();
        e.OnConnectToServer += () => ConnectToServer();
        e.OnConnectingToServer += () => OnOtp();
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.OnNewSession -= () => OnNewGame();
        e.OnContinueSession -= () => OnContinueGame(selectedSaveFolder);
        e.OnSaveSession -= () => OnSaveSession(lastSelectedSaveFolder);
        e.OnQuitSession -= async () => await OnQuitSession(lastSelectedSaveFolder);

        e.OnSelectSaveItem -= (path) => OnSelectSave(path);
        e.OnSyncFileSave -= async (path) => await BackUpSaveItemAsync(path);

        e.OnLogin -= () => Login();
        e.OnRegister -= () => Register();
        e.OnLogout -= async () => await Logout();
        e.OnConnectToServer -= () => ConnectToServer();
        e.OnConnectingToServer -= () => OnOtp();
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
            JsonUtility.FromJson<UserAccountData>(File.ReadAllText(userAccountsPath)).Users.Count == 0)
        {
            lastSelectedSaveFolder = null;
            // set trạng thái không có tài khoản hiện tại
            _core._accountStateMachine.SetState(new NoCurrentAccountState(_core._accountStateMachine, _coreEvent));
            Debug.Log("[Test] No users found. Showing login panel.");
        }
        else
        {
            TryAutoLogin();
        }
    }

    /// <summary>
    /// Thử tự động đăng nhập người dùng nếu có thông tin đăng nhập hợp lệ.
    /// </summary>
    private void TryAutoLogin()
    {
        if (_core._userAccountManager.TryAutoLogin(out string errorMessage))
        {
            UiPage06_C.Instance.ShowLogMessage(errorMessage);
            lastSelectedSaveFolder = GetValidLastSaveFolder();
            PlayTimeManager.Instance.StartCounting();

            if (!_core._userAccountManager.IsSynced(_core._userAccountManager.currentUserBaseName))
            {
                _core._accountStateMachine.SetState(new NoConnectToServerState(_core._accountStateMachine, _coreEvent));
            }
            else
            {
                _core._accountStateMachine.SetState(new HaveConnectToServer(_core._accountStateMachine, _coreEvent));
            }

            UiPage06_C.Instance.ActiveObj(true, false, false);

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
            StartCoroutine(RetryRefreshSaveListAfterDelay());
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

        PlayerCheckPoint.Instance.ApplyLoadedPosition();
    }
    #endregion

    #region nghiep vụ 2

    public string OnNewGame()
    {

        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            Debug.LogError($"currentUserBaseName == {UserAccountManager.Instance.currentUserBaseName}");
        }

        string newSaveFolder = SaveGameManager.Instance.CreateNewSaveFolder(UserAccountManager.Instance.currentUserBaseName);

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

        // Load scene và chờ callback khi load xong
        SceneController.Instance.LoadAdditiveScene(SceneDefault, PlayerCheckPoint.Instance, () =>
        {
            //Đảm bảo Player đã tồn tại sau khi load scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[OnNewGame] Player not found after loading scene.");
                return;
            }


            // Gọi Procession để load dữ lieu tu GameProcession
            ProgressionManager.Instance.InitProgression();
            PlayerCheckPoint.Instance.AssignCameraFromCore();
            PlayerCheckPoint.Instance.SetPlayerTransform(player.transform);
            //Đặt vị trí mặc định
            PlayerCheckPoint.Instance.ResetPlayerPositionWord();
            SaveGameManager.Instance.SaveToFolder(newSaveFolder);
        });



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

        SceneController.Instance.LoadAdditiveScene(sceneToLoad, PlayerCheckPoint.Instance, () =>
        {
            PlayTimeManager.Instance.StartCounting();
            MapStateSave.Instance.ApplyMapState();
            PlayerCheckPoint.Instance.StartCoroutine(WaitUntilPlayerAndApply());
            PlayerCheckPoint.Instance.AssignCameraFromCore();

            // Đồng bộ hóa dữ liệu vật thể
            ProgressionManager.Instance.SyncPuzzleStatesWithProgression();
        });
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
                            PlayerCheckPoint.Instance.ResetPlayerPositionWord();
                        });

                        //// Gọi lại load ảnh tại đây – sau khi ảnh mới đã được chụp xong
                        //string imagePath = Path.Combine(currentSaveFolder, "screenshot.png");
                        //SelectedSaveImagePath = File.Exists(imagePath) ? imagePath : null;
                        //ScreenshotDisplayer.Instance.LoadScreenshotToPlane(SelectedSaveImagePath);
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

    #endregion

    #region Nghiệp vụ 3

    private void Register()
    {
        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.Register);

        if (inputs.Length < 2)
        {
            //Debug.LogError("Không đủ input field cho đăng ký (cần ít nhất 2).");
            UiPage06_C.Instance.ShowLogMessage("Không đủ input field cho đăng ký (cần ít nhất 2).");
            return;
        }

        string userName = inputs[0].text;
        string passWord = inputs[1].text;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passWord))
        {
            //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu trống.");
            UiPage06_C.Instance.ShowLogMessage("du ma... đừng để trống name với pass ní.");
            return;
        }
        _core.RegisterAccount(userName, passWord);

        foreach (var input in inputs)
        {
            input.text = string.Empty;
        }

    }

    private void Login()
    {
        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.Login);

        if (inputs.Length < 2)
        {
            //Debug.LogError("Không đủ input field cho login (cần ít nhất 2).");
            UiPage06_C.Instance.ShowLogMessage("Không đủ input field cho login (cần ít nhất 2).");
            return;
        }

        string userName = inputs[0].text;
        string passWord = inputs[1].text;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passWord))
        {
            //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu trống.");
            UiPage06_C.Instance.ShowLogMessage("du ma... đừng để trống name với pass ní..");
            return;
        }

        if (_core.LogoutAccount())
        {
            _core.LoginAccount(userName, passWord);
        }

        foreach (var input in inputs)
        {
            input.text = string.Empty;
        }
    }

    private async Task Logout()
    {
        _core.LogoutAccount();
        lastSelectedSaveFolder = null;
        selectedSaveFolder = null;
        await UIPage05.Instance.RefreshSaveSlots();
    }

    private void ConnectToServer()
    {
        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.ConnectToServer);

        if (inputs.Length < 3)
        {
            //Debug.LogError("Không đủ input field cho login (cần ít nhất 3).");
            UiPage06_C.Instance.ShowLogMessage("Không đủ input field cho đăng ký (cần ít nhất 3).");
            return;
        }

        string userName = inputs[0].text;
        string passWord = inputs[1].text;
        string email = inputs[2].text;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passWord) || string.IsNullOrWhiteSpace(email))
        {
            //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu hoặc email trống.");
            UiPage06_C.Instance.ShowLogMessage("du ma... đừng để trống name với pass hoặc email ní..");
            return;
        }

        _core.SyncToServer(userName, passWord, email);

        foreach (var input in inputs)
        {
            input.text = string.Empty;
        }
    }

    private void OnOtp()
    {
        string userName = _core._userAccountManager.currentUserBaseName;

        TMP_InputField[] inputs = UiPage06_C.Instance.GetInputFieldsByAction(UIActionType.ConnectingToServer);

        if (inputs.Length < 1)
        {
            Debug.LogError("Không đủ input field cho login (cần ít nhất 2).");
            return;
        }

        string otp = inputs[0].text;

        if (string.IsNullOrWhiteSpace(otp))
        {
            //Debug.LogWarning("Tên đăng nhập hoặc mật khẩu trống.");
            UiPage06_C.Instance.ShowLogMessage("vl... OTP để trống kìa.");
            return;
        }

        _core.VerifyOTPAccount(otp, userName);

        foreach (var input in inputs)
        {
            input.text = string.Empty;
        }
    }

    #endregion

    /// <summary>
    /// ghi đè file save gốc bằng file backup với path được trả về bởi CheckBackupSaveAsync
    /// </summary>
    /// <param name="pathSelectSaveFolder"></param>
    /// <returns></returns>
    public async Task<bool> SetBackUpSaveItemAsync()
    {
        try
        {
            bool found = CurrentbackupOke;
            string backupPath = CurrentbackupSavePath;
            string originalPath = CurrentOriginalSavePath;

            return await Task.Run(() =>
            {
                if (!found || string.IsNullOrEmpty(backupPath) || string.IsNullOrEmpty(originalPath))
                    return false;

                if (Directory.Exists(originalPath))
                    Directory.Delete(originalPath, true); // Xóa bản cũ

                CopyDirectory(backupPath, originalPath); // Sao chép bản backup

                mess = "Đã khôi phục thành công bản backup.";
                return true;
            });
        }
        catch (Exception e)
        {
            mess = $"Lỗi khi khôi phục backup: {e.Message}";
            return false;
        }
        finally
        {
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
        mess = "";
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
                            return (true, save.FolderPath, backupPath);
                        }
                    }
                }

                //mess = "Không tìm thấy bản backup nào khớp với các bản save.";
                return (false, null, null);
            });
        }
        catch (Exception e)
        {
            //mess = $"Lỗi khi kiểm tra bản backup: {e.Message}";
            return (false, null, null);
        }
        finally
        {
            Debug.Log(mess);
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
            });

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
    private void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(filePath));
            File.Copy(filePath, destFile, true);
        }

        foreach (string dirPath in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(dirPath));
            CopyDirectory(dirPath, destSubDir);
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


}
