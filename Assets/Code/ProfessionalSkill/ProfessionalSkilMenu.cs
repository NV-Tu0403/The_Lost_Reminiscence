using DuckLe;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Script.Procession;
using UnityEngine;
using System.Collections.Generic;
using Code.Procession;
using UnityEngine.UI;
using TMPro;


public class ProfessionalSkilMenu : MonoBehaviour
{
    public static ProfessionalSkilMenu Instance { get; private set; }

    /// <summary>
    /// hiện dùng để test lưu trữ thư mục save gần nhất đã chọn
    /// </summary>
    private string lastSelectedSaveFolder;  
    public string selectedSaveFolder;
    public GameObject Menu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {

        RegisterSaveables();
        CheckUserAccounts();
    }

    #region nghiệp vụ 1

    /// <summary>
    /// Đăng ký các đối tượng có thể lưu trữ dữ liệu vào SaveGameManager.
    /// </summary>
    private void RegisterSaveables()
    {
        SaveGameManager.Instance.RegisterSaveable(PlayTimeManager.Instance);
        SaveGameManager.Instance.RegisterSaveable(PlayerCheckPoint.Instance);
    }

    /// <summary>
    /// Kiểm tra xem có người dùng nào đã đăng nhập và có tài khoản lưu trữ không. Nếu không, hiển thị giao diện đăng nhập.
    /// </summary>
    private void CheckUserAccounts()
    {
        string userAccountsPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "UserAccounts.json");
        if (!File.Exists(userAccountsPath) ||
            JsonUtility.FromJson<UserAccountData>(File.ReadAllText(userAccountsPath)).Users.Count == 0)
        {
            //loginPanel.SetActive(true);
            lastSelectedSaveFolder = null;
            Debug.Log("[Test] No users found. Showing login panel.");
        }
        else
        {
            TryAutoLogin();
        }
    }

    /// <summary>
    /// Thử tự động đăng nhập người dùng nếu có thông tin đăng nhập hợp lệ.
    /// </summary>
    private void TryAutoLogin()
    {
        if (UserAccountManager.Instance.TryAutoLogin(out string errorMessage))
        {

            lastSelectedSaveFolder = GetValidLastSaveFolder();
            //ContinueGame_Bt.interactable = !string.IsNullOrEmpty(lastSelectedSaveFolder);
            //UpdateCurrentSaveText();
            PlayTimeManager.Instance.StartCounting();
        }
        else
        {
            lastSelectedSaveFolder = null;
        }
    }

    /// <summary>
    /// Lấy thư mục lưu trữ hợp lệ gần nhất từ UserAccountManager.
    /// </summary>
    /// <returns></returns>
    private string GetValidLastSaveFolder()
    {
        string lastFileSave = UserAccountManager.Instance.GetLastFileSave();
        if (!string.IsNullOrEmpty(lastFileSave))
        {
            string fileSavePath = Path.Combine(Application.persistentDataPath, "User_DataGame",
                $"FileSave_{UserAccountManager.Instance.CurrentUserBaseName}", lastFileSave);
            if (Directory.Exists(fileSavePath) && Directory.GetFiles(fileSavePath, "*.json").Length > 0)
            {
                return fileSavePath;
            }
        }

        var saves = SaveGameManager.Instance.GetAllSaveFolders(UserAccountManager.Instance.CurrentUserBaseName);
        string latestFolder = saves.Count > 0 ? saves[0].FolderPath : null;
        //ContinueGame_Bt.interactable = !string.IsNullOrEmpty(latestFolder);
        return latestFolder;
    }

    /// <summary>
    /// Làm mới danh sách các thư mục lưu trữ và trả về SaveListContext để cập nhật UI.
    /// </summary>
    /// <returns></returns>
    public SaveListContext RefreshSaveList()
    {
        // Kiểm tra null trước khi sử dụng
        if (UserAccountManager.Instance == null)
        {
            Debug.LogError("UserAccountManager.Instance is null!");
            return new SaveListContext { UserName = null, Saves = new List<SaveFolder>(), IsContinueEnabled = false };
        }
        if (SaveGameManager.Instance == null)
        {
            Debug.LogError("SaveGameManager.Instance is null!");
            return new SaveListContext { UserName = UserAccountManager.Instance.currentUserBaseName, Saves = new List<SaveFolder>(), IsContinueEnabled = false };
        }
        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            Debug.LogError("currentUserBaseName is null or empty!");
            // Wait 1 second and try again
            StartCoroutine(RetryRefreshSaveListAfterDelay());
            //return new SaveListContext { UserName = null, Saves = new List<SaveFolder>(), IsContinueEnabled = false };
        }

        var saves = SaveGameManager.Instance.GetAllSaveFolders(UserAccountManager.Instance.currentUserBaseName);
        bool isContinueEnabled = saves.Any();
        return new SaveListContext
        {
            UserName = UserAccountManager.Instance.currentUserBaseName,
            Saves = saves.Select(s => new SaveFolder { FolderPath = s.FolderPath, ImagePath = s.ImagePath }).ToList(),
            IsContinueEnabled = isContinueEnabled
        };
    }

    private IEnumerator RetryRefreshSaveListAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        RefreshSaveList();
    }

    private IEnumerator WaitUntilPlayerAndApply()
    {
        Transform p = null;
        while (p == null)
        {
            p = GameObject.FindGameObjectWithTag("Player")?.transform;
            yield return null;
        }

        //PlayerCheckPoint.Instance.SetPlayerTransform(p);
        PlayerCheckPoint.Instance.ApplyLoadedPosition();
        //Debug.Log("[WaitUntil] Player position applied.");
    }

    private IEnumerator LoadImageAsync(string imagePath, RawImage saveImage)
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        saveImage.texture = texture;
        yield return null;
    }

    //private void UpdateCurrentSaveText()
    //{
    //    if (currentSaveText == null) return;
    //    currentSaveText.text = string.IsNullOrEmpty(lastSelectedSaveFolder) ? "Current Save: None" : $"Current Save: {Path.GetFileName(lastSelectedSaveFolder)}";
    //}
    #endregion

    #region nghiep vụ 2

    public string OnNewGame()
    {

        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            Debug.LogError($"currentUserBaseName == {UserAccountManager.Instance.currentUserBaseName}");
        }

        string newSaveFolder = SaveGameManager.Instance.CreateNewSaveFolder(UserAccountManager.Instance.currentUserBaseName);

        PlayTimeManager.Instance.ResetSession();
        PlayTimeManager.Instance.StartCounting();

        if (SceneController.Instance == null)
        {
            Debug.LogError("[OnNewGame] SceneController.Instance is null!");
            return null;
        }

        if (PlayerCheckPoint.Instance == null)
        {
            Debug.LogError("[OnNewGame] PlayerCheckPoint.Instance is null!");
            return null;
        }

        // Load scene và chờ callback khi load xong
        SceneController.Instance.LoadAdditiveScene("Phong_scene", PlayerCheckPoint.Instance, () =>
        {
            //Đảm bảo Player đã tồn tại sau khi load scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[OnNewGame] Player not found after loading scene.");
                return;
            }
            
            // Gọi Procession để load dữ lieu tu GameProcession
            ProgressionManager.Instance.InitProgression();

            //án playerTransform
            PlayerCheckPoint.Instance.SetPlayerTransform(player.transform);
            //Đặt vị trí mặc định
            PlayerCheckPoint.Instance.ResetPlayerPositionWord();
            SaveGameManager.Instance.SaveToFolder(newSaveFolder);

            Core.Instance._menuCamera.SetActive(false);
            Menu.SetActive(false);
        });
        
        

        return newSaveFolder;
    }

    public void OnContinueGame(string saveFolder)
    {
        if (string.IsNullOrEmpty(saveFolder) || !Directory.Exists(saveFolder))
        {
            throw new Exception("Invalid save folder!");
        }

        SaveGameManager.Instance.LoadFromFolder(saveFolder);
        string sceneToLoad = PlayerCheckPoint.Instance.CurrentMap;
        if (string.IsNullOrEmpty(sceneToLoad) || sceneToLoad == "Unknown" || sceneToLoad == "Menu")
        {
            sceneToLoad = "Phong_scene"; // Default scene if none is set
        }

        SceneController.Instance.LoadAdditiveScene(sceneToLoad, PlayerCheckPoint.Instance, () =>
        {
            PlayTimeManager.Instance.StartCounting();
            PlayerCheckPoint.Instance.StartCoroutine(WaitUntilPlayerAndApply());
        });

        Core.Instance._menuCamera.SetActive(false);
        Menu.SetActive(false);
    }

    /// <summary>
    /// nhận vào đường dẫn thư mục để đánh dấu là thư mục đã chọn.
    /// load dữ liệu từ thư mục đó.
    /// </summary>
    /// <param name="folderPath"></param>
    public void OnSelectSave(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        selectedSaveFolder = folderPath;
        lastSelectedSaveFolder = folderPath;


        SaveGameManager.Instance.LoadFromFolder(folderPath);

        // Log all JSON file contents in the selected save folder
        var jsonFileHandler = new JsonFileHandler();
        var jsonFiles = jsonFileHandler.LoadJsonFiles(folderPath);
        foreach (var (fileName, json) in jsonFiles)
        {
            Debug.Log($"[ProfessionalSkilMenu] OnSelectSave - {fileName}:\n{json}");
        }

        //UpdateCurrentSaveText();
    }

    /// <summary>
    /// Xử lý khi người dùng nhấn xóa một thư mục lưu trữ.
    /// </summary>
    /// <param name="folderPath"></param>
    public void OnDeleteSave(string folderPath)
    {
        if (SaveGameManager.Instance.DeleteSaveFolder(folderPath))
        {
            if (folderPath == lastSelectedSaveFolder)
            {
                lastSelectedSaveFolder = null;
                selectedSaveFolder = null;
                //ContinueGame_Bt.interactable = false;
            }
        }
        else
        {
            //errorText.text = "Failed to delete save!";
            //errorText.color = Color.red;
            Debug.LogError($"[OnDeleteSave] Failed to delete save folder: {folderPath}");
        }
    }

    /// <summary>
    /// Lưu tất cả dữ liệu của người dùng hiện tại vào thư mục lưu trữ tương ứng.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void OnSaveSession()
    {
        if (string.IsNullOrEmpty(UserAccountManager.Instance.currentUserBaseName))
        {
            throw new Exception("No user logged in!");
        }
        SaveGameManager.Instance.SaveAll(UserAccountManager.Instance.currentUserBaseName);
    }

    public void OnQuitSession(string currentSaveFolder)
    {
        if (!string.IsNullOrEmpty(currentSaveFolder))
        {
            try
            {
                SaveGameManager.Instance.SaveToFolder(currentSaveFolder);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OnQuitSession] Failed to save before unloading: {ex.Message}");
            }
        }

        // Sau khi đã save xong mới unload
        SceneController.Instance.UnloadAllAdditiveScenes(() =>
        {
            Debug.Log("[OnQuitSession] Unload complete.");
            PlayerCheckPoint.Instance.ResetPlayerPositionWord();
        });


    }

    #endregion
}
