using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class FolderManager
{
    private readonly string userDataPath;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(2, 2); // Giới hạn 2 tác vụ I/O đồng thời
    private Dictionary<string, List<string>> folderCache = new Dictionary<string, List<string>>();

    public FolderManager(string userDataPath)
    {
        this.userDataPath = userDataPath;
    }

    public async Task<string> CreateNewSaveFolderAsync(string userName, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogError("UserName is empty. Cannot create save folder!");
                return null;
            }

            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            if (!Directory.Exists(fileSavePath))
            {
                await Task.Run(() => Directory.CreateDirectory(fileSavePath), cancellationToken);
                Debug.Log($"Created FileSave directory: {fileSavePath}");
            }

            string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            int index = await GetNextIndexAsync(userName, cancellationToken);
            string folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
            string folderPath = Path.Combine(fileSavePath, folderName);

            if (Directory.Exists(folderPath))
            {
                Debug.LogWarning($"Save folder already exists: {folderPath}. Generating new index.");
                index = await GetNextIndexAsync(userName, cancellationToken);
                folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
                folderPath = Path.Combine(fileSavePath, folderName);
            }

            await Task.Run(() => Directory.CreateDirectory(folderPath), cancellationToken);
            Debug.Log($"Created new save folder: {folderPath}");

            // Cập nhật cache
            if (!folderCache.ContainsKey(userName))
                folderCache[userName] = new List<string>();
            folderCache[userName].Add(folderPath);

            return folderPath;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<int> GetNextIndexAsync(string userName, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            if (!Directory.Exists(fileSavePath)) return 1;

            var folders = await Task.Run(() => Directory.GetDirectories(fileSavePath)
                .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
                .Select(d =>
                {
                    string[] parts = Path.GetFileName(d).Split('_');
                    if (parts.Length == 4 && int.TryParse(parts[3], out int idx))
                        return idx;
                    return 0;
                })
                .ToList(), cancellationToken);

            int nextIndex = folders.Any() ? folders.Max() + 1 : 1;
            Debug.Log($"Next index for {userName}: {nextIndex}");
            return nextIndex;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<string> GetLatestSaveFolderAsync(string userName, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Làm mới cache: loại bỏ các thư mục không tồn tại
            if (folderCache.ContainsKey(userName))
            {
                folderCache[userName].RemoveAll(d => !Directory.Exists(d));
            }

            if (folderCache.ContainsKey(userName) && folderCache[userName].Any())
            {
                var cachedFolders = folderCache[userName]
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

                string latestFolder = cachedFolders.FirstOrDefault()?.Path;
                if (latestFolder != null)
                {
                    Debug.Log($"Found latest save folder from cache for user {userName}: {latestFolder}");
                    return latestFolder;
                }
            }

            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            if (!Directory.Exists(fileSavePath))
            {
                Debug.LogWarning($"No FileSave directory for user: {userName}");
                return null;
            }

            var folders = await Task.Run(() => Directory.GetDirectories(fileSavePath)
                .Where(d =>
                {
                    if (!Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
                        return false;
                    if (!Directory.Exists(d))
                    {
                        Debug.LogWarning($"Folder does not exist: {d}");
                        return false;
                    }
                    try
                    {
                        return Directory.GetFiles(d, "*.json").Length > 0;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to check JSON files in {d}: {ex.Message}");
                        return false;
                    }
                })
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
                .ToList(), cancellationToken);

            string latestFolderPath = folders.FirstOrDefault()?.Path;
            if (latestFolderPath != null)
            {
                Debug.Log($"Found latest save folder for user {userName}: {latestFolderPath}");
                if (!folderCache.ContainsKey(userName))
                    folderCache[userName] = new List<string>();
                folderCache[userName].Add(latestFolderPath);
                return latestFolderPath;
            }

            Debug.LogWarning($"No valid save folder found for user: {userName}");
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<bool> DeleteSaveFolderAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (Directory.Exists(folderPath))
            {
                await Task.Run(() => Directory.Delete(folderPath, true), cancellationToken);
                Debug.Log($"Deleted save folder: {folderPath}");

                // Xóa thư mục khỏi cache
                foreach (var cacheEntry in folderCache)
                {
                    cacheEntry.Value.Remove(folderPath);
                }
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
        finally
        {
            semaphore.Release();
        }
    }
    public async Task<bool> DeleteSaveFolderAsync(string folderPath)
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                await Task.Run(() => Directory.Delete(folderPath, true));
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

    public async Task<string> DuplicateSaveFolderAsync(string sourceFolderPath, string userName)
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError($"Source save folder does not exist: {sourceFolderPath}");
            return null;
        }

        string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int index = await GetNextIndexAsync(userName, CancellationToken.None);
        string newFolderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        string newFolderPath = Path.Combine(fileSavePath, newFolderName);

        try
        {
            await Task.Run(() => Directory.CreateDirectory(newFolderPath));
            foreach (string file in Directory.GetFiles(sourceFolderPath))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(newFolderPath, fileName);
                await Task.Run(() => File.Copy(file, destFile));
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

    public async Task<(bool Success, string ErrorMessage)> SyncFileSaveAsync(string sourceFolderPath)
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            string errorMessage = $"Source save folder does not exist: {sourceFolderPath}";
            Debug.LogError(errorMessage);
            return (false, errorMessage);
        }

        string savePath = GetTransferFolder();

        try
        {
            if (!Directory.Exists(savePath))
            {
                await Task.Run(() => Directory.CreateDirectory(savePath));
                Debug.Log($"Created SavePath directory: {savePath}");
            }

            string folderName = Path.GetFileName(sourceFolderPath);
            string destFolderPath = Path.Combine(savePath, folderName);

            if (Directory.Exists(destFolderPath))
            {
                await Task.Run(() => Directory.Delete(destFolderPath, true));
                Debug.Log($"Deleted existing destination folder: {destFolderPath}");
            }

            await DirectoryCopyAsync(sourceFolderPath, destFolderPath, true);
            Debug.Log($"Synced entire folder from {sourceFolderPath} to {destFolderPath}");
            return (true, string.Empty);
        }
        catch (Exception e)
        {
            string errorMessage = $"Failed to sync folder: {e.Message}";
            Debug.LogError(errorMessage);
            return (false, errorMessage);
        }
    }

    private string GetTransferFolder()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "Loc_Backend/SavePath");
#else
        return Path.Combine(Application.persistentDataPath, "SavePath");
#endif
    }

    private async Task DirectoryCopyAsync(string sourceDir, string destDir, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);
        DirectoryInfo[] dirs = dir.GetDirectories();
        await Task.Run(() => Directory.CreateDirectory(destDir));

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDir, file.Name);
            await Task.Run(() => file.CopyTo(tempPath, false));
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDir, subdir.Name);
                await DirectoryCopyAsync(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }

    public async Task<List<(string FolderPath, string ImagePath)>> GetAllSaveFoldersAsync(string userName)
    {
        string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
        var result = new List<(string, string)>();
        if (!Directory.Exists(fileSavePath))
        {
            Debug.LogWarning($"No FileSave directory for user: {userName}");
            return result;
        }

        var folders = await Task.Run(() => Directory.GetDirectories(fileSavePath)
            .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
            .ToList());

        foreach (var folder in folders)
        {
            string imagePath = Path.Combine(folder, "screenshot.png");
            string image = File.Exists(imagePath) ? imagePath : null;
            result.Add((folder, image));
        }

        Debug.Log($"Found {result.Count} save folders for user: {userName}");
        return result;
    }
}