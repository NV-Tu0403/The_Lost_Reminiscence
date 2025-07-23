using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[System.Serializable]
public class UserAccount
{
    public string UserName; //"a_20250525_153959"
    public string BaseName; // "a"
    public string TimeCheckIn; // "20250525_153959"
    public string PasswordHash;
    public bool SyncToServer = false; // Mặc định là false
}

[System.Serializable]
public class UserAccountData
{
    public string LastAccount;
    public string LastFileSave;
    public double PlayTime;
    public List<UserAccount> Users = new List<UserAccount>();
}

[System.Serializable]
public class TempUserAccountData
{
    public string LastAccount;
    public string LastFileSave;
    public double PlayTime;
    public string PlayTimeString;
    public List<UserAccount> Users = new List<UserAccount>();
}

public class UserAccountManager : MonoBehaviour
{
    public static UserAccountManager Instance { get; private set; }

    private string userDataPath;
    private string userAccountsPath;
    private UserAccountData userData;
    public string currentUserBaseName;

    public string CurrentUserBaseName => currentUserBaseName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of UserAccountManager detected. Destroying duplicate.");
        }

        userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        userAccountsPath = Path.Combine(userDataPath, "UserAccounts.json");
        currentUserBaseName = null;
        EnsureDirectory();
        LoadUserData();
    }

    private void EnsureDirectory()
    {
        if (!Directory.Exists(userDataPath))
        {
            Directory.CreateDirectory(userDataPath);
            Debug.Log($"Created directory: {userDataPath}");
        }
    }

    private void LoadUserData()
    {
        if (!File.Exists(userAccountsPath))
        {
            userData = new UserAccountData
            {
                LastAccount = "",
                LastFileSave = "",
                PlayTime = 0.0
            };
            SaveUserData();
            Debug.Log($"Created new UserAccounts.json at: {userAccountsPath}");
        }
        else
        {
            try
            {
                string json = File.ReadAllText(userAccountsPath);
                var tempData = JsonUtility.FromJson<TempUserAccountData>(json);
                userData = new UserAccountData
                {
                    LastAccount = tempData.LastAccount,
                    LastFileSave = tempData.LastFileSave,
                    Users = tempData.Users
                };

                if (!string.IsNullOrEmpty(tempData.PlayTimeString))
                {
                    userData.PlayTime = ConvertLegacyPlayTime(tempData.PlayTimeString);
                }
                else
                {
                    userData.PlayTime = tempData.PlayTime;
                }

                if (userData.PlayTime < 0)
                {
                    Debug.LogWarning("PlayTime was negative. Resetting to 0.");
                    userData.PlayTime = 0.0;
                }

                SaveUserData();
                //Debug.Log($"Loaded UserAccounts: Users={userData.Users.Count}, LastAccount={userData.LastAccount}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load UserAccounts.json: {e.Message}");
                userData = new UserAccountData
                {
                    LastAccount = "",
                    LastFileSave = "",
                    PlayTime = 0.0
                };
                SaveUserData();
            }
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
            return timeSpan.TotalSeconds;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert legacy PlayTime '{playTime}': {e.Message}. Returning 0.");
            return 0.0;
        }
    }

    private void SaveUserData()
    {
        try
        {
            if (File.Exists(userAccountsPath))
            {
                File.Copy(userAccountsPath, userAccountsPath + ".bak", true);
                //Debug.Log("Created backup of UserAccounts.json");
            }

            string json = JsonUtility.ToJson(userData, true);
            File.WriteAllText(userAccountsPath, json);
            //Debug.Log($"Saved UserAccounts to: {userAccountsPath}");
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

    public bool CreateAccount(string baseName, string password, out string errorMessage)
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

        if (userData.Users.Any(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = $"UserName '{baseName}' is already used!";
            return false;
        }

        string timeCheckIn = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string userName = $"{baseName}_{timeCheckIn}";
        userData.Users.Add(new UserAccount
        {
            UserName = userName,
            BaseName = baseName,
            TimeCheckIn = timeCheckIn,
            PasswordHash = HashPassword(password),
            SyncToServer = false
        });
        SaveUserData();

        currentUserBaseName = baseName;
        Debug.Log($"Created account: {userName}");
        return true;
    }

    public bool Login(string baseName, string password, out string errorMessage)
    {
        errorMessage = "";
        var user = userData.Users.FirstOrDefault(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            errorMessage = $"User '{baseName}' does not exist!";
            return false;
        }

        if (!string.IsNullOrEmpty(password) && user.PasswordHash != HashPassword(password))
        {
            errorMessage = "Incorrect password!";
            return false;
        }

        currentUserBaseName = baseName;
        userData.LastAccount = user.UserName;
        SaveUserData();
        Debug.Log($"Logged in user: {baseName}");
        return true;
    }

    public bool TryAutoLogin(out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrEmpty(userData.LastAccount))
        {
            errorMessage = "No LastAccount found.";
            return false;
        }

        var user = userData.Users.FirstOrDefault(u => u.UserName == userData.LastAccount);
        if (user == null)
        {
            errorMessage = $"LastAccount '{userData.LastAccount}' not found.";
            return false;
        }

        if (!string.IsNullOrEmpty(currentUserBaseName))
        {
            errorMessage = $"Already logged in as '{currentUserBaseName}'.";
            return false;
        }

        currentUserBaseName = user.BaseName;
        //Debug.Log($"Auto-logged in user: {user.BaseName}");
        return true;
    }

    public bool Logout(out string errorMessage)
    {
        errorMessage = "";
        currentUserBaseName = null;
        return true;
    }

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có kết nối với máy chủ chưa.
    /// trả về true nếu đã đồng bộ, false nếu chưa đồng bộ hoặc không tìm thấy người dùng.
    /// </summary>
    /// <returns></returns>
    public bool IsSynced(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("CurrentUserName is null or empty.");
            return false;
        }
        var user = userData.Users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            Debug.LogWarning($"User '{userName}' not found.");
            return false;
        }
        return user.SyncToServer;
    }

    /// <summary>
    /// Đánh dấu người dùng là đã đồng bộ với máy chủ.
    /// </summary>
    /// <param name="userName"></param>
    public bool MarkAsSynced(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("UserName is null or empty.");
            return false;
        }
        var user = userData.Users.FirstOrDefault(u => u.UserName == userName);
        if (user != null)
        {
            user.SyncToServer = true;
            SaveUserData();
        }
        return user.SyncToServer;
    }

    public void UpdateLastSession(string lastFileSave, double playTime)
    {
        if (playTime < 0)
        {
            Debug.LogWarning($"Invalid PlayTime: {playTime}. Setting to 0.");
            playTime = 0.0;
        }

        userData.LastFileSave = lastFileSave;
        userData.PlayTime = playTime;
        SaveUserData();
        Debug.Log($"Updated session: LastFileSave={lastFileSave}, PlayTime={playTime} seconds");
    }

    public double GetPlayTime()
    {
        return userData.PlayTime;
    }

    public string GetLastFileSave()
    {
        return userData.LastFileSave;
    }
}