using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Quản lý lưu trữ và tải tài khoản người dùng.(cần tách ra)
/// </summary>
[System.Serializable]
public class UserAccount
{
    public string UserName;
    public string BaseName;
    public string TimeCheckIn;
    public string PasswordHash;
}

/// <summary>
/// Lớp chứa danh sách tài khoản người dùng.(cần tách ra)
/// </summary>
[System.Serializable]
public class UserAccountList
{
    public List<UserAccount> Users = new List<UserAccount>();
}

/// <summary>
/// hệ thống quản lý Save game.
/// </summary>
public class SaveGameManager : MonoBehaviour
{
    private string userDataPath;
    private string userAccountsPath;
    private UserAccountList userAccounts;
    private string currentUserNamePlaying;

    /// <summary>
    /// Đường dẫn đến thư mục chứa dữ liệu người dùng.
    /// </summary>
    public string CurrentUserNamePlaying
    {
        get => currentUserNamePlaying;
        set => currentUserNamePlaying = value;
    }
    /// <summary>
    /// đương dẫn đến thư mục chứa dữ liệu người dùng tạm thời.
    /// </summary>
    /// <returns></returns>
    string GetTransferFolder()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "Loc_Backend/SavePath");
#else
        return Path.Combine(Application.persistentDataPath, "SavePath");
