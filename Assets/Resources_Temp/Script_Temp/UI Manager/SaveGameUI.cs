//using DuckLe;
//using Loc_Backend.Scripts;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class SaveGameUI : MonoBehaviour
//{
//    [SerializeField] private UserAccountManager userAccountManager;
//    [SerializeField] private SaveGameManager saveGameManager;
//    [SerializeField] private PlayTimeManager playTimeManager;
//    [SerializeField] private PlayerCheckPoint playerCheckPoint;
//    [SerializeField] private BackendSync backendSync;
//    [SerializeField] private TMP_Text currentSaveText; // hiển thị file save đang chơi
//    [SerializeField] private GameObject MainUI;
//    [SerializeField] private Button BackMainUI;

//    [Header("UI Elements")]
//    [SerializeField] private GameObject loginPanel;
//    [SerializeField] private TMP_InputField userNameInputField;
//    [SerializeField] private TMP_InputField passwordInputField;
//    [SerializeField] private Button submitUserNameButton;

//    [SerializeField] private Button createAccountButton;
//    [SerializeField] private GameObject createAccountPanel;
//    [SerializeField] private TMP_InputField createUserNameInputField;
//    [SerializeField] private TMP_InputField createPasswordInputField;
//    [SerializeField] private Button createButton;
//    [SerializeField] private Button backButton;
//    [SerializeField] private TMP_Text errorText;

//    [SerializeField] private Button continueButton;
//    [SerializeField] private Button newGameButton;
//    [SerializeField] private Button duplicateButton;
//    [SerializeField] private Button syncButton;
//    [SerializeField] private Button logoutButton;
//    [SerializeField] private Button quitButton;

//    [SerializeField] private Button cloudRegisterButton;
//    [SerializeField] private GameObject cloudRegisterPanel;
//    [SerializeField] private TMP_InputField cloudUserNameInputField;
//    [SerializeField] private TMP_InputField cloudPasswordInputField;
//    [SerializeField] private TMP_InputField cloudEmailInputField;
//    [SerializeField] private Button cloudSubmitButton;
//    [SerializeField] private Button cloudCancelButton;

//    [SerializeField] private GameObject otpPanel;
//    [SerializeField] private TMP_InputField otpInputField;
//    [SerializeField] private Button otpSubmitButton;

//    [SerializeField] private GameObject saveListPanel;
//    [SerializeField] private GameObject saveItemTemplate;
//    [SerializeField] private GameObject syncPanelStatus;
//    [SerializeField] private TMP_Text syncSaveNameText;
//    [SerializeField] private TMP_Text syncStatusText;

//    [SerializeField] private TMP_Text playTimeText;
//    private string lastSelectedSaveFolder;
//    private List<GameObject> saveItemInstances = new List<GameObject>();
//    private string selectedSaveFolder;
//    private float lastNewGameTime;
//    private const float NEW_GAME_COOLDOWN = 1f;
//    private float lastPlayTimeUpdate;
//    private const float PLAY_TIME_UPDATE_INTERVAL = 1f;

//    private async void Start()
//    {
//        if (!ValidateReferences()) return;

//        InitializeUI();
//        SetupButtonListeners();
//        RegisterSaveables();
//        await CheckUserAccountsAsync();
//        UpdateCurrentSaveTextAsync(); // Cập nhật thông tin file save khi khởi động
//    }
//    private void Update()
//    {
//        if (playTimeManager.isCounting && playTimeText != null && Time.time - lastPlayTimeUpdate >= PLAY_TIME_UPDATE_INTERVAL)
//        {
//            playTimeText.text = $"Play Time: {playTimeManager.FormatPlayTime()}";
//            lastPlayTimeUpdate = Time.time;
//        }
//    }

//    private bool ValidateReferences()
//    {
//        if (userAccountManager == null || saveGameManager == null || playTimeManager == null || backendSync == null)
//        {
//            Debug.LogError("One or more managers are not assigned!");
//            return false;
//        }
//        if (playTimeText == null)
//        {
//            Debug.LogWarning("PlayTimeText is not assigned in Inspector!");
//        }
//        return true;
//    }

