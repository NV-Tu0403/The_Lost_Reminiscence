using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using Loc_Backend.Scripts;
using System.Linq;
using System;

public class SaveGameUI : MonoBehaviour
{
    [SerializeField] private SaveGameManager saveGameManager;
    [SerializeField] private BackendSync backendSync;
    [SerializeField] private ProgressionManager progressionManager;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button submitUserNameButton;

    [SerializeField] private Button createAccountButton;
    [SerializeField] private GameObject createAccountPanel;
    [SerializeField] private TMP_InputField createUserNameInputField;
    [SerializeField] private TMP_InputField createPasswordInputField;
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text errorText;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button duplicateButton;
    [SerializeField] private Button syncButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Button cloudRegisterButton;
    [SerializeField] private GameObject cloudRegisterPanel;
    [SerializeField] private TMP_InputField cloudUserNameInputField;
    [SerializeField] private TMP_InputField cloudPasswordInputField;
    [SerializeField] private TMP_InputField cloudEmailInputField;
    [SerializeField] private Button cloudSubmitButton;
    [SerializeField] private Button cloudCancelButton;

    [SerializeField] private GameObject otpPanel;
    [SerializeField] private TMP_InputField otpInputField;
    [SerializeField] private Button otpSubmitButton;

    [SerializeField] private GameObject saveListPanel;
    [SerializeField] private GameObject jsonContentPanel;
    [SerializeField] private GameObject saveItemTemplate;
    [SerializeField] private GameObject jsonTextTemplate;
    [SerializeField] private GameObject syncPanelStatus;
    [SerializeField] private TMP_Text syncSaveNameText;
    [SerializeField] private TMP_Text syncStatusText;

    [SerializeField] private TMP_Text playTimeText; // Text để hiển thị PlayTime
    private float sessionPlayTime = 0f; // Thời gian chơi trong phiên hiện tại (giây)
    private bool isCountingPlayTime = false; // Trạng thái bộ đếm
    private string lastSelectedSaveFolder; // Lưu file save được chọn
    private double totalPlayTime = 0.0; // Lưu tổng PlayTime để hiển thị

    private List<GameObject> saveItemInstances = new List<GameObject>();
    private List<GameObject> jsonTextInstances = new List<GameObject>();
    private string selectedSaveFolder;

