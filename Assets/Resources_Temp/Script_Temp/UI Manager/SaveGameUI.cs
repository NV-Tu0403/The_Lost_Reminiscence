using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using Loc_Backend.Scripts;

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
        cloudRegisterButton.interactable = false; // Tắt đến khi đăng nhập
    }

    private void OnCloudCancelButtonClicked()
    {
        cloudRegisterPanel.SetActive(false);
    }

    private void OnSubmitUserName()
    {
        string userName = userNameInputField.text;
        string password = passwordInputField.text;

        if (saveGameManager.LoginUser(userName, password, out string errorMessage))
        {
            loginPanel.SetActive(false);
            continueButton.interactable = true;
            newGameButton.interactable = true;
            duplicateButton.interactable = true;
            syncButton.interactable = true;
            logoutButton.interactable = true;
            cloudRegisterButton.interactable = true;
            errorText.text = "";
            RefreshSaveList();
        }
        else
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
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

        if (saveGameManager.InputUserAccount(userName, password, out string errorMessage))
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

    private void OnCloudRegisterButtonClicked()
    {
        loginPanel.SetActive(false);
        cloudRegisterPanel.SetActive(true);
        errorText.text = "";
        cloudUserNameInputField.text = saveGameManager.CurrentUserNamePlaying; // Tự điền
        cloudPasswordInputField.text = "";
        cloudEmailInputField.text = "";
    }

    private void OnCloudSubmitButtonClicked()
    {
        string userName = cloudUserNameInputField.text;
        string password = cloudPasswordInputField.text;
        string email = cloudEmailInputField.text;

        // Kiểm tra UserName và Password local
        if (!saveGameManager.LoginUser(userName, password, out string errorMessage))
        {
            errorText.text = errorMessage;
            errorText.color = Color.red;
            return;
        }

        // Kiểm tra email cơ bản
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            errorText.text = "Please enter a valid email!";
            errorText.color = Color.red;
            return;
        }

        // Gửi request đăng ký cloud
        StartCoroutine(backendSync.RequestCloudRegister(userName, password, email, (success, message) =>
        {
            if (success)
            {
                cloudRegisterPanel.SetActive(false);
                otpPanel.SetActive(true);
                errorText.text = "OTP sent to your email. Please enter it.";
                errorText.color = Color.green;
            }
            else
            {
                errorText.text = message;
                errorText.color = Color.red;
            }
        }));
    }

    private void OnOtpSubmitButtonClicked()
    {
        string otp = otpInputField.text;
        string userName = cloudUserNameInputField.text; // Lấy từ CloudRegisterPanel

        if (string.IsNullOrEmpty(otp) || otp.Length != 6)
        {
            errorText.text = "Please enter a valid 6-digit OTP!";
            errorText.color = Color.red;
            return;
        }

        // Gửi OTP để xác thực
        StartCoroutine(backendSync.VerifyOtp(userName, otp, (success, message) =>
        {
            if (success)
            {
                otpPanel.SetActive(false);
                loginPanel.SetActive(true);
                errorText.text = "đăng ký Cloud thành công!";
                errorText.color = Color.green;
            }
            else
            {
                errorText.text = message;
                errorText.color = Color.red;
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
            return;
        }

        string latestSave = saveGameManager.GetLatestSaveFolder(userName);
        if (latestSave != null)
        {
            progressionManager.LoadProgression();
            Debug.Log($"Continued with latest save: {latestSave}");
        }
        else
        {
            Debug.LogWarning("No save found! Starting new game.");
            progressionManager.CreateNewGame();
        }
    }

    private void OnNewGameButtonClicked()
    {
        progressionManager.CreateNewGame();
        RefreshSaveList();
        Debug.Log("Started new game with new save.");
    }

    private void OnDuplicateButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedSaveFolder))
        {
            Debug.LogWarning("No save selected to duplicate!");
            errorText.text = "Please select a save to duplicate!";
            return;
        }

        string userName = saveGameManager.CurrentUserNamePlaying;
        string newSaveFolder = saveGameManager.DuplicateSaveFolder(selectedSaveFolder, userName);
        if (!string.IsNullOrEmpty(newSaveFolder))
        {
            RefreshSaveList();
            Debug.Log($"Duplicated save to: {newSaveFolder}");
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
        syncStatusText.text = "Đang sao chép...";
        syncStatusText.color = Color.yellow;

        yield return new WaitForSeconds(0.5f);

        if (saveGameManager.SyncFileSave(folderPath, out string errorMessage))
        {
            syncStatusText.text = "Sao chép thành công!";
            syncStatusText.color = Color.green;
            yield return new WaitForSeconds(1f);
            syncPanelStatus.SetActive(false);
        }
        else
        {
            syncStatusText.text = $"Lỗi: {errorMessage}";
            syncStatusText.color = Color.red;
            yield return new WaitForSeconds(2f);
            syncPanelStatus.SetActive(false);
            errorText.text = errorMessage;
            errorText.color = Color.red;
        }
    }

    private void OnLogoutButtonClicked()
    {
        progressionManager.SaveProgression();
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
        ClearJsonContent();
        RefreshSaveList();
        errorText.text = "";
        userNameInputField.text = "";
        passwordInputField.text = "";
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
    }

    private void OnSelectSave(string folderPath)
    {
        selectedSaveFolder = folderPath;
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
                ClearJsonContent();
            }
            RefreshSaveList();
            Debug.Log($"Deleted save: {folderPath}");
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
    }

    private void ClearJsonContent()
    {
        foreach (var text in jsonTextInstances)
        {
            Destroy(text);
        }
        jsonTextInstances.Clear();
    }
}