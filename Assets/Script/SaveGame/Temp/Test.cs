using Loc_Backend.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private UserAccountManager userAccountManager;
    [SerializeField] private SaveGameManager saveGameManager;
    [SerializeField] private PlayTimeManager playTimeManager;
    [SerializeField] private PlayerCheckPoint playerCheckPoint;
    [SerializeField] private BackendSync backendSync;

    [Header ("UI log")]
    [SerializeField] private TMP_Text currentSaveText;

    [Header("UI Login")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button submitUserNameButton;

    [Header("UI Create Account")]
    [SerializeField] private Button createAccountButton;
    [SerializeField] private GameObject createAccountPanel;
    [SerializeField] private TMP_InputField createUserNameInputField;
    [SerializeField] private TMP_InputField createPasswordInputField;
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text errorText;

    [Header("Cloud Save UI")]
    [SerializeField] private Button cloudRegisterButton;
    [SerializeField] private GameObject cloudRegisterPanel;
    [SerializeField] private TMP_InputField cloudUserNameInputField;
    [SerializeField] private TMP_InputField cloudPasswordInputField;
    [SerializeField] private TMP_InputField cloudEmailInputField;
    [SerializeField] private Button cloudSubmitButton;
    [SerializeField] private Button cloudCancelButton;

    [Header("OTP UI")]
    [SerializeField] private GameObject otpPanel;
    [SerializeField] private TMP_InputField otpInputField;
    [SerializeField] private Button otpSubmitButton;

    [Header("Save UI")]
    [SerializeField] private GameObject saveListPanel;
    [SerializeField] private GameObject saveItemTemplate;
    [SerializeField] private GameObject syncPanelStatus;
    [SerializeField] private TMP_Text syncSaveNameText;
    [SerializeField] private TMP_Text syncStatusText;

    [Header("Loading UI")]
    [SerializeField] private GameObject LoadingUI;

    [Header("Main UI")]
    [SerializeField] private GameObject MainUI;
    [SerializeField] private Button NewGame_Bt;
    [SerializeField] private Button ContinueGame_Bt;
    [SerializeField] private Button QuitGame_Bt;
    [SerializeField] private Button duplicateButton;
    [SerializeField] private Button syncButton;
    [SerializeField] private Button logoutButton;

    [Header ("GamePlay UI")]
    [SerializeField] private GameObject GamePlayUI;
    [SerializeField] private Button SaveSession_Bt;
    [SerializeField] private Button Menu_Bt;
    [SerializeField] private Button QuitSession_Bt;

    [SerializeField] private TMP_Text playTimeText;
    private string lastSelectedSaveFolder;
    private List<GameObject> saveItemInstances = new List<GameObject>();
    private string selectedSaveFolder;
    private float lastNewGameTime;
    private const float NEW_GAME_COOLDOWN = 1f;
    private float lastPlayTimeUpdate;
    private const float PLAY_TIME_UPDATE_INTERVAL = 1f;


    private async void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        RegisterSaveables();
        await CheckUserAccountsAsync();
        UpdateCurrentSaveTextAsync();
    }

    private void InitializeUI()
    {
        MainUI.SetActive(true);
        GamePlayUI.SetActive(false);
        LoadingUI.SetActive(false);
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(false);
        cloudRegisterPanel.SetActive(false);
        otpPanel.SetActive(false);
        syncPanelStatus.SetActive(false);
        //ContinueGame_Bt.interactable = false;
        //NewGame_Bt.interactable = false;
        //duplicateButton.interactable = false;
        //syncButton.interactable = false;
        //logoutButton.interactable = false;
        //cloudRegisterButton.interactable = false;
    }

    private void SetupButtonListeners()
    {
        submitUserNameButton.onClick.AddListener(() => OnSubmitUserNameAsync());
        NewGame_Bt.onClick.AddListener(OnNewGameClicked);
        ContinueGame_Bt.onClick.AddListener(OnContinueGameClicked);
        QuitGame_Bt.onClick.AddListener(OnQuitGameClicked);
        SaveSession_Bt.onClick.AddListener(OnSaveSessionClicked);
        Menu_Bt.onClick.AddListener(OnMenuClicked);
        QuitSession_Bt.onClick.AddListener(OnQuitSessionClicked);

        createAccountButton.onClick.AddListener(OnCreateAccountButtonClicked);
        createButton.onClick.AddListener(OnCreateButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        cloudRegisterButton.onClick.AddListener(OnCloudRegisterButtonClicked);
        cloudSubmitButton.onClick.AddListener(OnCloudSubmitButtonClicked);
        cloudCancelButton.onClick.AddListener(OnCloudCancelButtonClicked);
        otpSubmitButton.onClick.AddListener(OnOtpSubmitButtonClicked);
        duplicateButton.onClick.AddListener(async () => await RefreshSaveListAsync());
    }

    #region nghiệp vụ cần tách ra

    private void RegisterSaveables()
    {
        saveGameManager.RegisterSaveable(playTimeManager);
        saveGameManager.RegisterSaveable(playerCheckPoint);
        // Thêm các module ISaveable khác ở đây nếu cần
    }

    private async Task CheckUserAccountsAsync()
    {
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath) ||
            JsonUtility.FromJson<UserAccountData>(await File.ReadAllTextAsync(userAccountsPath)).Users.Count == 0)
        {
            loginPanel.SetActive(true);
            Debug.Log("No users found. Showing login panel.");
        }
        else
        {
            await TryAutoLoginAsync();
        }
    }

    private async Task TryAutoLoginAsync()
    {
        if (userAccountManager.TryAutoLogin(out string errorMessage))
        {
            loginPanel.SetActive(false);
            NewGame_Bt.interactable = true;
            duplicateButton.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            await RefreshSaveListAsync();

            var saves = await saveGameManager.GetAllSaveFoldersAsync(userAccountManager.CurrentUserBaseName);
            ContinueGame_Bt.interactable = saves.Count > 0;

            lastSelectedSaveFolder = await GetValidLastSaveFolderAsync();
            if (lastSelectedSaveFolder != null)
            {
                ContinueGame_Bt.interactable = true;
            }
            UpdateCurrentSaveTextAsync(); // Cập nhật UI
            playTimeManager.StartCounting();
            Debug.Log($"Auto-logged in user: {userAccountManager.CurrentUserBaseName}, Save: {lastSelectedSaveFolder}");
        }
        else
        {
            Debug.LogWarning($"Auto-login failed: {errorMessage}");
            loginPanel.SetActive(true);
        }
    }

    private async Task<string> GetValidLastSaveFolderAsync()
    {
        string lastFileSave = userAccountManager.GetLastFileSave();
        if (!string.IsNullOrEmpty(lastFileSave))
        {
            string fileSavePath = Path.Combine(Application.persistentDataPath, "User_DataGame",
                $"FileSave_{userAccountManager.CurrentUserBaseName}", lastFileSave);
            if (Directory.Exists(fileSavePath) && Directory.GetFiles(fileSavePath, "*.json").Length > 0)
            {
                return fileSavePath;
            }
        }

        return await saveGameManager.GetLatestSaveFolderAsync(userAccountManager.CurrentUserBaseName);
    }

    private async Task RefreshSaveListAsync()
    {
        foreach (var item in saveItemInstances)
        {
            Destroy(item);
        }
        saveItemInstances.Clear();

        string userName = userAccountManager.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("No user logged in. Cannot refresh save list.");
            return;
        }

        var saves = await saveGameManager.GetAllSaveFoldersAsync(userName);
        foreach (var save in saves)
        {
            GameObject saveItem = Instantiate(saveItemTemplate, saveListPanel.transform);
            saveItemInstances.Add(saveItem);

            var saveNameText = saveItem.GetComponentInChildren<TMP_Text>();
            var saveImage = saveItem.GetComponentInChildren<RawImage>();
            var buttons = saveItem.GetComponentsInChildren<Button>();
            var selectButton = buttons[0];
            var deleteButton = buttons[1];

            saveNameText.text = Path.GetFileName(save.FolderPath);
            if (!string.IsNullOrEmpty(save.ImagePath))
            {
                StartCoroutine(LoadImageAsync(save.ImagePath, saveImage));
            }
            else
            {
                saveImage.gameObject.SetActive(false);
            }

            selectButton.onClick.AddListener(() => OnSelectSaveAsync(save.FolderPath));
            deleteButton.onClick.AddListener(() => OnDeleteSaveAsync(save.FolderPath));
        }

        Debug.Log($"Refreshed save list for user: {userName}, Found {saves.Count} saves");
    }

    private async void OnDeleteSaveAsync(string folderPath)
    {
        if (await saveGameManager.DeleteSaveFolderAsync(folderPath))
        {
            if (folderPath == selectedSaveFolder)
            {
                selectedSaveFolder = null;
                lastSelectedSaveFolder = null;
            }
            await RefreshSaveListAsync();
            var saves = await saveGameManager.GetAllSaveFoldersAsync(userAccountManager.CurrentUserBaseName);
            ContinueGame_Bt.interactable = saves.Count > 0;
            Debug.Log($"Deleted save: {folderPath}");
        }
        else
        {
            errorText.text = "Failed to delete save!";
            errorText.color = Color.red;
        }
    }

    private async void OnSelectSaveAsync(string folderPath)
    {
        selectedSaveFolder = folderPath;
        lastSelectedSaveFolder = folderPath;
        ContinueGame_Bt.interactable = true;
        await saveGameManager.LoadLatestAsync(userAccountManager.CurrentUserBaseName);
        Debug.Log($"Selected save: {folderPath}");
    }

    private IEnumerator LoadImageAsync(string imagePath, RawImage saveImage)
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        saveImage.texture = texture;
        yield return null;
    }

    #endregion

    // hiển thị panel xác nhận
    private async Task<bool> ShowConfirmationPanel(string message)
    {
        // TODO: Thêm logic hiển thị panel xác nhận với message
        // Ví dụ: Hiển thị UI với 2 nút Confirm/Cancel và chờ phản hồi
        await Task.Delay(100); // Giả lập chờ người dùng
        return true; // Giả định người dùng xác nhận
    }

    private async void OnSubmitUserNameAsync()
    {
        string userName = userNameInputField.text;
        string password = passwordInputField.text;

        if (userAccountManager.Login(userName, password, out string errorMessage))
        {
            loginPanel.SetActive(false);
            NewGame_Bt.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            await RefreshSaveListAsync();

            lastSelectedSaveFolder = await GetValidLastSaveFolderAsync();
            var saves = await saveGameManager.GetAllSaveFoldersAsync(userName);
            ContinueGame_Bt.interactable = saves.Count > 0;

            playTimeManager.StartCounting();
            Debug.Log($"Logged in user: {userName}, Save: {lastSelectedSaveFolder}");
        }
        else
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
            Debug.LogWarning($"Login failed: {errorMessage}");
        }
    }

    private void UpdateCurrentSaveTextAsync()
    {
        if (currentSaveText == null) return;

        if (string.IsNullOrEmpty(lastSelectedSaveFolder))
        {
            currentSaveText.text = "Current Save: None";
        }
        else
        {
            string saveName = Path.GetFileName(lastSelectedSaveFolder);
            currentSaveText.text = $"Current Save: {saveName}";
        }
    }

    #region bt

    private async Task SyncSaveAsync(string folderPath)
    {
        syncPanelStatus.SetActive(true);
        syncSaveNameText.text = $"Save: {Path.GetFileName(folderPath)}";
        syncStatusText.text = "Syncing...";
        syncStatusText.color = Color.yellow;

        await Task.Delay(500);

        var (success, errorMessage) = await saveGameManager.SyncFileSaveAsync(folderPath);
        if (success)
        {
            syncStatusText.text = "Sync successful!";
            syncStatusText.color = Color.green;
            await Task.Delay(1000);
            syncPanelStatus.SetActive(false);
            Debug.Log($"Synced save: {folderPath}");
        }
        else
        {
            syncStatusText.text = $"Error: {errorMessage}";
            syncStatusText.color = Color.red;
            await Task.Delay(2000);
            syncPanelStatus.SetActive(false);
            errorText.text = errorMessage;
            errorText.color = Color.red;
            Debug.LogWarning($"Sync failed: {errorMessage}");
        }
    }

    private void OnCreateAccountButtonClicked()
    {
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(true);
        errorText.text = "";
        createUserNameInputField.text = "";
        createPasswordInputField.text = "";
    }

    private void OnCreateButtonClicked()
    {
        string userName = createUserNameInputField.text;
        string password = createPasswordInputField.text;

        if (userAccountManager.CreateAccount(userName, password, out string errorMessage))
        {
            createAccountPanel.SetActive(false);
            loginPanel.SetActive(true);
            errorText.text = "Account created successfully! Please log in.";
            errorText.color = Color.green;
            userNameInputField.text = userName;
            passwordInputField.text = "";
            Debug.Log($"Created account: {userName}");
        }
        else
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
            Debug.LogWarning($"Create account failed: {errorMessage}");
        }
    }

    private void OnBackButtonClicked()
    {
        createAccountPanel.SetActive(false);
        cloudRegisterPanel.SetActive(false);
        otpPanel.SetActive(false);
        loginPanel.SetActive(true);
        errorText.text = "";
    }

    private void OnCloudRegisterButtonClicked()
    {
        loginPanel.SetActive(false);
        cloudRegisterPanel.SetActive(true);
        errorText.text = "";
        cloudUserNameInputField.text = userAccountManager.CurrentUserBaseName;
        cloudPasswordInputField.text = "";
        cloudEmailInputField.text = "";
    }

    private void OnCloudSubmitButtonClicked()
    {
        string userName = cloudUserNameInputField.text;
        string password = cloudPasswordInputField.text;
        string email = cloudEmailInputField.text;

        if (!userAccountManager.Login(userName, password, out string errorMessage))
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
            Debug.LogWarning($"Cloud register login check failed: {errorMessage}");
            return;
        }

        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            errorText.text = "Please enter a valid email!";
            errorText.color = Color.red;
            Debug.LogWarning("Invalid email for cloud register");
            return;
        }

        StartCoroutine(backendSync.RequestCloudRegister(userName, password, email, (success, message) =>
        {
            if (success)
            {
                cloudRegisterPanel.SetActive(false);
                otpPanel.SetActive(true);
                errorText.text = "OTP sent to your email. Please enter it.";
                errorText.color = Color.green;
                Debug.Log("Cloud register OTP sent");
            }
            else
            {
                errorText.text = message;
                errorText.color = Color.red;
                Debug.LogWarning($"Cloud register failed: {message}");
            }
        }));
    }

    private void OnCloudCancelButtonClicked()
    {
        cloudRegisterPanel.SetActive(false);
        loginPanel.SetActive(true);
        errorText.text = "";
        Debug.Log("Cloud registration cancelled");
    }

    private void OnOtpSubmitButtonClicked()
    {
        string otp = otpInputField.text;
        string userName = cloudUserNameInputField.text;

        if (string.IsNullOrEmpty(otp) || otp.Length != 6)
        {
            errorText.text = "Please enter a valid 6-digit OTP!";
            errorText.color = Color.red;
            Debug.LogWarning("Invalid OTP entered");
            return;
        }

        StartCoroutine(backendSync.VerifyOtp(userName, otp, (success, message) =>
        {
            if (success)
            {
                otpPanel.SetActive(false);
                loginPanel.SetActive(true);
                errorText.text = "Cloud registration successful!";
                errorText.color = Color.green;
                Debug.Log("Cloud registration verified");
            }
            else
            {
                errorText.text = message;
                errorText.color = Color.red;
                Debug.LogWarning($"OTP verification failed: {message}");
            }
        }));
    }

    private async void OnNewGameClicked()
    {
        if (Time.time - lastNewGameTime < NEW_GAME_COOLDOWN)
        {
            Debug.LogWarning("New game button on cooldown!");
            return;
        }

        lastNewGameTime = Time.time;
        MainUI.SetActive(false);
        LoadingUI.SetActive(true);

        try
        {
            // Tạo save folder mới
            string userName = userAccountManager.CurrentUserBaseName;
            string newSaveFolder = await saveGameManager.CreateNewSaveFolder(userName);
            if (string.IsNullOrEmpty(newSaveFolder))
            {
                throw new Exception("Failed to create new save folder!");
            }

            // Đặt lại trạng thái người chơi
            playerCheckPoint.ResetPlayerPosition();
            playerCheckPoint.SetCurrentMapToCurrentScene();
            playTimeManager.ResetSession();

            // Lưu trạng thái ban đầu
            await saveGameManager.SaveAllAsync(userName);
            lastSelectedSaveFolder = newSaveFolder;
            UpdateCurrentSaveTextAsync();

            // Tải scene
            await SceneController.Instance.LoadAdditiveSceneAsync("white_Space", playerCheckPoint);
            GamePlayUI.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start new game: {ex.Message}");
            errorText.text = "Failed to start new game!";
            errorText.color = Color.red;
            InitializeUI(); // Rollback UI
        }
        finally
        {
            LoadingUI.SetActive(false);
        }
    }

    private async void OnContinueGameClicked()
    {
        if (string.IsNullOrEmpty(lastSelectedSaveFolder))
        {
            errorText.text = "No save selected!";
            errorText.color = Color.red;
            return;
        }

        MainUI.SetActive(false);
        LoadingUI.SetActive(true);

        try
        {
            await saveGameManager.LoadLatestAsync(userAccountManager.CurrentUserBaseName);
            string sceneToLoad = playerCheckPoint.CurrentMap;
            if (string.IsNullOrEmpty(sceneToLoad) || sceneToLoad == "Unknown" || sceneToLoad == "Menu")
            {
                sceneToLoad = "white_Space"; // Scene gameplay mặc định
                Debug.LogWarning("[Test] Invalid or unknown scene in checkpoint, using default: white_Space");
            }

            // Kiểm tra xem scene đã được tải additive chưa
            if (!SceneController.Instance.GetLoadedAdditiveScenes().Contains(sceneToLoad))
            {
                await SceneController.Instance.LoadAdditiveSceneAsync(sceneToLoad, playerCheckPoint);
            }
            else
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
            }

            playTimeManager.StartCounting();
            GamePlayUI.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Test] Failed to continue game: {ex.Message}");
            errorText.text = "Failed to load game!";
            errorText.color = Color.red;
            InitializeUI();
        }
        finally
        {
            LoadingUI.SetActive(false);
        }
    }

    private void OnMenuClicked()
    {
        InitializeUI();
    }

    private async void OnQuitSessionClicked()
    {
        LoadingUI.SetActive(true);

        try
        {
            // Lưu trạng thái trước khi thoát session
            await saveGameManager.SaveAwait();

            // Unload scene map hiện tại
            string currentMap = playerCheckPoint.CurrentMap;
            if (!string.IsNullOrEmpty(currentMap) && currentMap != "Unknown" && !new[] { "Menu" }.Contains(currentMap))
            {
                await SceneController.Instance.UnloadAdditiveScenes(currentMap);
            }

            InitializeUI();
            GamePlayUI.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Test] Failed to quit session: {ex.Message}");
            errorText.text = "Failed to quit session!";
            errorText.color = Color.red;
            InitializeUI();
        }
        finally
        {
            LoadingUI.SetActive(false);
        }
    }

    private async void OnSaveSessionClicked()
    {
        //if (Time.time - lastSaveTime < SAVE_COOLDOWN)
        //{
        //    Debug.LogWarning("Save session on cooldown!");
        //    return;
        //}

        //lastSaveTime = Time.time;
        LoadingUI.SetActive(true);

        try
        {
            if (playerCheckPoint.CurrentMap == "Menu")
            {
                playerCheckPoint.SetCurrentMapToCurrentScene(); // Cập nhật map hiện tại nếu đang ở Menu
            }

            await saveGameManager.SaveAwait();
            errorText.text = "Game saved successfully!";
            errorText.color = Color.green;
            await RefreshSaveListAsync();
            UpdateCurrentSaveTextAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save session: {ex.Message}");
            errorText.text = "Failed to save game!";
            errorText.color = Color.red;
        }
        finally
        {
            LoadingUI.SetActive(false);
        }
    }

    private async void OnQuitGameClicked()
    {
        // Hiển thị panel xác nhận (giả định có panel xác nhận)
        bool confirmed = await ShowConfirmationPanel("Are you sure you want to quit?");
        if (!confirmed)
        {
            return;
        }

        LoadingUI.SetActive(true);
        try
        {
            // Lưu trạng thái trước khi thoát
            await saveGameManager.SaveAwait();
            Debug.Log("Game state saved before quitting.");
            Application.Quit();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save before quitting: {ex.Message}");
            errorText.text = "Failed to save game!";
            errorText.color = Color.red;
        }
        finally
        {
            LoadingUI.SetActive(false);
        }
    }

    #endregion
    private void OnDestroy()
    {
        NewGame_Bt.onClick.RemoveListener(OnNewGameClicked);
        ContinueGame_Bt.onClick.RemoveListener(OnContinueGameClicked);
        QuitGame_Bt.onClick.RemoveListener(OnQuitGameClicked);
        SaveSession_Bt.onClick.RemoveListener(OnSaveSessionClicked);
        Menu_Bt.onClick.RemoveListener(OnMenuClicked);
        QuitSession_Bt.onClick.RemoveListener(OnQuitGameClicked);
        submitUserNameButton.onClick.RemoveListener(() => OnSubmitUserNameAsync());
        createAccountButton.onClick.RemoveListener(OnCreateAccountButtonClicked);
        createButton.onClick.RemoveListener(OnCreateButtonClicked);
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        cloudRegisterButton.onClick.RemoveListener(OnCloudRegisterButtonClicked);
        cloudSubmitButton.onClick.RemoveListener(OnCloudSubmitButtonClicked);
        cloudCancelButton.onClick.RemoveListener(OnCloudCancelButtonClicked);
        otpSubmitButton.onClick.RemoveListener(OnOtpSubmitButtonClicked);
        saveGameManager = null;
        playTimeManager = null;
        playerCheckPoint = null;
        userNameInputField = null;
        passwordInputField = null;
        submitUserNameButton = null;
        createAccountButton = null;
        createAccountPanel = null;
        createUserNameInputField = null;
        createPasswordInputField = null;
        createButton = null;
        backButton = null;
        errorText = null;
        cloudRegisterButton = null;
        cloudRegisterPanel = null;
        cloudUserNameInputField = null;
        cloudPasswordInputField = null;
        cloudEmailInputField = null;
        cloudSubmitButton = null;
        cloudCancelButton = null;
        otpPanel = null;
        otpInputField = null;
        otpSubmitButton = null;
        saveListPanel = null;
        saveItemTemplate = null;
        syncPanelStatus = null;
        syncSaveNameText = null;
        syncStatusText = null;
        LoadingUI = null;
        MainUI = null;
        GamePlayUI = null;
        NewGame_Bt = null;
        ContinueGame_Bt = null;
        QuitGame_Bt = null;
        SaveSession_Bt = null;
        Menu_Bt = null;
        QuitSession_Bt = null;
        duplicateButton = null;
        syncButton = null;
        logoutButton = null;
        Debug.Log("[Test] All listeners removed and references cleared.");
    }

}
