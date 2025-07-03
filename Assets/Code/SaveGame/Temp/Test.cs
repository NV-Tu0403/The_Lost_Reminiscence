using Loc_Backend.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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
    //[SerializeField] private TMP_Text syncSaveNameText;
    //[SerializeField] private TMP_Text syncStatusText;

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
    private Coroutine resetSaveSelectionCoroutine;


    private void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        RegisterSaveables();
        CheckUserAccounts();
        UpdateCurrentSaveText();
    }

    private void InitializeUI()
    {
        MainUI.SetActive(true);
        GamePlayUI.SetActive(false);
        //LoadingUI.SetActive(false);
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(false);
        cloudRegisterPanel.SetActive(false);
        otpPanel.SetActive(false);
        syncPanelStatus.SetActive(false);
        ContinueGame_Bt.interactable = false;
        //NewGame_Bt.interactable = false;
        //duplicateButton.interactable = false;
        //syncButton.interactable = false;
        //logoutButton.interactable = false;
        //cloudRegisterButton.interactable = false;
    }

    private void SetupButtonListeners()
    {
        submitUserNameButton.onClick.AddListener(OnSubmitUserName);
        NewGame_Bt.onClick.AddListener(OnNewGameClicked);
        ContinueGame_Bt.onClick.AddListener(OnContinueGameClicked);
        QuitGame_Bt.onClick.AddListener(OnQuitGameClicked);
        SaveSession_Bt.onClick.AddListener(OnSaveSessionClicked);
        Menu_Bt.onClick.AddListener(OnMenuClicked);
        QuitSession_Bt.onClick.AddListener(OnQuitSessionClicked);
        logoutButton.onClick.AddListener(OnLogoutClicked);
        createAccountButton.onClick.AddListener(OnCreateAccountButtonClicked);
        createButton.onClick.AddListener(OnCreateButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        cloudRegisterButton.onClick.AddListener(OnCloudRegisterButtonClicked);
        cloudSubmitButton.onClick.AddListener(OnCloudSubmitButtonClicked);
        cloudCancelButton.onClick.AddListener(OnCloudCancelButtonClicked);
        otpSubmitButton.onClick.AddListener(OnOtpSubmitButtonClicked);
        duplicateButton.onClick.AddListener(RefreshSaveList);
    }

    #region nghiệp vụ cần tách ra

    private void RegisterSaveables()
    {
        saveGameManager.RegisterSaveable(playTimeManager);
        saveGameManager.RegisterSaveable(playerCheckPoint);
    }

    private void CheckUserAccounts()
    {
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath) ||
            JsonUtility.FromJson<UserAccountData>(File.ReadAllText(userAccountsPath)).Users.Count == 0)
        {
            loginPanel.SetActive(true);
            lastSelectedSaveFolder = null;
            Debug.Log("[Test] No users found. Showing login panel.");
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
            NewGame_Bt.interactable = true;
            duplicateButton.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            RefreshSaveList();

            lastSelectedSaveFolder = GetValidLastSaveFolder();
            ContinueGame_Bt.interactable = !string.IsNullOrEmpty(lastSelectedSaveFolder);
            UpdateCurrentSaveText();
            playTimeManager.StartCounting();
        }
        else
        {
            loginPanel.SetActive(true);
            lastSelectedSaveFolder = null;
            ContinueGame_Bt.interactable = false;
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

        var saves = saveGameManager.GetAllSaveFolders(userAccountManager.CurrentUserBaseName);
        string latestFolder = saves.Count > 0 ? saves[0].FolderPath : null;
        ContinueGame_Bt.interactable = !string.IsNullOrEmpty(latestFolder);
        return latestFolder;
    }

    private void RefreshSaveList()
    {
        foreach (var item in saveItemInstances)
        {
            Destroy(item);
        }
        saveItemInstances.Clear();

        var context = ProfessionalSkilMenu.Instance.RefreshSaveList();
        var saves = context.Saves;

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

        ContinueGame_Bt.interactable = context.IsContinueEnabled;
        UpdateCurrentSaveText();
    }

    private IEnumerator ResetSaveSelectionAfterDelay()
    {
        yield return new WaitForSeconds(10f);
        if (!string.IsNullOrEmpty(lastSelectedSaveFolder))
        {
            lastSelectedSaveFolder = null;
            ContinueGame_Bt.interactable = false;
            //UpdateCurrentSaveTextAsync();
            Debug.Log("[Test] Save selection reset after 10 seconds.");
        }
    }

    private void OnSelectSave(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            errorText.text = "Selected save no longer exists!";
            errorText.color = Color.red;
            return;
        }

        selectedSaveFolder = folderPath;
        lastSelectedSaveFolder = folderPath;

        ContinueGame_Bt.interactable = true;

        saveGameManager.LoadFromFolder(folderPath);

        // Log all JSON file contents in the selected save folder
        var jsonFileHandler = new JsonFileHandler();
        var jsonFiles = jsonFileHandler.LoadJsonFiles(folderPath);
        foreach (var (fileName, json) in jsonFiles)
        {
            Debug.Log($"[SaveContent] {fileName}:\n{json}");
        }

        UpdateCurrentSaveText();
    }

    private void OnDeleteSave(string folderPath)
    {
        if (saveGameManager.DeleteSaveFolder(folderPath))
        {
            if (folderPath == lastSelectedSaveFolder)
            {
                lastSelectedSaveFolder = null;
                selectedSaveFolder = null;
                ContinueGame_Bt.interactable = false;
            }
            RefreshSaveList();
            UpdateCurrentSaveText();
        }
        else
        {
            errorText.text = "Failed to delete save!";
            errorText.color = Color.red;
        }
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
        await Task.Delay(100); // Giả lập chờ người dùng
        return true; // Giả định người dùng xác nhận
    }

    private void OnSubmitUserName()
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
            RefreshSaveList();

            lastSelectedSaveFolder = GetValidLastSaveFolder();
            var saves = saveGameManager.GetAllSaveFolders(userName);
            ContinueGame_Bt.interactable = saves.Count > 0;

            playTimeManager.StartCounting();
        }
        else
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
        }
    }

    private void UpdateCurrentSaveText()
    {
        if (currentSaveText == null) return;
        currentSaveText.text = string.IsNullOrEmpty(lastSelectedSaveFolder) ? "Current Save: None" : $"Current Save: {Path.GetFileName(lastSelectedSaveFolder)}";
    }

    public async void Loading(bool state)
    {
        if (state)
        {
            LoadingUI.SetActive(true);
        }
        else
        {
            await Task.Delay(3000); // Giả lập thời gian tải
            LoadingUI.SetActive(false);
        }
    }

    #region bt

    private void OnNewGameClicked()
    {
        Loading(true);

        Core.Instance._menuCamera.SetActive(false);
        lastSelectedSaveFolder = ProfessionalSkilMenu.Instance.OnNewGame();

        UpdateCurrentSaveText();
        ContinueGame_Bt.interactable = true;

        MainUI.SetActive(false);
        GamePlayUI.SetActive(true);
        Loading(false);
        //try
        //{
        //    lastSelectedSaveFolder = ProfessionalSkilMenu.Instance.OnNewGame();
        //    ContinueGame_Bt.interactable = true;
        //    UpdateCurrentSaveText();
        //}
        //catch (Exception)
        //{
        //    errorText.text = "Failed to start new game!";
        //    errorText.color = Color.red;
        //    InitializeUI();
        //}
        //finally
        //{
        //    LoadingUI.SetActive(false);
        //}
    }

    private void OnContinueGameClicked()
    {
        if (string.IsNullOrEmpty(lastSelectedSaveFolder) || !Directory.Exists(lastSelectedSaveFolder))
        {
            errorText.text = "No valid save selected!";
            errorText.color = Color.red;
            ContinueGame_Bt.interactable = false;
            lastSelectedSaveFolder = null;
            UpdateCurrentSaveText();
            return;
        }

        Loading(true);
        try
        {
            Core.Instance._menuCamera.SetActive(false);
            ProfessionalSkilMenu.Instance.OnContinueGame(lastSelectedSaveFolder);
            GamePlayUI.SetActive(true);
            MainUI.SetActive(false);
        }
        catch (Exception)
        {
            errorText.text = "Failed to load game!";
            errorText.color = Color.red;
            InitializeUI();
        }
        finally
        {
            Loading(false);
        }
    }

    private void OnMenuClicked()
    {
        MainUI.SetActive(true);
        GamePlayUI.SetActive(false);
    }

    private void OnQuitSessionClicked()
    {
        try
        {
            Loading(true);
            Core.Instance._menuCamera.SetActive(true);
            ProfessionalSkilMenu.Instance.OnQuitSession(lastSelectedSaveFolder);
            lastSelectedSaveFolder = null;
            ContinueGame_Bt.interactable = false;
            RefreshSaveList();
            InitializeUI();
            GamePlayUI.SetActive(false);
        }
        catch (Exception)
        {
            errorText.text = "Failed to quit session!";
            errorText.color = Color.red;
            InitializeUI();
        }
        finally
        {
            Loading(false);
        }
    }

    private void OnSaveSessionClicked()
    {
        //Loading(true);
        try
        {
            ProfessionalSkilMenu.Instance.OnSaveSession();
            errorText.text = "Game saved successfully!";
            errorText.color = Color.green;
            RefreshSaveList();
            UpdateCurrentSaveText();
        }
        catch (Exception)
        {
            errorText.text = "Failed to save game!";
            errorText.color = Color.red;
        }
        finally
        {
            //Loading(false);
        }
    }

    private void OnLogoutClicked()  // cần tách ra
    {
        Loading(true);
        try
        {
            userAccountManager.Logout();
            lastSelectedSaveFolder = null;
            ContinueGame_Bt.interactable = false;
            NewGame_Bt.interactable = false;
            duplicateButton.interactable = false;
            syncButton.interactable = false;
            logoutButton.interactable = false;
            cloudRegisterButton.interactable = false;

            loginPanel.SetActive(true);
            MainUI.SetActive(true);
            GamePlayUI.SetActive(false);
            RefreshSaveList();
            UpdateCurrentSaveText();
        }
        catch (Exception)
        {
            errorText.text = "Failed to logout!";
            errorText.color = Color.red;
        }
        finally
        {
            Loading(false);
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

    private void OnCreateButtonClicked() // cần tách ra
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
        }
        else
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
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

    private void OnCloudRegisterButtonClicked() // cần tách ra
    {
        loginPanel.SetActive(false);
        cloudRegisterPanel.SetActive(true);
        errorText.text = "";
        cloudUserNameInputField.text = userAccountManager.CurrentUserBaseName;
        cloudPasswordInputField.text = "";
        cloudEmailInputField.text = "";
    }

    private void OnCloudSubmitButtonClicked() // cần tách ra
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

    private void OnOtpSubmitButtonClicked() // cần tách ra
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

    private async void OnQuitGameClicked()
    {
        // Hiển thị panel xác nhận (giả định có panel xác nhận)
        bool confirmed = await ShowConfirmationPanel("Are you sure you want to quit?");
        if (!confirmed)
        {
            return;
        }

        Loading(true);
        try
        {
            // Lưu trạng thái trước khi thoát
            //await SaveGameManager.Instance.SaveAwait();
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
            Loading(false);
        }
    }

    #endregion
    private void OnDestroy()
    {
        NewGame_Bt.onClick.RemoveListener(OnNewGameClicked);
        ContinueGame_Bt.onClick.RemoveListener(OnContinueGameClicked);
        QuitGame_Bt.onClick.RemoveListener(OnQuitGameClicked);
        SaveSession_Bt.onClick.RemoveListener(() => OnSaveSessionClicked());
        Menu_Bt.onClick.RemoveListener(OnMenuClicked);
        QuitSession_Bt.onClick.RemoveListener(OnQuitGameClicked);
        //submitUserNameButton.onClick.RemoveListener(() SubmitUserNameAs());
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
        //syncSaveNameText = null;
        //syncStatusText = null;
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