//    private void InitializeUI()
//    {
//        loginPanel.SetActive(false);
//        createAccountPanel.SetActive(false);
//        cloudRegisterPanel.SetActive(false);
//        otpPanel.SetActive(false);
//        syncPanelStatus.SetActive(false);
//        continueButton.interactable = false;
//        newGameButton.interactable = false;
//        duplicateButton.interactable = false;
//        syncButton.interactable = false;
//        logoutButton.interactable = false;
//        cloudRegisterButton.interactable = false;
//    }

//    private void SetupButtonListeners()
//    {
//        submitUserNameButton.onClick.AddListener(() => OnSubmitUserNameAsync());
//        createAccountButton.onClick.AddListener(OnCreateAccountButtonClicked);
//        createButton.onClick.AddListener(OnCreateButtonClicked);
//        backButton.onClick.AddListener(OnBackButtonClicked);
//        continueButton.onClick.AddListener(() => OnContinueButtonClickedAsync());
//        newGameButton.onClick.AddListener(() => OnNewGameButtonClickedAsync().ConfigureAwait(false));
//        duplicateButton.onClick.AddListener(() => OnDuplicateButtonClickedAsync());
//        syncButton.onClick.AddListener(() => OnSyncButtonClickedAsync());
//        logoutButton.onClick.AddListener(() => OnLogoutButtonClickedAsync());
//        cloudRegisterButton.onClick.AddListener(OnCloudRegisterButtonClicked);
//        cloudSubmitButton.onClick.AddListener(OnCloudSubmitButtonClicked);
//        otpSubmitButton.onClick.AddListener(OnOtpSubmitButtonClicked);
//        cloudCancelButton.onClick.AddListener(OnCloudCancelButtonClicked);
//        quitButton.onClick.AddListener(OnQuitButtonClicked);
//        BackMainUI.onClick.AddListener(() =>
//        {
//            OnBackMenuUIClicked();
//        });
//    }

//    private async void OnBackMenuUIClicked()
//    {
//        string userName = userAccountManager.CurrentUserBaseName;
//        if (string.IsNullOrEmpty(userName))
//        {
//            Debug.LogError("[SaveGameUI] No user logged in, cannot save position!");
//            await SceneController.Instance.UnloadAllAdditiveScenesAsync();
//            MainUI.SetActive(true);
//            return;
//        }

//        // Lưu tọa độ hiện tại trước khi đặt lại vị trí
//        if (playerCheckPoint != null)
//        {
//            Vector3 currentPosition = PlayerController.Instance.transform.position;
//            PlayerCheckPointData lastData = playerCheckPoint.GetLastLoadedData();
//            if (lastData == null || lastData.position.ToVector3() != currentPosition)
//            {
//                await saveGameManager.SaveAllAsync(userName);
//                Debug.Log($"[SaveGameUI] Saved player position: {currentPosition}");
//            }
//            else
//            {
//                Debug.Log("[SaveGameUI] Position unchanged, skipping save.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("[SaveGameUI] PlayerCheckPoint or PlayerTransform is null, skipping save.");
//        }

//        // Đặt lại vị trí player trước khi unload
//        bool resetSuccess = false;
//        if (playerCheckPoint != null)
//        {
//            //resetSuccess = playerCheckPoint.ResetPlayerPositionWord();
//        }
//        else
//        {
//            Debug.LogError("[SaveGameUI] PlayerCheckPoint is null, cannot reset player position!");
//        }

//        // Chỉ unload nếu đặt lại vị trí thành công
//        if (resetSuccess)
//        {
//            await SceneController.Instance.UnloadAllAdditiveScenesAsync();
//            MainUI.SetActive(true);
//            Debug.Log("[SaveGameUI] Unloaded additive scenes after successful position reset.");
//        }
//        else
//        {
//            Debug.LogWarning("[SaveGameUI] Position reset failed, skipping unload.");
//        }

        
//    }

