using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    private string userDataPath;
    private string fileSavePath;

    void Awake()
    {
        InitializePaths();
        EnsureDirectoryStructure();
    }

    /// <summary>
    /// Khởi tạo đường dẫn thư mục.
    /// </summary>
    private void InitializePaths()
    {
        userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        fileSavePath = Path.Combine(userDataPath, "FileSave");
    }

    /// <summary>
    /// Kiểm tra và tạo cấu trúc thư mục nếu chưa tồn tại.
    /// </summary>
    private void EnsureDirectoryStructure()
    {
        if (!Directory.Exists(userDataPath))
        {
            Directory.CreateDirectory(userDataPath);
            Debug.Log($"Created directory: {userDataPath}");
        }

        if (!Directory.Exists(fileSavePath))
        {
            Directory.CreateDirectory(fileSavePath);
            Debug.Log($"Created directory: {fileSavePath}");
        }
    }

    /// <summary>
    /// Tạo một thư mục save mới với định dạng SaveGame_UserName_DateSave_Index.
    /// </summary>
    public string CreateNewSaveFolder(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            userName = "PlayerNam_Null";

        string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int index = GetNextIndex(userName, dateSave);
        string folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
        string folderPath = Path.Combine(fileSavePath, folderName);

        Directory.CreateDirectory(folderPath);
        Debug.Log($"Created new save folder: {folderPath}");
        return folderPath;
    }


    /// <summary>
    /// Tính index tiếp theo cho thư mục save.
    /// </summary>
    private int GetNextIndex(string userName, string dateSave)
    {
        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_{dateSave}_"))
            .Select(d => int.Parse(Path.GetFileName(d).Split('_').Last()))
            .ToList();

        return folders.Any() ? folders.Max() + 1 : 1;
    }

    /// <summary>
    /// Lưu một file JSON vào thư mục save.
    /// </summary>
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
    /// Đọc một file JSON từ thư mục save.
    /// </summary>
    public string LoadJsonFile(string saveFolderPath, string fileName)
    {
        try
        {
            string filePath = Path.Combine(saveFolderPath, fileName);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Debug.Log($"Loaded JSON from: {filePath}");
                return json;
            }
            Debug.LogWarning($"File {fileName} not found in {saveFolderPath}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load JSON {fileName}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Trả về thư mục save có DateSave gần nhất.
    /// </summary>
    public string GetLatestSaveFolder()
    {
        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith("SaveGame_"))
            .OrderByDescending(d =>
            {
                string[] parts = Path.GetFileName(d).Split('_');
                if (parts.Length >= 4 && DateTime.TryParseExact(parts[2], "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    return date;
                return DateTime.MinValue;
            })
            .ToList();

        foreach (var folder in folders)
        {
            if (Directory.GetFiles(folder, "*.json").Length > 0)
                return folder;
        }

        Debug.LogWarning("No valid save folder found!");
        return null;
    }

    /// <summary>
    /// Trả về danh sách các thư mục save kèm đường dẫn ảnh.
    /// </summary>
    public List<(string FolderPath, string ImagePath)> GetAllSaveFolders()
    {
        var result = new List<(string, string)>();
        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith("SaveGame_"));

        foreach (var folder in folders)
        {
            string imagePath = Path.Combine(folder, "screenshot.png");
            if (!File.Exists(imagePath))
            {
                if (!TryCreateDefaultScreenshot(imagePath))
                    imagePath = null;
            }
            result.Add((folder, imagePath));
        }

        return result;
    }

    /// <summary>
    /// Tạo ảnh mặc định và lưu vào đường dẫn chỉ định. Trả về true nếu thành công, false nếu thất bại.
    /// </summary>
    private bool TryCreateDefaultScreenshot(string imagePath)
    {
        try
        {
            int width = 100, height = 100;
            Texture2D defaultTex = new Texture2D(width, height);
            Color32[] pixels = Enumerable.Repeat(new Color32(128, 128, 128, 255), width * height).ToArray();
            defaultTex.SetPixels32(pixels);
            defaultTex.Apply();
            byte[] png = defaultTex.EncodeToPNG();
            File.WriteAllBytes(imagePath, png);
            UnityEngine.Object.Destroy(defaultTex);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create default screenshot: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Xóa một thư mục save.
    /// </summary>
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

    // Chức năng mới: Nhân bản thư mục save
    public string DuplicateSaveFolder(string sourceFolderPath, string userName)
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError($"Source save folder does not exist: {sourceFolderPath}");
            return null;
        }

        string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int index = GetNextIndex(userName, dateSave);
        string newFolderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
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
}