using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    private readonly List<ISaveable> saveables = new List<ISaveable>();
    public static SaveGameManager Instance { get; private set; }

    private FolderManager folderManager;
    private JsonFileHandler jsonFileHandler;

    private float lastSaveTime;
    private const float SAVE_COOLDOWN = 5f; // Chỉ lưu sau mỗi 5 giây

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

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
    /// Tải một tệp JSON từ thư mục đã cho.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<string> LoadJsonFile(string folderPath, string fileName)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var files = await jsonFileHandler.LoadJsonFilesAsync(folderPath, cts.Token);
        var result = files.FirstOrDefault(f => f.fileName == fileName);
        return result.json;
    }

    /// <summary>
    /// Đăng ký một đối tượng ISaveable để quản lý lưu trữ.
    /// </summary>
    /// <param name="saveable"></param>
    public void RegisterSaveable(ISaveable saveable)
    {
        if (saveable != null && !saveables.Contains(saveable))
        {
            saveables.Add(saveable);
            Debug.Log($"Registered saveable: {saveable.FileName}");
        }
    }

    /// <summary>
    /// Lưu tất cả dữ liệu của người dùng hiện tại vào thư mục lưu trữ.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task SaveAllAsync(string userName)
    {
        if (Time.time - lastSaveTime < SAVE_COOLDOWN)
        {
            Debug.Log("Save cooldown active, skipping save.");
            return;
        }

        lastSaveTime = Time.time;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        string tempFolderPath = Path.Combine(Application.persistentDataPath, "TempSave_" + Guid.NewGuid().ToString());
        string saveFolderPath = await folderManager.CreateNewSaveFolderAsync(userName, cts.Token);
        if (saveFolderPath == null) return;

        try
        {
            // Tạo thư mục tạm thời
            await Task.Run(() => Directory.CreateDirectory(tempFolderPath), cts.Token);

            // Lưu tất cả ISaveable vào thư mục tạm
            foreach (var saveable in saveables)
            {
                string json;
                lock (saveable)
                {
                    json = saveable.SaveToJson();
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new Exception($"Failed to serialize {saveable.FileName}");
                    }
                }
                await jsonFileHandler.SaveJsonFileAsync(tempFolderPath, saveable.FileName, json, cts.Token);
            }

            // Di chuyển từ thư mục tạm sang thư mục chính thức
            await Task.Run(() =>
            {
                foreach (var file in Directory.GetFiles(tempFolderPath))
                {
                    string destFile = Path.Combine(saveFolderPath, Path.GetFileName(file));
                    File.Move(file, destFile);
                }
                Directory.Delete(tempFolderPath, true);
            }, cts.Token);

            Debug.Log($"Successfully saved all files to {saveFolderPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
            // Rollback: Xóa thư mục tạm nếu có lỗi
            if (Directory.Exists(tempFolderPath))
            {
                await Task.Run(() => Directory.Delete(tempFolderPath, true), cts.Token);
            }
            if (Directory.Exists(saveFolderPath))
            {
                await folderManager.DeleteSaveFolderAsync(saveFolderPath, cts.Token);
            }
        }
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
            var saveable = saveables.Find(s => s.FileName == fileName);
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