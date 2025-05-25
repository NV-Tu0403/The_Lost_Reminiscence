using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System;
using Loc_Backend.Scripts;

public class SaveGameUI : MonoBehaviour
{
    [SerializeField] private UserAccountManager userAccountManager;
    [SerializeField] private SaveGameManager saveGameManager;
    [SerializeField] private PlayTimeManager playTimeManager;
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

    [SerializeField] private TMP_Text playTimeText;
    private string lastSelectedSaveFolder;
    private List<GameObject> saveItemInstances = new List<GameObject>();
    private List<GameObject> jsonTextInstances = new List<GameObject>();
    private string selectedSaveFolder;
    private float lastNewGameTime;
    private const float NEW_GAME_COOLDOWN = 1f;

    void Start()
    {
        if (!ValidateReferences()) return;

        InitializeUI();
        SetupButtonListeners();
        CheckUserAccounts();
    }

    void Update()
    {
        if (playTimeManager.isCounting && playTimeText != null)
        {
            playTimeText.text = $"Play Time: {playTimeManager.FormatPlayTime(playTimeManager.GetTotalPlayTime())}";
        }
    }

    private bool ValidateReferences()
    {
        if (userAccountManager == null || saveGameManager == null || playTimeManager == null ||
            progressionManager == null || backendSync == null)
        {
            Debug.LogError("One or more managers are not assigned!");
            return false;
        }
        if (playTimeText == null)
        {
            Debug.LogWarning("PlayTimeText is not assigned in Inspector!");
        }
        return true;
    }

    private void InitializeUI()
    {
        loginPanel.SetActive(false);
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
    }

    private void SetupButtonListeners()
    {
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
    }

    private void CheckUserAccounts()
    {
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath) ||
            JsonUtility.FromJson<UserAccountData>(File.ReadAllText(userAccountsPath)).Users.Count == 0)
        {
            loginPanel.SetActive(true);
            Debug.Log("No users found. Showing login panel.");
        }
        else
        {
            TryAutoLogin();
        }
    }

    private void TryAutoLogin()
    {
        if (userAccountManager.TryAutoLogin(out string errorMessage))
        {
            loginPanel.SetActive(false);
            newGameButton.interactable = true;
            duplicateButton.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            RefreshSaveList();

            var saves = saveGameManager.GetAllSaveFolders(userAccountManager.CurrentUserBaseName);
            continueButton.interactable = saves.Count > 0;

            lastSelectedSaveFolder = GetValidLastSaveFolder();
            if (lastSelectedSaveFolder != null)
            {
                continueButton.interactable = true;
            }

            playTimeManager.StartCounting();
            Debug.Log($"Auto-logged in user: {userAccountManager.CurrentUserBaseName}, Save: {lastSelectedSaveFolder}");
        }
        else
        {
            Debug.LogWarning($"Auto-login failed: {errorMessage}");
            loginPanel.SetActive(true);
        }
    }

    private string GetValidLastSaveFolder()
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

        return saveGameManager.GetLatestSaveFolder(userAccountManager.CurrentUserBaseName);
    }

    private void OnSubmitUserName()
    {
        string userName = userNameInputField.text;
        string password = passwordInputField.text;

        if (userAccountManager.Login(userName, password, out string errorMessage))
        {
            loginPanel.SetActive(false);
            newGameButton.interactable = true;
            duplicateButton.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            RefreshSaveList();

            lastSelectedSaveFolder = GetValidLastSaveFolder();
            var saves = saveGameManager.GetAllSaveFolders(userName);
            continueButton.interactable = saves.Count > 0;

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

    private void OnContinueButtonClicked()
    {
        string userName = userAccountManager.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("No user logged in!");
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
            OnNewGameButtonClicked();
        }
    }

    private void OnNewGameButtonClicked()
    {
        if (Time.time - lastNewGameTime < NEW_GAME_COOLDOWN) return;
        lastNewGameTime = Time.time;

        if (!newGameButton.interactable) return;
        newGameButton.interactable = false;

        string userName = userAccountManager.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("No user logged in!");
            errorText.text = "Please log in first!";
            errorText.color = Color.red;
            newGameButton.interactable = true;
            return;
        }

        try
        {
            string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
            lastSelectedSaveFolder = newSaveFolder;
            progressionManager.CreateNewGame();
            RefreshSaveList();
            continueButton.interactable = true;
            Debug.Log($"Started new game with save: {newSaveFolder}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create new game: {e.Message}");
            errorText.text = "Failed to create new game!";
            errorText.color = Color.red;
        }
        finally
        {
            newGameButton.interactable = true;
        }
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

        string userName = userAccountManager.CurrentUserBaseName;
        string newSaveFolder = saveGameManager.DuplicateSaveFolder(selectedSaveFolder, userName);
        if (!string.IsNullOrEmpty(newSaveFolder))
        {
            lastSelectedSaveFolder = newSaveFolder;
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
        userAccountManager.Logout();
        playTimeManager.StopCounting();
        playTimeManager.ResetSession();
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
        if (playTimeText != null)
        {
            playTimeText.text = "Play Time: 0 00:00:00";
        }
        Debug.Log("Logged out and returned to login screen.");
    }

    private void OnQuitButtonClicked()
    {
        progressionManager.SaveProgression();
        SaveSessionData();
        playTimeManager.StopCounting();
        playTimeManager.ResetSession();
        OnLogoutButtonClicked();
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    /// <summary>
    /// lưu dữ liệu phiên hiện tại, bao gồm tên người dùng, thư mục lưu gần nhất và thời gian chơi tổng.
    /// </summary>
    private void SaveSessionData()
    {
        string userName = userAccountManager.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("No user logged in. Skipping session save.");
            return;
        }

        string saveFolder = lastSelectedSaveFolder ?? saveGameManager.GetLatestSaveFolder(userName);
        string lastFileSave = saveFolder != null ? Path.GetFileName(saveFolder) : "";
        double totalPlayTime = playTimeManager.GetTotalPlayTime();

        userAccountManager.UpdateLastSession(lastFileSave, totalPlayTime);
    }

    /// <summary>
    /// làm mới danh sách các lưu trữ hiện có cho người dùng hiện tại.
    /// </summary>
    private void RefreshSaveList()
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
        continueButton.interactable = true;
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
            var saves = saveGameManager.GetAllSaveFolders(userAccountManager.CurrentUserBaseName);
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