//    /// <summary>
//    /// Đăng ký các module ISaveable với SaveGameManager.
//    /// </summary>
//    private void RegisterSaveables()
//    {
//        saveGameManager.RegisterSaveable(playTimeManager);
//        saveGameManager.RegisterSaveable(playerCheckPoint);
//        // Thêm các module ISaveable khác ở đây nếu cần
//    }

//    /// <summary>
//    /// Kiểm tra xem có tài khoản người dùng nào đã được tạo hay không.
//    /// </summary>
//    /// <returns></returns>
//    private async Task CheckUserAccountsAsync()
//    {
//        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
//        if (!File.Exists(userAccountsPath) ||
//            JsonUtility.FromJson<UserAccountData>(await File.ReadAllTextAsync(userAccountsPath)).Users.Count == 0)
//        {
//            loginPanel.SetActive(true);
//            Debug.Log("No users found. Showing login panel.");
//        }
//        else
//        {
//            await TryAutoLoginAsync();
//        }
//    }

//    private async Task TryAutoLoginAsync()
//    {
//        if (userAccountManager.TryAutoLogin(out string errorMessage))
//        {
//            loginPanel.SetActive(false);
//            newGameButton.interactable = true;
//            duplicateButton.interactable = true;
//            syncButton.interactable = true;
//            logoutButton.interactable = true;
//            cloudRegisterButton.interactable = true;
//            errorText.text = "";
//            await RefreshSaveListAsync();

//            var saves = await saveGameManager.GetAllSaveFoldersAsync(userAccountManager.CurrentUserBaseName);
//            continueButton.interactable = saves.Count > 0;

//            lastSelectedSaveFolder = await GetValidLastSaveFolderAsync();
//            if (lastSelectedSaveFolder != null)
//            {
//                continueButton.interactable = true;
//            }
//            UpdateCurrentSaveTextAsync(); // Cập nhật UI
//            playTimeManager.StartCounting();
//            Debug.Log($"Auto-logged in user: {userAccountManager.CurrentUserBaseName}, Save: {lastSelectedSaveFolder}");
//        }
//        else
//        {
//            Debug.LogWarning($"Auto-login failed: {errorMessage}");
//            loginPanel.SetActive(true);
//        }
//    }

//    private async void OnSubmitUserNameAsync()
//    {
//        string userName = userNameInputField.text;
//        string password = passwordInputField.text;

//        if (userAccountManager.Login(userName, password, out string errorMessage))
//        {
//            loginPanel.SetActive(false);
//            newGameButton.interactable = true;
//            duplicateButton.interactable = true;
//            syncButton.interactable = true;
//            logoutButton.interactable = true;
//            cloudRegisterButton.interactable = true;
//            errorText.text = "";
//            await RefreshSaveListAsync();

//            lastSelectedSaveFolder = await GetValidLastSaveFolderAsync();
//            var saves = await saveGameManager.GetAllSaveFoldersAsync(userName);
//            continueButton.interactable = saves.Count > 0;

//            playTimeManager.StartCounting();
//            Debug.Log($"Logged in user: {userName}, Save: {lastSelectedSaveFolder}");
//        }
//        else
//        {
//            errorText.text = errorMessage;
//            errorText.color = Color.red;
//            Debug.LogWarning($"Login failed: {errorMessage}");
//        }
//    }

//    private void OnCreateAccountButtonClicked()
//    {
//        loginPanel.SetActive(false);
//        createAccountPanel.SetActive(true);
//        errorText.text = "";
//        createUserNameInputField.text = "";
//        createPasswordInputField.text = "";
//    }

//    private void OnCreateButtonClicked()
//    {
//        string userName = createUserNameInputField.text;
//        string password = createPasswordInputField.text;

