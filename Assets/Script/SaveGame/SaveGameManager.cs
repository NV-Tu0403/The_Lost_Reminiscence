using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly object saveablesLock = new object();//  để bảo vệ truy cập đồng thời đến danh sách saveables
    private readonly HashSet<ISaveable> saveables = new HashSet<ISaveable>();
    public static SaveGameManager Instance { get; private set; }

    private FolderManager folderManager;
    private JsonFileHandler jsonFileHandler;

    private float lastSaveTime;
    private const float SAVE_COOLDOWN = 5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        string userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        folderManager = new FolderManager(userDataPath);
        jsonFileHandler = new JsonFileHandler();
    }


    /// <summary>
    /// Lưu một tệp JSON vào thư mục đã cho.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileName"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    public async Task SaveJsonFile(string folderPath, string fileName, string json)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await jsonFileHandler.SaveJsonFileAsync(folderPath, fileName, json, cts.Token);
    }

    /// <summary>
    /// Tải một tệp JSON đơn từ thư mục đã cho.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<string> LoadJsonFile(string folderPath, string fileName)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        return await jsonFileHandler.LoadSingleJsonFileAsync(folderPath, fileName, cts.Token);
    }

    /// <summary>
    /// Đăng ký một đối tượng ISaveable để quản lý lưu trữ.
    /// lock để đảm bảo an toàn khi truy cập đồng thời từ nhiều luồng.
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

    /// <summary>
    /// Hủy đăng ký một đối tượng ISaveable khỏi quản lý lưu trữ.
    /// </summary>
    /// <param name="saveable"></param>
    public void UnregisterSaveable(ISaveable saveable)
    {
        if (saveable == null) return;

        lock (saveablesLock)
        {
            saveables.Remove(saveable);
        }
    }

    /// <summary>
    /// Xóa tất cả các đối tượng ISaveable khỏi quản lý lưu trữ.
    /// </summary>
    public void ClearAllSaveables()
    {
        lock (saveablesLock)
        {
            saveables.Clear();
        }
    }

    /// <summary>
    /// Lưu tất cả dữ liệu của người dùng hiện tại vào thư mục tạm thời.
    /// </summary>
    /// <param name="tempFolderPath"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<bool> SaveAllToTempFolderAsync(string tempFolderPath, CancellationToken token)
    {
        bool hasError = false;

        Directory.CreateDirectory(tempFolderPath);
        List<Task> saveTasks = new();

        lock (saveablesLock)
        {
            foreach (var saveable in saveables)
            {
                var json = saveable.SaveToJson();
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"Failed to serialize {saveable.FileName}");
                    hasError = true;
                    continue;
                }

                saveTasks.Add(jsonFileHandler.SaveJsonFileAsync(tempFolderPath, saveable.FileName, json, token));
            }
        }

        await Task.WhenAll(saveTasks);
        return !hasError;
    }

    public async Task SaveToFolderAsync(string folderPath, CancellationToken token)
    {
        //if (Time.time - lastSaveTime < SAVE_COOLDOWN)
        //{
        //    Debug.LogWarning("[SaveGameManager] Save cooldown active, skipping save.");
        //    return;
        //}

        //lastSaveTime = Time.time;
        string tempFolderPath = Path.Combine(Application.persistentDataPath, $"TempSave_{Guid.NewGuid()}");

        try
        {
            if (!await SaveAllToTempFolderAsync(tempFolderPath, token))
                throw new Exception("One or more saveable objects failed to save.");

            // Xóa các file cũ trong folder đích
            if (Directory.Exists(folderPath))
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(folderPath);
            }

            // Di chuyển file từ temp sang folder đích
            await MoveFilesAsync(tempFolderPath, folderPath, token);
            Debug.Log($"[SaveGameManager] Successfully saved to {folderPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveGameManager] Save failed: {ex.Message}");
            await RollbackSave(tempFolderPath, folderPath, token);
            throw;
        }
    }

    /// <summary>
    /// Lưu tất cả dữ liệu của người dùng hiện tại vào thư mục lưu trữ.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public async Task SaveAllAsync(string userName, string folderPath = null)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        string saveFolderPath = folderPath ?? await folderManager.CreateNewSaveFolderAsync(userName, cts.Token);
        if (saveFolderPath == null) return;

        await SaveToFolderAsync(saveFolderPath, cts.Token);
    }

    private async Task MoveFilesAsync(string sourceFolder, string destFolder, CancellationToken token)
    {
        await Task.Run(() =>
        {
            foreach (var file in Directory.GetFiles(sourceFolder))
            {
                string dest = Path.Combine(destFolder, Path.GetFileName(file));
                File.Move(file, dest);
            }
            Directory.Delete(sourceFolder, true);
        }, token);
    }

    private async Task RollbackSave(string tempFolder, string saveFolder, CancellationToken token)
    {
        if (Directory.Exists(tempFolder))
            await Task.Run(() => Directory.Delete(tempFolder, true), token);

        if (Directory.Exists(saveFolder))
            await folderManager.DeleteSaveFolderAsync(saveFolder, token);
    }

    /// <summary>
    /// Tải dữ liệu mới nhất từ thư mục lưu trữ của người dùng.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task LoadLatestAsync(string userName)
    {
        string latestFolder = await folderManager.GetLatestSaveFolderAsync(userName);
        if (latestFolder == null) return;

        var jsonFiles = await jsonFileHandler.LoadJsonFilesAsync(latestFolder);
        foreach (var (fileName, json) in jsonFiles)
        {
            var saveable = saveables.FirstOrDefault(s => s.FileName == fileName);
            saveable?.LoadFromJson(json);
        }
    }

    #region controller/helper

    /// <summary>
    /// Lưu ngay lập tức tất cả dữ liệu của người dùng hiện tại.
    /// Gọi từ void, không quan tâm save xong chưa.
    /// </summary>
    public void SaveNow()
    {
        var userName = UserAccountManager.Instance?.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("[SaveGameManager] SaveNow: No user logged in.");
            return;
        }

        _ = SaveAsyncSafe(userName);
    }

    private async Task SaveAsyncSafe(string userName)
    {
        try
        {
            await SaveAllAsync(userName);
            Debug.Log("[SaveGameManager] SaveNow: Save completed.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveGameManager] SaveNow: Save failed! {ex.Message}");
        }
    }

    /// <summary>
    /// Lưu tất cả dữ liệu của người dùng hiện tại, chờ hoàn thành.
    /// Cần đợi lưu xong mới tiếp, hoặc cần xử lý lỗi
    /// </summary>
    /// <returns></returns>
    public async Task SaveAwait()
    {
        var userName = UserAccountManager.Instance?.CurrentUserBaseName;
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("[SaveGameManager] SaveAwait: No user logged in.");
            return;
        }

        try
        {
            await SaveAllAsync(userName);
            Debug.Log("[SaveGameManager] SaveAwait: Save completed.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveGameManager] SaveAwait: Save failed! {ex.Message}");
        }
    }

    #endregion

    #region các phương thức ánh xạ đên quản lý thư mục lưu trữ

    public async Task<string> GetLatestSaveFolderAsync(string userName)
    {
        return await folderManager.GetLatestSaveFolderAsync(userName);
    }

    public async Task<List<(string FolderPath, string ImagePath)>> GetAllSaveFoldersAsync(string userName)
    {
        return await folderManager.GetAllSaveFoldersAsync(userName);
    }

    public async Task<bool> DeleteSaveFolderAsync(string folderPath)
    {
        return await folderManager.DeleteSaveFolderAsync(folderPath);
    }

    public async Task<string> DuplicateSaveFolderAsync(string sourceFolderPath, string userName)
    {
        return await folderManager.DuplicateSaveFolderAsync(sourceFolderPath, userName);
    }

    public async Task<(bool Success, string ErrorMessage)> SyncFileSaveAsync(string folderPath)
    {
        return await folderManager.SyncFileSaveAsync(folderPath);
    }

    public async Task<string> CreateNewSaveFolder(string userName)
    {
        return await folderManager.CreateNewSaveFolderAsync(userName);
    }

    public async Task<string> GetLatestSaveFolder(string userName)
    {
        return await folderManager.GetLatestSaveFolderAsync(userName);
    }

    #endregion
}