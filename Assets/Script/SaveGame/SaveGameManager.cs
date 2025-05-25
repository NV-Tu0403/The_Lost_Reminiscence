using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class UserAccount
{
    public string UserName;
    public string BaseName;
    public string TimeCheckIn;
    public string PasswordHash;
}

[System.Serializable]
public class UserAccountList
{
    public string LastAccount;
    public string LastFileSave;
    public double PlayTime;
    public List<UserAccount> Users = new List<UserAccount>();
}

public class SaveGameManager : MonoBehaviour
{
    private string userDataPath;
    private string userAccountsPath;
    private UserAccountList userAccounts;
    private string currentUserNamePlaying;

    public string CurrentUserNamePlaying
    {
        get => currentUserNamePlaying;
        set => currentUserNamePlaying = value;
    }

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
        currentUserNamePlaying = null; // Reset để đảm bảo trạng thái sạch
        InitializePaths();
        EnsureDirectoryStructure();
        LoadUserAccounts();
    }

    private void InitializePaths()
    {
        userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        userAccountsPath = Path.Combine(userDataPath, "UserAccounts.json");
        Debug.Log($"Initialized paths: userDataPath={userDataPath}, userAccountsPath={userAccountsPath}");
    }

    private void EnsureDirectoryStructure()
    {
        if (!Directory.Exists(userDataPath))
        {
            Directory.CreateDirectory(userDataPath);
            Debug.Log($"Created directory: {userDataPath}");
        }
    }

    private double ConvertLegacyPlayTime(string playTime)
    {
        if (string.IsNullOrEmpty(playTime))
        {
            Debug.LogWarning("Legacy PlayTime is empty. Returning 0.");
            return 0.0;
        }

        var parts = playTime.Split('.');
        if (parts.Length != 6 || !parts.All(p => int.TryParse(p, out _)))
        {
            Debug.LogWarning($"Invalid legacy PlayTime format: '{playTime}'. Returning 0.");
            return 0.0;
        }

        try
        {
            int[] values = parts.Select(int.Parse).ToArray();
            TimeSpan timeSpan = new TimeSpan(values[2], values[3], values[4], values[5]) +
                               TimeSpan.FromDays(values[1] * 30 + values[0] * 365);
            double seconds = timeSpan.TotalSeconds;
            Debug.Log($"Converted legacy PlayTime '{playTime}' to {seconds} seconds");
            return seconds;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert legacy PlayTime '{playTime}': {e.Message}. Returning 0.");
            return 0.0;
        }
    }

    private void LoadUserAccounts()
    {
        if (!File.Exists(userAccountsPath))
        {
            userAccounts = new UserAccountList
            {
                LastAccount = "",
                LastFileSave = "",
                PlayTime = 0.0
            };
            SaveUserAccounts();
            Debug.Log($"Created new UserAccounts.json at: {userAccountsPath}");
        }
        else
        {
            try
            {
                string json = File.ReadAllText(userAccountsPath);
                var tempJson = JsonUtility.FromJson<TempUserAccountList>(json);
                userAccounts = new UserAccountList
                {
                    LastAccount = tempJson.LastAccount,
                    LastFileSave = tempJson.LastFileSave,
                    Users = tempJson.Users
                };

                if (!string.IsNullOrEmpty(tempJson.PlayTimeString))
                {
                    userAccounts.PlayTime = ConvertLegacyPlayTime(tempJson.PlayTimeString);
                }
                else
                {
                    userAccounts.PlayTime = tempJson.PlayTime;
                }

                if (userAccounts.PlayTime < 0)
                {
                    Debug.LogWarning("PlayTime was negative. Resetting to 0.");
                    userAccounts.PlayTime = 0.0;
                }

                SaveUserAccounts();
                Debug.Log($"Loaded UserAccounts from: {userAccountsPath}, Users: {userAccounts.Users.Count}, LastAccount: {userAccounts.LastAccount}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load UserAccounts.json: {e.Message}");
                userAccounts = new UserAccountList
                {
                    LastAccount = "",
                    LastFileSave = "",
                    PlayTime = 0.0
                };
                SaveUserAccounts();
            }
        }
    }

    private void SaveUserAccounts()
    {
        try
        {
            if (File.Exists(userAccountsPath))
            {
                File.Copy(userAccountsPath, userAccountsPath + ".bak", true);
                Debug.Log("Created backup of UserAccounts.json");
            }

            string json = JsonUtility.ToJson(userAccounts, true);
            File.WriteAllText(userAccountsPath, json);
            Debug.Log($"Saved UserAccounts to: {userAccountsPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save UserAccounts.json: {e.Message}");
        }
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public bool InputUserAccount(string baseName, string password, out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrEmpty(baseName))
        {
            errorMessage = "UserName cannot be empty!";
            Debug.LogWarning(errorMessage);
            return false;
        }
        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "Password cannot be empty!";
            Debug.LogWarning(errorMessage);
            return false;
        }

        if (userAccounts.Users.Any(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = $"UserName '{baseName}' is already used!";
            Debug.LogWarning(errorMessage);
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

    public bool LoginUser(string baseName, string password, out string errorMessage)
    {
        errorMessage = "";
        var user = userAccounts.Users.FirstOrDefault(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            errorMessage = $"User '{baseName}' does not exist!";
            Debug.LogWarning(errorMessage);
            return false;
        }

        // Bỏ qua kiểm tra mật khẩu nếu password là null (cho tự động đăng nhập)
        if (!string.IsNullOrEmpty(password) && user.PasswordHash != HashPassword(password))
        {
            errorMessage = "Incorrect password!";
            Debug.LogWarning(errorMessage);
            return false;
        }

        currentUserNamePlaying = baseName;
        Debug.Log($"Logged in user: {baseName}");
        return true;
    }

    /// <summary>
    ///  tạo một thư mục lưu trữ mới cho người dùng với định dạng "SaveGame_{userName}_{dateSave}_{index:D3}"
    ///  trong thư mục này sẽ chứa các tệp JSON lưu trữ trò chơi.
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
        string folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
        string folderPath = Path.Combine(fileSavePath, folderName);

        Directory.CreateDirectory(folderPath);
        Debug.Log($"Created new save folder: {folderPath}");
        return folderPath;
    }

    private int GetNextIndex(string userName, string dateSave)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        if (!Directory.Exists(fileSavePath)) return 1;

        var folders = Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_{dateSave}_"))
            .Select(d => int.Parse(Path.GetFileName(d).Split('_').Last()))
            .ToList();

        int nextIndex = folders.Any() ? folders.Max() + 1 : 1;
        Debug.Log($"Next index for {userName} on {dateSave}: {nextIndex}");
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

    public void SaveLastSession(string lastAccount, string lastFileSave, double playTime)
    {
        if (playTime < 0)
        {
            Debug.LogWarning($"Invalid PlayTime: {playTime}. Setting to 0.");
            playTime = 0.0;
        }

        userAccounts.LastAccount = lastAccount;
        userAccounts.LastFileSave = lastFileSave;
        userAccounts.PlayTime = playTime;
        SaveUserAccounts();
        Debug.Log($"Saved session: LastAccount={lastAccount}, LastFileSave={lastFileSave}, PlayTime={playTime} seconds");
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

        Debug.Log($"Found {result.Count} save folders for user: {userName}");
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

    [Obsolete("This method is not used and may be removed in future versions.")]
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

    [Obsolete("This method is not used and may be removed in future versions.")]
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

    [System.Serializable]
    private class TempUserAccountList
    {
        public string LastAccount;
        public string LastFileSave;
        public double PlayTime;
        public string PlayTimeString;
        public List<UserAccount> Users = new List<UserAccount>();
    }
}