//        if (userAccountManager.CreateAccount(userName, password, out string errorMessage))
//        {
//            createAccountPanel.SetActive(false);
//            loginPanel.SetActive(true);
//            errorText.text = "Account created successfully! Please log in.";
//            errorText.color = Color.green;
//            userNameInputField.text = userName;
//            passwordInputField.text = "";
//            Debug.Log($"Created account: {userName}");
//        }
//        else
//        {
//            errorText.text = errorMessage;
//            errorText.color = Color.red;
//            Debug.LogWarning($"Create account failed: {errorMessage}");
//        }
//    }

//    private void OnBackButtonClicked()
//    {
//        createAccountPanel.SetActive(false);
//        cloudRegisterPanel.SetActive(false);
//        otpPanel.SetActive(false);
//        loginPanel.SetActive(true);
//        errorText.text = "";
//    }

//    private void OnCloudRegisterButtonClicked()
//    {
//        loginPanel.SetActive(false);
//        cloudRegisterPanel.SetActive(true);
//        errorText.text = "";
//        cloudUserNameInputField.text = userAccountManager.CurrentUserBaseName;
//        cloudPasswordInputField.text = "";
//        cloudEmailInputField.text = "";
//    }

//    private void OnCloudSubmitButtonClicked()
//    {
//        string userName = cloudUserNameInputField.text;
//        string password = cloudPasswordInputField.text;
//        string email = cloudEmailInputField.text;

//        if (!userAccountManager.Login(userName, password, out string errorMessage))
//        {
//            errorText.text = errorMessage;
//            errorText.color = Color.red;
//            Debug.LogWarning($"Cloud register login check failed: {errorMessage}");
//            return;
//        }

//        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
//        {
//            errorText.text = "Please enter a valid email!";
//            errorText.color = Color.red;
//            Debug.LogWarning("Invalid email for cloud register");
//            return;
//        }

//        StartCoroutine(backendSync.RequestCloudRegister(userName, password, email, (success, message) =>
//        {
//            if (success)
//            {
//                cloudRegisterPanel.SetActive(false);
//                otpPanel.SetActive(true);
//                errorText.text = "OTP sent to your email. Please enter it.";
//                errorText.color = Color.green;
//                Debug.Log("Cloud register OTP sent");
//            }
//            else
//            {
//                errorText.text = message;
//                errorText.color = Color.red;
//                Debug.LogWarning($"Cloud register failed: {message}");
//            }
//        }));
//    }

//    private void OnCloudCancelButtonClicked()
//    {
//        cloudRegisterPanel.SetActive(false);
//        loginPanel.SetActive(true);
//        errorText.text = "";
//        Debug.Log("Cloud registration cancelled");
//    }

//    private void OnOtpSubmitButtonClicked()
//    {
//        string otp = otpInputField.text;
//        string userName = cloudUserNameInputField.text;

//        if (string.IsNullOrEmpty(otp) || otp.Length != 6)
//        {
//            errorText.text = "Please enter a valid 6-digit OTP!";
//            errorText.color = Color.red;
//            Debug.LogWarning("Invalid OTP entered");
//            return;
//        }

//        StartCoroutine(backendSync.VerifyOtp(userName, otp, (success, message) =>
//        {
//            if (success)
//            {
//                otpPanel.SetActive(false);
//                loginPanel.SetActive(true);
//                errorText.text = "Cloud registration successful!";
//                errorText.color = Color.green;
//                Debug.Log("Cloud registration verified");
//            }
//            else
//            {
//                errorText.text = message;
//                errorText.color = Color.red;
//                Debug.LogWarning($"OTP verification failed: {message}");
//            }
//        }));
//    }

//    private async void OnContinueButtonClickedAsync()
//    {
//        string userName = userAccountManager.CurrentUserBaseName;
//        if (string.IsNullOrEmpty(userName))
//        {
//            Debug.LogError("No user logged in!");
//            errorText.text = "Please log in first!";
//            errorText.color = Color.red;
//            return;
//        }