    void Start()
    {
        if (saveGameManager == null || progressionManager == null || backendSync == null)
        {
            Debug.LogError("SaveGameManager, ProgressionManager, or BackendSync is not assigned!");
            return;
        }

        // Đảm bảo tất cả các panel UI đều tắt khi khởi động
        if (loginPanel != null) loginPanel.SetActive(false);
        if (createAccountPanel != null) createAccountPanel.SetActive(false);
        if (cloudRegisterPanel != null) cloudRegisterPanel.SetActive(false);
        if (otpPanel != null) otpPanel.SetActive(false);
        if (syncPanelStatus != null) syncPanelStatus.SetActive(false);


        // Kiểm tra playTimeText
        if (playTimeText == null)
        {
            Debug.LogWarning("PlayTimeText is not assigned in Inspector!");
        }

        // Gán sự kiện cho các nút
        submitUserNameButton.onClick.AddListener(OnSubmitUserName);
        createAccountButton.onClick.AddListener(OnCreateAccountButtonClicked);
        createButton.onClick.AddListener(OnCreateButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        duplicateButton.onClick.AddListener(OnDuplicateButtonClicked);
        syncButton.onClick.AddListener(OnSyncButtonClicked);
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        cloudRegisterButton.onClick.AddListener(OnCloudRegisterButtonClicked);
        cloudSubmitButton.onClick.AddListener(OnCloudSubmitButtonClicked);
        otpSubmitButton.onClick.AddListener(OnOtpSubmitButtonClicked);
        cloudCancelButton.onClick.AddListener(OnCloudCancelButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);

        // Kiểm tra UserAccount.json để quyết định hiển thị panel
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath) ||
            JsonUtility.FromJson<UserAccountList>(File.ReadAllText(userAccountsPath)).Users.Count == 0)
        {
            // Không có user, hiển thị login panel
            loginPanel.SetActive(true);
            createAccountPanel.SetActive(false);
            cloudRegisterPanel.SetActive(false);
            otpPanel.SetActive(false);
            syncPanelStatus.SetActive(false);
            continueButton.interactable = false;
            newGameButton.interactable = false;
            duplicateButton.interactable = false;
            syncButton.interactable = false;
            logoutButton.interactable = false;
            cloudRegisterButton.interactable = false;
            Debug.Log("No users found. Showing login panel.");
        }
        else
        {
            // Có user, thử tự động đăng nhập
            TryAutoLogin();
        }
    }

    void Update()
    {
        if (isCountingPlayTime && playTimeText != null)
        {
            sessionPlayTime += Time.deltaTime;
            playTimeText.text = $"Play Time: {FormatPlayTime(totalPlayTime + sessionPlayTime)}";
        }
    }

    private void TryAutoLogin()
    {
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath))
        {
            Debug.LogWarning("UserAccounts.json not found!");
            return;
        }

        try
        {
            UserAccountList userAccounts = JsonUtility.FromJson<UserAccountList>(File.ReadAllText(userAccountsPath));
            if (string.IsNullOrEmpty(userAccounts.LastAccount))
            {
                Debug.Log("LastAccount is empty. Showing login panel.");
                loginPanel.SetActive(true);
                return;
            }

            // Tìm user tương ứng với LastAccount
            var user = userAccounts.Users.FirstOrDefault(u => u.UserName == userAccounts.LastAccount);
            if (user == null)
            {
                Debug.LogWarning($"LastAccount '{userAccounts.LastAccount}' not found in Users!");
                loginPanel.SetActive(true);
                return;
            }

            // Kiểm tra currentUserNamePlaying
            if (!string.IsNullOrEmpty(saveGameManager.CurrentUserNamePlaying))
            {
                Debug.LogWarning($"CurrentUserNamePlaying is already set to '{saveGameManager.CurrentUserNamePlaying}'. Skipping auto-login.");
                return;
            }

            // Thử đăng nhập với BaseName (không cần mật khẩu)
            if (saveGameManager.LoginUser(user.BaseName, null, out string errorMessage))
            {
                loginPanel.SetActive(false);
                newGameButton.interactable = true;
                duplicateButton.interactable = true;
                syncButton.interactable = true;
                logoutButton.interactable = true;
                cloudRegisterButton.interactable = true;
                errorText.text = "";
                RefreshSaveList();

                // Kiểm tra xem có file save nào không
                var saves = saveGameManager.GetAllSaveFolders(user.BaseName);
                continueButton.interactable = saves.Count > 0; // Bật nút nếu có ít nhất 1 file save

                // Kiểm tra LastFileSave
                if (!string.IsNullOrEmpty(userAccounts.LastFileSave))
                {
                    string fileSavePath = Path.Combine(Application.persistentDataPath, "User_DataGame", $"FileSave_{user.BaseName}", userAccounts.LastFileSave);
                    if (Directory.Exists(fileSavePath) && Directory.GetFiles(fileSavePath, "*.json").Length > 0)
                    {
                        lastSelectedSaveFolder = fileSavePath;
                        continueButton.interactable = true;
                    }
                }

                // Nếu không có LastFileSave hợp lệ, lấy file mới nhất
                if (string.IsNullOrEmpty(lastSelectedSaveFolder))
                {
                    lastSelectedSaveFolder = saveGameManager.GetLatestSaveFolder(user.BaseName);
                    if (lastSelectedSaveFolder != null)
                    {
                        continueButton.interactable = true;
                    }
                }

                // Lấy PlayTime từ UserAccount.json
                totalPlayTime = userAccounts.PlayTime;
                isCountingPlayTime = true;
                Debug.Log($"Auto-logged in user: {user.BaseName}, Save: {lastSelectedSaveFolder}, PlayTime: {FormatPlayTime(totalPlayTime)}");
            }
            else
            {
                Debug.LogWarning($"Auto-login failed for user '{user.BaseName}': {errorMessage}");
                loginPanel.SetActive(true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read UserAccount.json: {e.Message}");
            loginPanel.SetActive(true);
        }
    }

    private void OnCloudCancelButtonClicked()
    {
        cloudRegisterPanel.SetActive(false);
        loginPanel.SetActive(true);
        errorText.text = "";
    }

    private void OnQuitButtonClicked()
    {
        progressionManager.SaveProgression();
        SaveSessionData();
        sessionPlayTime = 0f;
        isCountingPlayTime = false;
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    private void SaveSessionData()
    {
        string userName = saveGameManager.CurrentUserNamePlaying;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("No user logged in. Skipping session save.");
            return;
        }

        // Tìm UserName đầy đủ (a_20250525_153959)
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath))
        {
            Debug.LogError("UserAccounts.json not found!");
            return;
        }

        try
        {
            UserAccountList userAccounts = JsonUtility.FromJson<UserAccountList>(File.ReadAllText(userAccountsPath));
            var user = userAccounts.Users.FirstOrDefault(u => u.BaseName == userName);
            if (user == null)
            {
                Debug.LogWarning($"User with BaseName '{userName}' not found in UserAccounts!");
                return;
            }

            // Lấy file save hiện tại
            string saveFolder = lastSelectedSaveFolder ?? saveGameManager.GetLatestSaveFolder(userName);
            string lastFileSave = saveFolder != null ? Path.GetFileName(saveFolder) : "";

            // Tính toán PlayTime (tổng giây)
            double existingPlayTime = userAccounts.PlayTime;
            double sessionPlayTimeSeconds = sessionPlayTime;
            double totalPlayTime = existingPlayTime + sessionPlayTimeSeconds;

            // Lưu thông tin phiên
            saveGameManager.SaveLastSession(user.UserName, lastFileSave, totalPlayTime);

            // Cập nhật totalPlayTime để hiển thị
            this.totalPlayTime = totalPlayTime;
            Debug.Log($"Saved PlayTime: {FormatPlayTime(totalPlayTime)} ({totalPlayTime} seconds)");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save session data: {e.Message}");
        }
    }

    private void OnSubmitUserName()
    {
        string userName = userNameInputField.text;
        string password = passwordInputField.text;

        if (saveGameManager.LoginUser(userName, password, out string errorMessage))
        {
            loginPanel.SetActive(false);
            newGameButton.interactable = true;
            duplicateButton.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            RefreshSaveList();

            // Kiểm tra xem có file save nào không
            lastSelectedSaveFolder = saveGameManager.GetLatestSaveFolder(userName);
            var saves = saveGameManager.GetAllSaveFolders(userName);
            continueButton.interactable = saves.Count > 0;

            // Lấy PlayTime từ UserAccount.json
            string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
            UserAccountList userAccounts = JsonUtility.FromJson<UserAccountList>(File.ReadAllText(userAccountsPath));
            totalPlayTime = userAccounts.PlayTime;

            isCountingPlayTime = true;
            Debug.Log($"Logged in user: {userName}, Save: {lastSelectedSaveFolder}, PlayTime: {FormatPlayTime(totalPlayTime)}");
        }
        else
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
            Debug.LogWarning($"Login failed: {errorMessage}");
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

    private string FormatPlayTime(double totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
        int days = timeSpan.Days;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        return $"{days} {hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    private void OnCreateButtonClicked()
    {
        string userName = createUserNameInputField.text;
        string password = createPasswordInputField.text;

        if (saveGameManager.InputUserAccount(userName, password, out string errorMessage))
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
        cloudUserNameInputField.text = saveGameManager.CurrentUserNamePlaying;
        cloudPasswordInputField.text = "";
        cloudEmailInputField.text = "";
    }

    private void OnCloudSubmitButtonClicked()
    {
        string userName = cloudUserNameInputField.text;
        string password = cloudPasswordInputField.text;
        string email = cloudEmailInputField.text;

        if (!saveGameManager.LoginUser(userName, password, out string errorMessage))
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

    private void OnContinueButtonClicked()
    {
        string userName = saveGameManager.CurrentUserNamePlaying;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("CurrentUserNamePlaying is not set!");
            errorText.text = "Please log in first!";
            errorText.color = Color.red;
            return;
        }

        string saveFolder = lastSelectedSaveFolder ?? saveGameManager.GetLatestSaveFolder(userName);
        if (saveFolder != null)
        {
            progressionManager.LoadProgression();
            Debug.Log($"Continued with save: {saveFolder}");
        }
        else
        {
            Debug.LogWarning("No save found! Starting new game.");
            errorText.text = "No save found! Starting new game.";
            errorText.color = Color.yellow;
            progressionManager.CreateNewGame();
        }
    }

    private void OnNewGameButtonClicked()
    {
        progressionManager.CreateNewGame();
        RefreshSaveList();
        continueButton.interactable = true;
        Debug.Log("Started new game.");
    }

    private void OnDuplicateButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedSaveFolder))
        {
            Debug.LogWarning("No save selected to duplicate!");
            errorText.text = "Please select a save to duplicate!";
            errorText.color = Color.red;
            return;
        }

        string userName = saveGameManager.CurrentUserNamePlaying;
        string newSaveFolder = saveGameManager.DuplicateSaveFolder(selectedSaveFolder, userName);
        if (!string.IsNullOrEmpty(newSaveFolder))
        {
            lastSelectedSaveFolder = newSaveFolder; // Cập nhật file save mới
            RefreshSaveList();
            continueButton.interactable = true;
            Debug.Log($"Duplicated save to: {newSaveFolder}");
        }
        else
        {
            errorText.text = "Failed to duplicate save!";
            errorText.color = Color.red;
        }
    }

    private void OnSyncButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedSaveFolder))
        {
            Debug.LogWarning("No save selected to sync!");
            errorText.text = "Please select a save to sync!";
            errorText.color = Color.red;
            return;
        }

        StartCoroutine(SyncSaveCoroutine(selectedSaveFolder));
    }

    private IEnumerator SyncSaveCoroutine(string folderPath)
    {
        syncPanelStatus.SetActive(true);
        syncSaveNameText.text = $"Save: {Path.GetFileName(folderPath)}";
        syncStatusText.text = "Syncing...";
        syncStatusText.color = Color.yellow;

        yield return new WaitForSeconds(0.5f);

        if (saveGameManager.SyncFileSave(folderPath, out string errorMessage))
        {
            syncStatusText.text = "Sync successful!";
            syncStatusText.color = Color.green;
            yield return new WaitForSeconds(1f);
            syncPanelStatus.SetActive(false);
            Debug.Log($"Synced save: {folderPath}");
        }
        else
        {
            syncStatusText.text = $"Error: {errorMessage}";
            syncStatusText.color = Color.red;
            yield return new WaitForSeconds(2f);
            syncPanelStatus.SetActive(false);
            errorText.text = errorMessage;
            errorText.color = Color.red;
            Debug.LogWarning($"Sync failed: {errorMessage}");
        }
    }

    private void OnLogoutButtonClicked()
    {
        progressionManager.SaveProgression();
        SaveSessionData();
        saveGameManager.Logout();
        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
        cloudRegisterPanel.SetActive(false);
        otpPanel.SetActive(false);
        syncPanelStatus.SetActive(false);
        continueButton.interactable = false;
        newGameButton.interactable = false;
        duplicateButton.interactable = false;
        syncButton.interactable = false;
        logoutButton.interactable = false;
        cloudRegisterButton.interactable = false;
        selectedSaveFolder = null;
        lastSelectedSaveFolder = null;
        ClearJsonContent();
        RefreshSaveList();
        errorText.text = "";
        userNameInputField.text = "";
        passwordInputField.text = "";
        isCountingPlayTime = false;
        sessionPlayTime = 0f;
        totalPlayTime = 0.0; // Reset totalPlayTime
        if (playTimeText != null)
        {
            playTimeText.text = "Play Time: 0 00:00:00";
        }
        Debug.Log("Logged out and returned to login screen.");
    }

    private void RefreshSaveList()
    {
        foreach (var item in saveItemInstances)
        {
            Destroy(item);
        }
        saveItemInstances.Clear();

        string userName = saveGameManager.CurrentUserNamePlaying;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("No user logged in. Cannot refresh save list.");
            return;
        }

        var saves = saveGameManager.GetAllSaveFolders(userName);
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
                byte[] imageBytes = File.ReadAllBytes(save.ImagePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                saveImage.texture = texture;
            }
            else
            {
                saveImage.gameObject.SetActive(false);
            }

            selectButton.onClick.AddListener(() => OnSelectSave(save.FolderPath));
            deleteButton.onClick.AddListener(() => OnDeleteSave(save.FolderPath));
        }

        Debug.Log($"Refreshed save list for user: {userName}, Found {saves.Count} saves");
    }

    private void OnSelectSave(string folderPath)
    {
        selectedSaveFolder = folderPath;
        lastSelectedSaveFolder = folderPath;
        continueButton.interactable = true; // Bật nút continue khi chọn save
        Debug.Log($"Selected save: {folderPath}");
        DisplayJsonContent(folderPath);
    }

    private void OnDeleteSave(string folderPath)
    {
        if (saveGameManager.DeleteSaveFolder(folderPath))
        {
            if (folderPath == selectedSaveFolder)
            {
                selectedSaveFolder = null;
                lastSelectedSaveFolder = null;
                ClearJsonContent();
            }
            RefreshSaveList();
            var saves = saveGameManager.GetAllSaveFolders(saveGameManager.CurrentUserNamePlaying);
            continueButton.interactable = saves.Count > 0;
            Debug.Log($"Deleted save: {folderPath}");
        }
        else
        {
            errorText.text = "Failed to delete save!";
            errorText.color = Color.red;
        }
    }

    private void DisplayJsonContent(string folderPath)
    {
        ClearJsonContent();
        var jsonFiles = Directory.GetFiles(folderPath, "*.json");
        foreach (var file in jsonFiles)
        {
            string json = saveGameManager.LoadJsonFile(folderPath, Path.GetFileName(file));
            if (!string.IsNullOrEmpty(json))
            {
                GameObject jsonText = Instantiate(jsonTextTemplate, jsonContentPanel.transform);
                jsonTextInstances.Add(jsonText);
                var textComponent = jsonText.GetComponent<TMP_Text>();
                textComponent.text = $"File: {Path.GetFileName(file)}\nContent: {json}";
            }
        }
        Debug.Log($"Displayed JSON content for save: {folderPath}, Found {jsonFiles.Length} JSON files");
    }

    private void ClearJsonContent()
    {
        foreach (var text in jsonTextInstances)
        {
            Destroy(text);
        }
        jsonTextInstances.Clear();
        Debug.Log("Cleared JSON content display");
    }
}