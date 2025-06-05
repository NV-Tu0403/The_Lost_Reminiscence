using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    private readonly List<ISaveable> saveables = new List<ISaveable>();
    private FolderManager folderManager;
    private JsonFileHandler jsonFileHandler;
    private float lastSaveTime;
    private const float SAVE_COOLDOWN = 5f; // Chỉ lưu sau mỗi 5 giây

    private void Awake()
    {
        string userDataPath = Path.Combine(Application.persistentDataPath, "User_DataGame");
        folderManager = new FolderManager(userDataPath);
        jsonFileHandler = new JsonFileHandler();
    }

    public void RegisterSaveable(ISaveable saveable)
    {
        if (saveable != null && !saveables.Contains(saveable))
        {
            saveables.Add(saveable);
            Debug.Log($"Registered saveable: {saveable.FileName}");
        }
    }

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
}