//        string saveFolder = lastSelectedSaveFolder ?? await saveGameManager.GetLatestSaveFolderAsync(userName);
//        if (saveFolder != null)
//        {
//            await saveGameManager.LoadLatestAsync(userName);
//            lastSelectedSaveFolder = saveFolder; // Cập nhật thư mục save đang chơi
//            UpdateCurrentSaveTextAsync(); // Cập nhật UI
//            if (playerCheckPoint != null)
//            {
//                string sceneToLoad = playerCheckPoint.CurrentMap;
//                Debug.Log($"[SaveGameUI] Loading additive scene from checkpoint: {sceneToLoad}");
//                await SceneController.Instance.LoadAdditiveSceneAsync(sceneToLoad, playerCheckPoint);
//            }
//            MainUI.SetActive(false); // Ẩn UI chính khi tiếp tục chơi
//        }
//        else
//        {
//            Debug.LogWarning("No save found! Starting new game.");
//            errorText.text = "No save found! Starting new game.";
//            errorText.color = Color.yellow;
//            await OnNewGameButtonClickedAsync();
//        }
//    }

//    private async Task OnNewGameButtonClickedAsync()
//    {
//        //if (Time.time - lastNewGameTime < NEW_GAME_COOLDOWN) return;
//        //lastNewGameTime = Time.time;

//        if (!newGameButton.interactable) return;
//        newGameButton.interactable = false;

//        string userName = userAccountManager.CurrentUserBaseName;
//        if (string.IsNullOrEmpty(userName))
//        {
//            Debug.LogError("No user logged in!");
//            errorText.text = "Please log in first!";
//            errorText.color = Color.red;
//            newGameButton.interactable = true;
//            return;
//        }

//        try
//        {
//            await saveGameManager.SaveAllAsync(userName);
//            lastSelectedSaveFolder = await saveGameManager.GetLatestSaveFolderAsync(userName);
//            await RefreshSaveListAsync();
//            continueButton.interactable = true;
//            Debug.Log($"Started new game with save: {lastSelectedSaveFolder}");
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"Failed to create new game: {e.Message}");
//            errorText.text = "Failed to create new game!";
//            errorText.color = Color.red;
//        }
//        finally
//        {
//            newGameButton.interactable = true;
//        }
//    }

//    private async void OnDuplicateButtonClickedAsync()
//    {
//        if (string.IsNullOrEmpty(selectedSaveFolder))
//        {
//            Debug.LogWarning("No save selected to duplicate!");
//            errorText.text = "Please select a save to duplicate!";
//            errorText.color = Color.red;
//            return;
//        }

//        string userName = userAccountManager.CurrentUserBaseName;
//        string newSaveFolder = await saveGameManager.DuplicateSaveFolderAsync(selectedSaveFolder, userName);
//        if (!string.IsNullOrEmpty(newSaveFolder))
//        {
//            lastSelectedSaveFolder = newSaveFolder;
//            await RefreshSaveListAsync();
//            continueButton.interactable = true;
//            Debug.Log($"Duplicated save to: {newSaveFolder}");
//        }
//        else
//        {
//            errorText.text = "Failed to duplicate save!";
//            errorText.color = Color.red;
//        }
//    }

//    private async void OnSyncButtonClickedAsync()
//    {
//        if (string.IsNullOrEmpty(selectedSaveFolder))
//        {
//            Debug.LogWarning("No save selected to sync!");
//            errorText.text = "Please select a save to sync!";
//            errorText.color = Color.red;
//            return;
//        }

//        await SyncSaveAsync(selectedSaveFolder);
//    }

//    private async Task SyncSaveAsync(string folderPath)
//    {
//        syncPanelStatus.SetActive(true);
//        syncSaveNameText.text = $"Save: {Path.GetFileName(folderPath)}";
//        syncStatusText.text = "Syncing...";
//        syncStatusText.color = Color.yellow;