#endif
    }

    void Awake()
    {
        InitializePaths();
        EnsureDirectoryStructure();
        LoadUserAccounts();
    }

    /// <summary>
    /// khởi tạo đường dẫn cho các tệp tin
    /// </summary>
    private void InitializePaths()
    {
        userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        userAccountsPath = Path.Combine(userDataPath, "UserAccounts.json");
    }

    /// <summary>
    /// Đảm bảo cấu trúc thư mục tồn tại
    /// </summary>
    private void EnsureDirectoryStructure()
    {
        if (!Directory.Exists(userDataPath))
        {
            Directory.CreateDirectory(userDataPath);
            Debug.Log($"Created directory: {userDataPath}");
        }
    }

    /// <summary>
    /// Tải danh sách tài khoản người dùng từ tệp JSON
    /// </summary>
    private void LoadUserAccounts()
    {
        if (!File.Exists(userAccountsPath))
        {
            userAccounts = new UserAccountList();
            SaveUserAccounts();
            Debug.Log($"Created new UserAccounts.json at: {userAccountsPath}");
        }
        else
        {
            try
            {
                string json = File.ReadAllText(userAccountsPath);
                userAccounts = JsonUtility.FromJson<UserAccountList>(json); // Chuyển đổi JSON thành danh sách tài khoản người dùng
                Debug.Log($"Loaded UserAccounts from: {userAccountsPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load UserAccounts.json: {e.Message}");
                userAccounts = new UserAccountList(); // Nếu có lỗi, tạo danh sách mới
                SaveUserAccounts();
            }
        }
    }

    /// <summary>
    /// Lưu danh sách tài khoản người dùng vào tệp JSON
    /// </summary>
    private void SaveUserAccounts()
    {
        try
        {
            string json = JsonUtility.ToJson(userAccounts, true); // Chuyển đổi danh sách thành JSON
            File.WriteAllText(userAccountsPath, json);
            Debug.Log($"Saved UserAccounts to: {userAccountsPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save UserAccounts.json: {e.Message}");
        }
    }

    // Mã hóa mật khẩu bằng SHA256 (cần tách ra)
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// tạo tài khoản người dùng mới (cần tách ra)
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="password"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public bool InputUserAccount(string baseName, string password, out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrEmpty(baseName))
        {
            errorMessage = "UserName cannot be empty!";
            return false;
        }
        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "Password cannot be empty!";
            return false;
        }

        if (userAccounts.Users.Any(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = $"UserName '{baseName}' is already used!";
            return false;
        }

        string timeCheckIn = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string userName = $"{baseName}_{timeCheckIn}";
        userAccounts.Users.Add(new UserAccount
        {
            UserName = userName,
            BaseName = baseName,
            TimeCheckIn = timeCheckIn,
            PasswordHash = HashPassword(password)
        });
        SaveUserAccounts();

        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{baseName}");
        if (!Directory.Exists(fileSavePath))
        {
            Directory.CreateDirectory(fileSavePath);
            Debug.Log($"Created FileSave directory: {fileSavePath}");
        }

        currentUserNamePlaying = baseName;
        Debug.Log($"Added new user: {userName}, CurrentUserNamePlaying: {currentUserNamePlaying}");
        return true;
    }

    /// <summary>
    /// Đăng nhập người dùng (cần tách ra)
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="password"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public bool LoginUser(string baseName, string password, out string errorMessage)
    {
        errorMessage = "";
        var user = userAccounts.Users.FirstOrDefault(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            errorMessage = $"User '{baseName}' does not exist!";
            return false;
        }

        if (user.PasswordHash != HashPassword(password))
        {
            errorMessage = "Incorrect password!";
            return false;
        }

        currentUserNamePlaying = baseName;
        Debug.Log($"Logged in user: {baseName}");
        return true;
    }

    /// <summary>
    /// Tạo thư mục lưu trữ mới cho người dùng
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string CreateNewSaveFolder(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath))
        {
            Directory.CreateDirectory(fileSavePath);
            Debug.Log($"Created FileSave directory: {fileSavePath}");
        }

        string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int index = GetNextIndex(userName, dateSave);
        string folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}"; // Thay đổi định dạng tên thư mục
        string folderPath = Path.Combine(fileSavePath, folderName);

        Directory.CreateDirectory(folderPath); // Tạo thư mục lưu trữ mới
        Debug.Log($"Created new save folder: {folderPath}");
        return folderPath;
    }

    /// <summary>
    /// Lấy chỉ số tiếp theo cho thư mục lưu trữ 
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="dateSave"></param>
    /// <returns></returns>
    private int GetNextIndex(string userName, string dateSave)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath)) return 1;

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_{dateSave}_"))
            .Select(d => int.Parse(Path.GetFileName(d).Split('_').Last()))
            .ToList();

        return folders.Any() ? folders.Max() + 1 : 1;
    }

    /// <summary>
    /// Lưu tệp JSON vào thư mục lưu trữ
    /// </summary>
    /// <param name="saveFolderPath"></param>
    /// <param name="fileName"></param>
    /// <param name="jsonContent"></param>
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
    /// Tải tệp JSON từ thư mục lưu trữ
    /// </summary>
    /// <param name="saveFolderPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
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
    /// Lấy thư file save mới nhất cho người dùng (đang lỗi)
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string GetLatestSaveFolder(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath)) return null;

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
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

        Debug.LogWarning($"No valid save folder found for user: {userName}");
        return null;
    }

    /// <summary>
    /// Lấy tất cả các file save cho người dùng
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public List<(string FolderPath, string ImagePath)> GetAllSaveFolders(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        var result = new List<(string, string)>();
        if (!Directory.Exists(fileSavePath)) return result;

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"));

        foreach (var folder in folders)
        {
            string imagePath = Path.Combine(folder, "screenshot.png");
            string image = File.Exists(imagePath) ? imagePath : null;
            result.Add((folder, image));
        }

        return result;
    }

    /// <summary>
    /// Xóa file save được chọn
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Nhân bản file save được chọn
    /// </summary>
    /// <param name="sourceFolderPath"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Lấy đường dẫn thư mục lưu trữ cho người dùng (chưa dúng)
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string GetFileSavePathForUser(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (Directory.Exists(fileSavePath))
        {
            Debug.Log($"Found FileSave path for user {userName}: {fileSavePath}");
            return fileSavePath;
        }
        Debug.LogWarning($"FileSave path not found for user: {userName}");
        return null;
    }

    /// <summary>
    /// Lấy tên người dùng mới nhất từ danh sách tài khoản
    /// </summary>
    /// <returns></returns>
    public string GetLatestUser()
    {
        if (userAccounts.Users.Count == 0)
        {
            Debug.LogWarning("No users found in UserAccounts.json!");
            return null;
        }

        var latestUser = userAccounts.Users
            .OrderByDescending(u => DateTime.ParseExact(u.TimeCheckIn, "yyyyMMdd_HHmmss", null))
            .FirstOrDefault();

        if (latestUser != null)
        {
            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{latestUser.BaseName}");
            if (Directory.Exists(fileSavePath))
            {
                currentUserNamePlaying = latestUser.BaseName;
                Debug.Log($"Selected latest user: {latestUser.UserName}, BaseName: {latestUser.BaseName}");
                return latestUser.BaseName;
            }
        }

        Debug.LogWarning("No matching FileSave folder found for latest user!");
        return null;
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

    public void Logout()
    {
        currentUserNamePlaying = null;
        Debug.Log("User logged out. CurrentUserNamePlaying reset.");
    }
}