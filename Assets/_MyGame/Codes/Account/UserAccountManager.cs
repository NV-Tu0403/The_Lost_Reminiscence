using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UserAccount
{
    public string UserName; //"a_20250525_153959"
    public string BaseName; // "a"
    public string TimeCheckIn; // "20250525_153959"
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

    public void LoadUserData()
    {
        if (!File.Exists(userAccountsPath))
        {
            userData = new UserAccountData
            {
                LastAccount = "",
                LastFileSave = "",
                PlayTime = 0.0
            };

            CreateGuestAccount(); // Create default GuestAccount
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

                // If no users exist, create a GuestAccount
                if (userData.Users.Count == 0)
                {
                    CreateGuestAccount();
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
                CreateGuestAccount();
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

    /// <summary>
    /// Tạo tài khoản Guest mặc định khi không có tài khoản nào.
    /// </summary>
    private void CreateGuestAccount()
    {
        string baseName = "Guest";
        string timeCheckIn = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string userName = $"{baseName}_{timeCheckIn}";
        userData.Users.Add(new UserAccount
        {
            UserName = userName,
            BaseName = baseName,
            TimeCheckIn = timeCheckIn
        });
        userData.LastAccount = userName; // Set GuestAccount as the last account
        Debug.Log($"Created GuestAccount: {userName}");
    }

    /// <summary>
    /// Thiết lập tài khoản Cloud (lưu BaseName và tạo UserName nếu chưa tồn tại).
    /// Được gọi cùng với hàm đăng nhập/đăng ký thực sự từ script khác.
    /// </summary>
    public bool SetupLocalAccount(string baseName, out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrEmpty(baseName))
        {
            errorMessage = "BaseName cannot be empty.";
            return false;
        }

        var user = userData.Users.FirstOrDefault(u => u.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            string timeCheckIn = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string userName = $"{baseName}_{timeCheckIn}";
            userData.Users.Add(new UserAccount
            {
                UserName = userName,
                BaseName = baseName,
                TimeCheckIn = timeCheckIn
            });
            user = userData.Users.Last();
        }

        currentUserBaseName = baseName;
        userData.LastAccount = user.UserName;
        SaveUserData();
        Debug.Log($"Setup Cloud account: {baseName}");
        return true;
    }

    /// <summary>
    /// Tự động đăng nhập vào tài khoản cuối cùng đã sử dụng hoặc GuestAccount nếu không có tài khoản nào.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public bool TryAutoLoginLocal(out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrEmpty(userData.LastAccount))
        {
            // Try to find GuestAccount if no LastAccount is set
            var guestUser = userData.Users.FirstOrDefault(u => u.BaseName.Equals("Guest", StringComparison.OrdinalIgnoreCase));
            if (guestUser == null)
            {
                errorMessage = "No LastAccount or GuestAccount found.";
                return false;
            }
            currentUserBaseName = guestUser.BaseName;
            userData.LastAccount = guestUser.UserName;
            SaveUserData();
            Debug.Log($"Auto-logged in GuestAccount: {guestUser.BaseName}");
            return true;
        }

        var user = userData.Users.FirstOrDefault(u => u.UserName == userData.LastAccount);
        if (user == null)
        {
            // Fall back to GuestAccount if LastAccount is invalid
            var guestUser = userData.Users.FirstOrDefault(u => u.BaseName.Equals("Guest", StringComparison.OrdinalIgnoreCase));
            if (guestUser == null)
            {
                errorMessage = $"LastAccount '{userData.LastAccount}' not found and no GuestAccount available.";
                return false;
            }
            currentUserBaseName = guestUser.BaseName;
            userData.LastAccount = guestUser.UserName;
            SaveUserData();
            Debug.Log($"Auto-logged in GuestAccount: {guestUser.BaseName}");
            return true;
        }

        // Kiểm tra không có mạng và LastAccount không phải Guest (coi như AccountCloud)
        if (Application.internetReachability == NetworkReachability.NotReachable && !user.BaseName.Equals("Guest", StringComparison.OrdinalIgnoreCase))
        {
            var guestUser = userData.Users.FirstOrDefault(u => u.BaseName.Equals("Guest", StringComparison.OrdinalIgnoreCase));
            if (guestUser != null)
            {
                currentUserBaseName = guestUser.BaseName;
                userData.LastAccount = guestUser.UserName;
                SaveUserData();
                Debug.Log($"No internet, switched to GuestAccount: {guestUser.BaseName}");
                return true;
            }
            else
            {
                errorMessage = "No internet and no GuestAccount available.";
                return false;
            }
        }

        // Nếu đã đăng nhập vào một tài khoản khác, thì không cho phép tự động đăng nhập
        if (!string.IsNullOrEmpty(currentUserBaseName))
        {
            errorMessage = $"Already logged in as '{currentUserBaseName}'.";
            return false;
        }

        currentUserBaseName = user.BaseName;
        Debug.Log($"Auto-logged in user: {user.BaseName}");
        return true;
    }

    public bool LoginGuest(string Username)
    {
        currentUserBaseName = Username;
        userData.LastAccount = Username;
        SaveUserData();
        Debug.Log($"Auto-logged in GuestAccount: {Username}");
        return true;
    }

    public bool Logout(out string errorMessage)
    {
        errorMessage = "";
        currentUserBaseName = null;
        return true;
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

    /// <summary>
    /// Lấy Username đầy đủ (BaseName_TimeCheckIn) để tạo file save.
    /// </summary>
    public string GetUsernameForSave()
    {
        if (string.IsNullOrEmpty(currentUserBaseName))
        {
            Debug.LogWarning("No current user logged in.");
            return "";
        }

        var user = userData.Users.FirstOrDefault(u => u.BaseName.Equals(currentUserBaseName, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            Debug.LogWarning($"User with BaseName '{currentUserBaseName}' not found.");
            return "";
        }

        return user.UserName;
    }
}