//        await Task.Delay(500);

//        var (success, errorMessage) = await saveGameManager.SyncFileSaveAsync(folderPath);
//        if (success)
//        {
//            syncStatusText.text = "Sync successful!";
//            syncStatusText.color = Color.green;
//            await Task.Delay(1000);
//            syncPanelStatus.SetActive(false);
//            Debug.Log($"Synced save: {folderPath}");
//        }
//        else
//        {
//            syncStatusText.text = $"Error: {errorMessage}";
//            syncStatusText.color = Color.red;
//            await Task.Delay(2000);
//            syncPanelStatus.SetActive(false);
//            errorText.text = errorMessage;
//            errorText.color = Color.red;
//            Debug.LogWarning($"Sync failed: {errorMessage}");
//        }
//    }

//    /// <summary>
//    /// Cập nhật văn bản hiển thị file save hiện tại.
//    /// </summary>
//    private void UpdateCurrentSaveTextAsync()
//    {
//        if (currentSaveText == null) return;

//        if (string.IsNullOrEmpty(lastSelectedSaveFolder))
//        {
//            currentSaveText.text = "Current Save: None";
//        }
//        else
//        {
//            string saveName = Path.GetFileName(lastSelectedSaveFolder);
//            currentSaveText.text = $"Current Save: {saveName}";
//        }
//    }

//    private async void OnLogoutButtonClickedAsync()
//    {
//        //await SaveSessionDataAsync();
//        playTimeManager.StopCounting();
//        playTimeManager.ResetSession();
//        await SceneController.Instance.UnloadAllAdditiveScenesAsync(); // Unload các scene additive
//        loginPanel.SetActive(true);
//        createAccountPanel.SetActive(false);
//        cloudRegisterPanel.SetActive(false);
//        otpPanel.SetActive(false);
//        syncPanelStatus.SetActive(false);
//        continueButton.interactable = false;
//        newGameButton.interactable = false;
//        duplicateButton.interactable = false;
//        syncButton.interactable = false;
//        logoutButton.interactable = false;
//        cloudRegisterButton.interactable = false;
//        selectedSaveFolder = null;
//        lastSelectedSaveFolder = null;
//        lastSelectedSaveFolder = null; // Xóa thư mục save đang chơi
//        UpdateCurrentSaveTextAsync(); // Cập nhật UI
//        await RefreshSaveListAsync();
//        errorText.text = "";
//        userNameInputField.text = "";
//        passwordInputField.text = "";
//        if (playTimeText != null)
//        {
//            playTimeText.text = "Play Time: 0 00:00:00";
//        }
//        Debug.Log("Logged out and returned to login screen.");
//    }

//    private void OnQuitButtonClicked()
//    {
//        playTimeManager.StopCounting();
//        playTimeManager.ResetSession();
//        OnLogoutButtonClickedAsync();

//        Debug.Log("Quitting game...");
//        Application.Quit();
//    }

//    private async Task<string> GetValidLastSaveFolderAsync()
//    {
//        string lastFileSave = userAccountManager.GetLastFileSave();
//        if (!string.IsNullOrEmpty(lastFileSave))
//        {
//            string fileSavePath = Path.Combine(Application.persistentDataPath, "User_DataGame",
//                $"FileSave_{userAccountManager.CurrentUserBaseName}", lastFileSave);
//            if (Directory.Exists(fileSavePath) && Directory.GetFiles(fileSavePath, "*.json").Length > 0)
//            {
//                return fileSavePath;
//            }
//        }

//        return await saveGameManager.GetLatestSaveFolderAsync(userAccountManager.CurrentUserBaseName);
//    }

//    private async Task SaveSessionDataAsync()
//    {
//        string userName = userAccountManager.CurrentUserBaseName;
//        if (string.IsNullOrEmpty(userName))
//        {
//            Debug.LogWarning("No user logged in. Skipping session save.");
//            return;
//        }

