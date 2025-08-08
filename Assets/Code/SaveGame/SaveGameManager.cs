using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveFolder
{
    public string FolderPath { get; set; }
    public string ImagePath { get; set; }  // Nếu bạn lưu ảnh đại diện cho save
}

public class SaveListContext
{
    public string UserName { get; set; }
    public List<SaveFolder> Saves { get; set; }
    public bool IsContinueEnabled { get; set; }
}

public class SaveGameManager : MonoBehaviour
{
    private readonly object saveablesLock = new object();
    private readonly HashSet<ISaveable> saveables = new HashSet<ISaveable>();
    public static SaveGameManager Instance { get; private set; }

    private FolderManager folderManager;
    private JsonFileHandler jsonFileHandler;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("SaveGameManager ==  oke");
        }

        string userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        folderManager = new FolderManager(userDataPath);
        jsonFileHandler = new JsonFileHandler();
    }

    /// <summary>
    /// Gọi hàm này và truyền vào Class x để đăng ký một đối tượng có thể lưu trữ dữ liệu.
    /// </summary>
    /// <param name="saveable"></param>
    public void RegisterSaveable(ISaveable saveable)
    {
        if (saveable == null || string.IsNullOrEmpty(saveable.FileName)) return;
        lock (saveablesLock)
        {
            saveables.Add(saveable);
        }
    }

    public void UnregisterSaveable(ISaveable saveable)
    {
        if (saveable == null) return;
        lock (saveablesLock)
        {
            saveables.Remove(saveable);
        }
    }

    public void SaveToFolder(string folderPath)
    {
        string tempFolderPath = Path.Combine(Application.persistentDataPath, $"TempSave_{Guid.NewGuid()}");
        string backupFolderPath = folderPath + "_backup";

        try
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Move(folderPath, backupFolderPath); // sao lưu
            }

            if (!SaveAllToTempFolder(tempFolderPath))
                throw new Exception("One or more saveable objects failed to save.");

            Directory.CreateDirectory(folderPath);
            MoveFiles(tempFolderPath, folderPath);

            if (Directory.Exists(backupFolderPath))
                Directory.Delete(backupFolderPath, true);

            Debug.Log($"[SaveGameManager] Successfully saved to {folderPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveGameManager] Save failed: {ex.Message}");
            RollbackSave(tempFolderPath, folderPath, backupFolderPath);
        }
    }

    public void SaveAll(string userName, string folderPath = null)
    {
        string saveFolderPath = folderPath ?? folderManager.CreateNewSaveFolder(userName);
        if (saveFolderPath == null) return;
        SaveToFolder(saveFolderPath);
    }

    public void LoadLatest(string userName)
    {
        string latestFolder = folderManager.GetLatestSaveFolder(userName);
        if (latestFolder == null) return;

        var jsonFiles = jsonFileHandler.LoadJsonFiles(latestFolder);
        foreach (var (fileName, json) in jsonFiles)
        {
            var saveable = saveables.FirstOrDefault(s => s.FileName == fileName);
            if (saveable != null)
            {
                saveable.LoadFromJson(json);
                saveable.AfterLoad();
            }
        }
    }

    public void LoadFromFolder(string folderPath)
    {
        var jsonFiles = jsonFileHandler.LoadJsonFiles(folderPath);
        foreach (var (fileName, json) in jsonFiles)
        {
            var saveable = saveables.FirstOrDefault(s => s.FileName == fileName);
            if (saveable != null)
            {
                saveable.LoadFromJson(json);
                saveable.AfterLoad();
            }
        }
    }

    private bool SaveAllToTempFolder(string tempFolderPath)
    {
        bool hasError = false;
        Directory.CreateDirectory(tempFolderPath);

        lock (saveablesLock)
        {
            foreach (var saveable in saveables)
            {
                if (!saveable.ShouldSave()) continue;
                saveable.BeforeSave();
                var json = saveable.SaveToJson();
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning($"[SaveGameManager] Failed to serialize {saveable.FileName}");
                    hasError = true;
                    continue;
                }
                jsonFileHandler.SaveJsonFile(tempFolderPath, saveable.FileName, json);
            }
        }

        return !hasError;
    }

    private void MoveFiles(string sourceFolder, string destFolder)
    {
        foreach (var file in Directory.GetFiles(sourceFolder))
        {
            string dest = Path.Combine(destFolder, Path.GetFileName(file));
            File.Move(file, dest);
        }
        Directory.Delete(sourceFolder, true);
    }

    private void RollbackSave(string tempFolder, string saveFolder, string backupFolder)
    {
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);
        if (Directory.Exists(saveFolder))
            Directory.Delete(saveFolder, true);
        if (Directory.Exists(backupFolder))
            Directory.Move(backupFolder, saveFolder);
    }

    public string GetLatestSaveFolder(string userName) => folderManager.GetLatestSaveFolder(userName);
    public List<(string FolderPath, string ImagePath)> GetAllSaveFolders(string userName) => folderManager.GetAllSaveFolders(userName);
    public bool DeleteSaveFolder(string folderPath) => folderManager.DeleteSaveFolder(folderPath);
    public string CreateNewSaveFolder(string userName) => folderManager.CreateNewSaveFolder(userName);
}