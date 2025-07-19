
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Code.Procession;

/// <summary>
/// Điều phối các nghiệp vụ chuyên môn.
/// Đăng kí Logic nghiệp vụ cho các Event ở đây, có thể gọi trigger các sự kiện từ đây (cẩn thận tránh lặp vô hạn).
/// * KHÔNG ĐƯỢC ĐĂNG KÍ HOẶC CHỨA LOGIC CHANGECORESTATE Ở ĐÂY.
/// </summary>
public class ProfessionalSkilMenu : CoreEventListenerBase
{
    public static ProfessionalSkilMenu Instance { get; private set; }

    /// <summary>
    /// hiện dùng để test lưu trữ thư mục save gần nhất đã chọn
    /// </summary>

    private Core _core;
    private string lastSelectedSaveFolder;  
    public string selectedSaveFolder;
    public string SelectedSaveImagePath;

    public string SceneDefault = "Phong_scene";

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
        e.OnQuitSession += () => OnQuitSession(lastSelectedSaveFolder);

        e.OnSelectSaveItem += (path) => OnSelectSave(path);
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.OnNewSession -= () => OnNewGame();
        e.OnContinueSession -= () => OnContinueGame(selectedSaveFolder);
        e.OnSaveSession -= () => OnSaveSession(lastSelectedSaveFolder);
        e.OnQuitSession -= () => OnQuitSession(lastSelectedSaveFolder);

        e.OnSelectSaveItem -= (path) => OnSelectSave(path);
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
            //loginPanel.SetActive(true);
            lastSelectedSaveFolder = null;
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
        if (UserAccountManager.Instance.TryAutoLogin(out string errorMessage))
        {

            lastSelectedSaveFolder = GetValidLastSaveFolder();
            //ContinueGame_Bt.interactable = !string.IsNullOrEmpty(lastSelectedSaveFolder);
            //UpdateCurrentSaveText();
            PlayTimeManager.Instance.StartCounting();
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
            Saves = saves.Select(s => new SaveFolder { FolderPath = s.FolderPath, ImagePath = s.ImagePath }).ToList(),
            IsContinueEnabled = isContinueEnabled
        };
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

    //private void UpdateCurrentSaveText()
    //{
    //    if (currentSaveText == null) return;
    //    currentSaveText.text = string.IsNullOrEmpty(lastSelectedSaveFolder) ? "Current Save: None" : $"Current Save: {Path.GetFileName(lastSelectedSaveFolder)}";
    //}
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

        string imagePath = Path.Combine(folderPath, "screenshot.png");
        SelectedSaveImagePath = File.Exists(imagePath) ? imagePath : null; // lấy thêm đường dẫn ảnh nếu có

        ScreenshotDisplayer.Instance.LoadScreenshotToPlane(SelectedSaveImagePath);
    }

    /// <summary>
    /// Xử lý khi người dùng nhấn xóa một thư mục lưu trữ.
    /// </summary>
    /// <param name="folderPath"></param>
    public void OnDeleteSave(string folderPath)
    {
        if (SaveGameManager.Instance.DeleteSaveFolder(folderPath))
        {
            if (folderPath == lastSelectedSaveFolder)
            {
                lastSelectedSaveFolder = null;
                selectedSaveFolder = null;
                UIPage05.Instance.RefreshSaveSlots();
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

    public void OnQuitSession(string currentSaveFolder)
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

        UIPage05.Instance.RefreshSaveSlots();
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
}