//        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//        if (currentScene == "Menu")
//        {
//            Debug.Log("[SaveGameUI] Skipped saving PlayerCheckPoint in Menu scene.");
//            // Save các dữ liệu khác như playTimeManager thôi
//            //await saveGameManager.SaveOnlyAsync(userName, excludeType: typeof(PlayerCheckPoint));
//        }
//        else
//        {
//            Debug.Log("[SaveGameUI] Saving PlayerCheckPoint and others...");
//            await saveGameManager.SaveAllAsync(userName);
//        }

//        string saveFolder = lastSelectedSaveFolder ?? await saveGameManager.GetLatestSaveFolderAsync(userName);
//        string lastFileSave = saveFolder != null ? Path.GetFileName(saveFolder) : "";
//        userAccountManager.UpdateLastSession(lastFileSave, playTimeManager.SessionPlayTime);
//    }

//    private async Task RefreshSaveListAsync()
//    {
//        foreach (var item in saveItemInstances)
//        {
//            Destroy(item);
//        }
//        saveItemInstances.Clear();

//        string userName = userAccountManager.CurrentUserBaseName;
//        if (string.IsNullOrEmpty(userName))
//        {
//            Debug.LogWarning("No user logged in. Cannot refresh save list.");
//            return;
//        }

//        var saves = await saveGameManager.GetAllSaveFoldersAsync(userName);
//        foreach (var save in saves)
//        {
//            GameObject saveItem = Instantiate(saveItemTemplate, saveListPanel.transform);
//            saveItemInstances.Add(saveItem);

//            var saveNameText = saveItem.GetComponentInChildren<TMP_Text>();
//            var saveImage = saveItem.GetComponentInChildren<RawImage>();
//            var buttons = saveItem.GetComponentsInChildren<Button>();
//            var selectButton = buttons[0];
//            var deleteButton = buttons[1];

//            saveNameText.text = Path.GetFileName(save.FolderPath);
//            if (!string.IsNullOrEmpty(save.ImagePath))
//            {
//                StartCoroutine(LoadImageAsync(save.ImagePath, saveImage));
//            }
//            else
//            {
//                saveImage.gameObject.SetActive(false);
//            }

//            selectButton.onClick.AddListener(() => OnSelectSaveAsync(save.FolderPath));
//            deleteButton.onClick.AddListener(() => OnDeleteSaveAsync(save.FolderPath));
//        }

//        Debug.Log($"Refreshed save list for user: {userName}, Found {saves.Count} saves");
//    }

//    private IEnumerator LoadImageAsync(string imagePath, RawImage saveImage)
//    {
//        byte[] imageBytes = File.ReadAllBytes(imagePath);
//        Texture2D texture = new Texture2D(2, 2);
//        texture.LoadImage(imageBytes);
//        saveImage.texture = texture;
//        yield return null;
//    }

//    private async void OnSelectSaveAsync(string folderPath)
//    {
//        selectedSaveFolder = folderPath;
//        lastSelectedSaveFolder = folderPath;
//        continueButton.interactable = true;
//        await saveGameManager.LoadLatestAsync(userAccountManager.CurrentUserBaseName);
//        Debug.Log($"Selected save: {folderPath}");
//    }

//    private async void OnDeleteSaveAsync(string folderPath)
//    {
//        if (await saveGameManager.DeleteSaveFolderAsync(folderPath))
//        {
//            if (folderPath == selectedSaveFolder)
//            {
//                selectedSaveFolder = null;
//                lastSelectedSaveFolder = null;
//            }
//            await RefreshSaveListAsync();
//            var saves = await saveGameManager.GetAllSaveFoldersAsync(userAccountManager.CurrentUserBaseName);
//            continueButton.interactable = saves.Count > 0;
//            Debug.Log($"Deleted save: {folderPath}");
//        }
//        else
//        {
//            errorText.text = "Failed to delete save!";
//            errorText.color = Color.red;
//        }
//    }
//}