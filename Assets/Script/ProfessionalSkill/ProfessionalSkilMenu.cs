using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfessionalSkilMenu : MonoBehaviour
{
    public static ProfessionalSkilMenu Instance { get; private set; }

    private string lastSelectedSaveFolder;
    private List<GameObject> saveItemInstances = new List<GameObject>();
    private string selectedSaveFolder;
    private float lastNewGameTime;
    private const float NEW_GAME_COOLDOWN = 1f;
    private float lastPlayTimeUpdate;
    private const float PLAY_TIME_UPDATE_INTERVAL = 1f;
    private Coroutine resetSaveSelectionCoroutine;

    private void Awake()
    {
        if (Instance == null )
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task OnQuitSesion()
    {
        try
        {
            await OnSaveSession();

            var loadedScenes = SceneController.Instance.GetLoadedAdditiveScenes();
            Debug.Log($"[Test] Loaded scenes before unload: {string.Join(", ", loadedScenes)}");

            // Unload tất cả scene additive
            await SceneController.Instance.UnloadAllAdditiveScenesAsync();

            loadedScenes = SceneController.Instance.GetLoadedAdditiveScenes();
            Debug.Log($"[Test] Loaded scenes after unload: {string.Join(", ", loadedScenes)}");

            //await RefreshSaveList();
            lastSelectedSaveFolder = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ProfessionalSkilMenu] Error during session quit: {ex}");

        }
        finally
        {
            //Debug.Log($"");
        }
    }

    public async Task OnSaveSession()
    {
        try
        {
            string userName = UserAccountManager.Instance.CurrentUserBaseName;
            if (string.IsNullOrEmpty(userName))
                throw new Exception("No user logged in!");

            // Kiểm tra và cập nhật CurrentMap
            string currentScene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(PlayerCheckPoint.Instance.CurrentMap) || PlayerCheckPoint.Instance.CurrentMap == "Unknown" || PlayerCheckPoint.Instance.CurrentMap != currentScene)
            {
                PlayerCheckPoint.Instance.SetCurrentMapToCurrentScene();
                Debug.Log($"[Test] Updated CurrentMap to: {PlayerCheckPoint.Instance.CurrentMap}");
            }

            string saveFolder = lastSelectedSaveFolder;
            if (string.IsNullOrEmpty(saveFolder) || !Directory.Exists(saveFolder))
            {
                saveFolder = await GetValidLastSaveFolderAsync();
                if (string.IsNullOrEmpty(saveFolder))
                {
                    saveFolder = await SaveGameManager.Instance.CreateNewSaveFolder(userName);
                }
                lastSelectedSaveFolder = saveFolder;
                //ContinueGame_Bt.interactable = true;
            }

            await SaveGameManager.Instance.SaveToFolderAsync(saveFolder, CancellationToken.None); // Lưu vào thư mục đã chọn
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ProfessionalSkilMenu] Error during save session: {ex}");
            throw;
        }
        //finally
        //{
        //    //Debug.Log($"");
        //}
    }

    //public async Task<SaveListContext> RefreshSaveList()
    //{
    //    foreach (var item in saveItemInstances)
    //    {
    //        Destroy(item);
    //    }
    //    saveItemInstances.Clear();

    //    string userName = UserAccountManager.Instance.CurrentUserBaseName;
    //    if (string.IsNullOrEmpty(userName))
    //    {
    //        Debug.LogWarning("[Test] No user logged in. Cannot refresh save list.");
    //        return new SaveListContext
    //        {
    //            UserName = userName,
    //            Saves = new List<SaveFolder>(),
    //            IsContinueEnabled = false
    //        };
    //    }

    //    var saves = await SaveGameManager.Instance.GetAllSaveFoldersAsync(userName);

    //    if (!string.IsNullOrEmpty(lastSelectedSaveFolder) &&
    //        !Directory.Exists(lastSelectedSaveFolder))
    //    {
    //        lastSelectedSaveFolder = null;
    //    }

    //    //return new SaveListContext
    //    //{
    //    //    UserName = userName,
    //    //    Saves = saves,
    //    //    IsContinueEnabled = !string.IsNullOrEmpty(lastSelectedSaveFolder) &&
    //    //                        Directory.Exists(lastSelectedSaveFolder)
    //    //};
    //}

    private async Task<string> GetValidLastSaveFolderAsync()
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

        var saves = await SaveGameManager.Instance.GetAllSaveFoldersAsync(UserAccountManager.Instance.CurrentUserBaseName);
        string latestFolder = saves.Count > 0 ? saves[0].FolderPath : null;
        //ContinueGame_Bt.interactable = !string.IsNullOrEmpty(latestFolder);
        return latestFolder;
    }
}
