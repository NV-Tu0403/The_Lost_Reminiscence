using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    private string userDataPath;

    [SerializeField] private ProgressionManager progressionManager;

    void Awake()
    {
        userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        //Debug.Log($"SaveGameManager initialized: {userDataPath}");
    }

    private string GetTransferFolder()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "Loc_Backend/SavePath");
#else
        return Path.Combine(Application.persistentDataPath, "SavePath");
#endif
    }

    public string CreateNewSaveFolder(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("UserName is empty. Cannot create save folder!");
            return null;
        }

        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath))
        {
            Directory.CreateDirectory(fileSavePath);
            Debug.Log($"Created FileSave directory: {fileSavePath}");
        }

        string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int index = GetNextIndex(userName);
        string folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
        string folderPath = Path.Combine(fileSavePath, folderName);

        if (Directory.Exists(folderPath))
        {
            Debug.LogWarning($"Save folder already exists: {folderPath}. Generating new index.");
            index = GetNextIndex(userName);
            folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
            folderPath = Path.Combine(fileSavePath, folderName);
        }

        Directory.CreateDirectory(folderPath);
        Debug.Log($"Created new save folder: {folderPath}");
        return folderPath;
    }

    private int GetNextIndex(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath)) return 1;

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
            .Select(d =>
            {
                string[] parts = Path.GetFileName(d).Split('_');
                if (parts.Length == 4 && int.TryParse(parts[3], out int idx))
                    return idx;
                return 0;
            })
            .ToList();

        int nextIndex = folders.Any() ? folders.Max() + 1 : 1;
        Debug.Log($"Next index for {userName}: {nextIndex}");
        return nextIndex;
    }

    public void SaveJsonFile(string saveFolderPath, string fileName, string jsonContent)
    {
        try
        {
            string filePath = Path.Combine(saveFolderPath, fileName);
            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"Saved JSON to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save JSON {fileName}: {e.Message}");
        }
    }

    /// <summary>
    /// chỉ chuyên để đọc file JSON
    /// trả về mảng các file JSON trong thư mục save đươc chọn
    /// </summary>
    /// <param name="saveFolderPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public (string fileName, string json)[] LoadJsonFiles(string folderPath)
    {
        var result = new List<(string fileName, string json)>(); // mảng kết quả chứa tên file và nội dung JSON
        try
        {
            var jsonFiles = Directory.GetFiles(folderPath, "*.json"); // lấy tất cả file JSON trong thư mục
            if (jsonFiles.Length == 0)
            {
                Debug.LogWarning($"No JSON files found in {folderPath}");
                return result.ToArray();
            }

            foreach (var file in jsonFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    string fileName = Path.GetFileName(file);
                    result.Add((fileName, json));
                    //Debug.Log($"Loaded JSON from: {file}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to read JSON file {file}: {ex.Message}");
                }
            }
            var jsonArray = result.ToArray();

            ProcessJsonFiles(jsonArray);
            return jsonArray;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load JSON files in {folderPath}: {e.Message}");
            return result.ToArray();
        }
    }

    /// <summary>
    /// Gán dữ liệu/Xử lý các file JSON đã đọc từ thư mục save
    /// </summary>
    /// <param name="files"></param>
    public void ProcessJsonFiles((string fileName, string json)[] files)
    {
        foreach (var (fileName, json) in files)
        {
            switch (fileName)
            {
                
                case "playerProgression.json":
                    progressionManager.LoadProgression(json);
                    break;

                case "gameState.json":
                    // gameStateManager.LoadFromJson(json);
                    break;

                case "inventory.json":
                    // inventoryManager.LoadInventory(json);
                    break;

                default:
                    Debug.LogWarning($"Unhandled JSON file: {fileName}");
                    break;
            }
        }
    }


    public string GetLatestSaveFolder(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath))
        {
            Debug.LogWarning($"No FileSave directory for user: {userName}");
            return null;
        }

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_") && Directory.GetFiles(d, "*.json").Length > 0)
            .Select(d => new
            {
                Path = d,
                Name = Path.GetFileName(d)
            })
            .OrderByDescending(d =>
            {
                string[] parts = d.Name.Split('_');
                if (parts.Length >= 4 && DateTime.TryParseExact(parts[2], "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    int index = int.Parse(parts[3]);
                    return new DateTime(date.Ticks + index);
                }
                return DateTime.MinValue;
            })
            .ToList();

        string latestFolder = folders.FirstOrDefault()?.Path;
        if (latestFolder != null)
        {
            Debug.Log($"Found latest save folder for user {userName}: {latestFolder}");
            return latestFolder;
        }

        Debug.LogWarning($"No valid save folder found for user: {userName}");
        return null;
    }

    public List<(string FolderPath, string ImagePath)> GetAllSaveFolders(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        var result = new List<(string, string)>();
        if (!Directory.Exists(fileSavePath))
        {
            Debug.LogWarning($"No FileSave directory for user: {userName}");
            return result;
        }

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"));

        foreach (var folder in folders)
        {
            string imagePath = Path.Combine(folder, "screenshot.png");
            string image = File.Exists(imagePath) ? imagePath : null;
            result.Add((folder, image));
        }

        //Debug.Log($"Found {result.Count} save folders for user: {userName}");
        return result;
    }

    public bool DeleteSaveFolder(string folderPath)
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                Debug.Log($"Deleted save folder: {folderPath}");
                return true;
            }
            Debug.LogWarning($"Save folder not found: {folderPath}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save folder {folderPath}: {e.Message}");
            return false;
        }
    }

    public string DuplicateSaveFolder(string sourceFolderPath, string userName)
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError($"Source save folder does not exist: {sourceFolderPath}");
            return null;
        }

        string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int index = GetNextIndex(userName);
        string newFolderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        string newFolderPath = Path.Combine(fileSavePath, newFolderName);

        try
        {
            Directory.CreateDirectory(newFolderPath);
            foreach (string file in Directory.GetFiles(sourceFolderPath))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(newFolderPath, fileName);
                File.Copy(file, destFile);
            }
            Debug.Log($"Duplicated save from {sourceFolderPath} to {newFolderPath}");
            return newFolderPath;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to duplicate save folder: {e.Message}");
            return null;
        }
    }

    public bool SyncFileSave(string sourceFolderPath, out string errorMessage)
    {
        errorMessage = "";
        if (!Directory.Exists(sourceFolderPath))
        {
            errorMessage = $"Source save folder does not exist: {sourceFolderPath}";
            Debug.LogError(errorMessage);
            return false;
        }

        string savePath = GetTransferFolder();

        try
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Debug.Log($"Created SavePath directory: {savePath}");
            }

            string folderName = Path.GetFileName(sourceFolderPath);
            string destFolderPath = Path.Combine(savePath, folderName);

            if (Directory.Exists(destFolderPath))
            {
                Directory.Delete(destFolderPath, true);
                Debug.Log($"Deleted existing destination folder: {destFolderPath}");
            }

            DirectoryCopy(sourceFolderPath, destFolderPath, true);
            Debug.Log($"Synced entire folder from {sourceFolderPath} to {destFolderPath}");
            return true;
        }
        catch (Exception e)
        {
            errorMessage = $"Failed to sync folder: {e.Message}";
            Debug.LogError(errorMessage);
            return false;
        }
    }

    private void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);
        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDir);

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDir, file.Name);
            file.CopyTo(tempPath, false);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDir, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }
}