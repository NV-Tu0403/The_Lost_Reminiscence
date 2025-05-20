using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class SaveGameUI : MonoBehaviour
{
    [SerializeField] private SaveGameManager saveGameManager;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button duplicateButton;
    [SerializeField] private GameObject saveListPanel;
    [SerializeField] private GameObject jsonContentPanel; // Panel hiển thị nội dung JSON
    [SerializeField] private GameObject saveItemTemplate;
    [SerializeField] private GameObject jsonTextTemplate; // Prefab cho UI Text hiển thị JSON

    private List<GameObject> saveItemInstances = new List<GameObject>();
    private List<GameObject> jsonTextInstances = new List<GameObject>();
    private string selectedSaveFolder; // Lưu thư mục save được chọn

    void Start()
    {
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        duplicateButton.onClick.AddListener(OnDuplicateButtonClicked);
        RefreshSaveList();
    }

    private void OnContinueButtonClicked()
    {
        string latestSave = saveGameManager.GetLatestSaveFolder();
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
            return;
        }

        string newSaveFolder = saveGameManager.DuplicateSaveFolder(selectedSaveFolder, "DefaultUser");
        if (!string.IsNullOrEmpty(newSaveFolder))
        {
            RefreshSaveList();
            Debug.Log($"Duplicated save to: {newSaveFolder}");
        }
    }

    private void RefreshSaveList()
    {
        foreach (var item in saveItemInstances)
        {
            Destroy(item);
        }
        saveItemInstances.Clear();

        var saves = saveGameManager.GetAllSaveFolders();
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

    /// <summary>
    /// Hiển thị nội dung JSON trong thư mục save được chọn.
    /// </summary>
    /// <param name="folderPath"></param>
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