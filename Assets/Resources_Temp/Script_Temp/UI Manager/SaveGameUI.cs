using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

public class SaveGameUI : MonoBehaviour
{
    [SerializeField] private SaveGameManager saveGameManager;
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
        if (saveGameManager == null || progressionManager == null)
        {
            Debug.LogError("SaveGameManager or ProgressionManager is not assigned!");
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

        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
        syncPanelStatus.SetActive(false);
        continueButton.interactable = false;
        newGameButton.interactable = false;
        duplicateButton.interactable = false;
        syncButton.interactable = false;
        logoutButton.interactable = false;
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
        loginPanel.SetActive(true);
        errorText.text = "";
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

        yield return new WaitForSeconds(0.5f); // Giả lập thời gian xử lý

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
        syncPanelStatus.SetActive(false);
        continueButton.interactable = false;
        newGameButton.interactable = false;
        duplicateButton.interactable = false;
        syncButton.interactable = false;
        logoutButton.interactable = false;
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

    private void SyncWithMongoDB(string folderPath)
    {
        Debug.Log($"Syncing save {folderPath} with MongoDB...");
        var jsonFiles = Directory.GetFiles(folderPath, "*.json");
        foreach (var file in jsonFiles)
        {
            string json = saveGameManager.LoadJsonFile(folderPath, Path.GetFileName(file));
            Debug.Log($"JSON content for {Path.GetFileName(file)}: {json}");
        }
    }
}