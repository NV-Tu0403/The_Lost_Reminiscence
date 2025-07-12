using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FolderManager
{
    private readonly string userDataPath;
    private readonly object folderLock = new object();
    private Dictionary<string, List<string>> folderCache = new Dictionary<string, List<string>>();

    public FolderManager(string userDataPath)
    {
        this.userDataPath = userDataPath;
    }

    public string CreateNewSaveFolder(string userName)
    {
        lock (folderLock)
        {
            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogError("[FolderManager] UserName is empty!");
                return null;
            }

            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            if (!Directory.Exists(fileSavePath))
            {
                Directory.CreateDirectory(fileSavePath);
                Debug.Log($"[FolderManager] Created FileSave directory: {fileSavePath}");
            }

            string dateSave = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            int index = GetNextIndex(userName);
            string folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
            string folderPath = Path.Combine(fileSavePath, folderName);

            if (Directory.Exists(folderPath))
            {
                index = GetNextIndex(userName);
                folderName = $"SaveGame_{userName}_{dateSave}_{index:D3}";
                folderPath = Path.Combine(fileSavePath, folderName);
            }

            Directory.CreateDirectory(folderPath);
            Debug.Log($"[FolderManager] Created new save folder: {folderPath}");

            if (!folderCache.ContainsKey(userName))
                folderCache[userName] = new List<string>();
            folderCache[userName].Add(folderPath);

            return folderPath;
        }
    }

    private int GetNextIndex(string userName)
    {
        lock (folderLock)
        {
            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            if (!Directory.Exists(fileSavePath)) return 1;

            var folders = Directory.GetDirectories(fileSavePath)
                .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
                .Select(d =>
                {
                    string[] parts = Path.GetFileName(d).Split('_');
                    if (parts.Length == 4 && int.TryParse(parts[3], out int idx))
                        return idx;
                    return 0;
                })
                .ToList();

            return folders.Any() ? folders.Max() + 1 : 1;
        }
    }

    public string GetLatestSaveFolder(string userName)
    {
        lock (folderLock)
        {
            if (folderCache.ContainsKey(userName))
            {
                folderCache[userName].RemoveAll(d => !Directory.Exists(d));
            }

            if (folderCache.ContainsKey(userName) && folderCache[userName].Any())
            {
                var latestFolder = folderCache[userName]
                    .Select(d => new { Path = d, Name = Path.GetFileName(d) })
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
                    .FirstOrDefault()?.Path;

                if (latestFolder != null)
                {
                    Debug.Log($"[FolderManager] Found latest save folder from cache: {latestFolder}");
                    return latestFolder;
                }
            }

            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            if (!Directory.Exists(fileSavePath))
            {
                Debug.LogWarning($"[FolderManager] No FileSave directory for user: {userName}");
                return null;
            }

            var latestFolderPath = Directory.GetDirectories(fileSavePath)
                .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_") && Directory.GetFiles(d, "*.json").Length > 0)
                .Select(d => new { Path = d, Name = Path.GetFileName(d) })
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
                .FirstOrDefault()?.Path;

            if (latestFolderPath != null)
            {
                if (!folderCache.ContainsKey(userName))
                    folderCache[userName] = new List<string>();
                folderCache[userName].Add(latestFolderPath);
                return latestFolderPath;
            }

            Debug.LogWarning($"[FolderManager] No valid save folder found for user: {userName}");
            return null;
        }
    }

    public bool DeleteSaveFolder(string folderPath)
    {
        lock (folderLock)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                Debug.Log($"[FolderManager] Deleted save folder: {folderPath}");
                foreach (var cacheEntry in folderCache)
                {
                    cacheEntry.Value.Remove(folderPath);
                }
                return true;
            }
            Debug.LogWarning($"[FolderManager] Save folder not found: {folderPath}");
            return false;
        }
    }

    public List<(string FolderPath, string ImagePath)> GetAllSaveFolders(string userName)
    {
        lock (folderLock)
        {
            string fileSavePath = Path.Combine(userDataPath, $"FileSave_{userName}");
            var result = new List<(string, string)>();
            if (!Directory.Exists(fileSavePath))
            {
                Debug.LogWarning($"[FolderManager] No FileSave directory for user: {userName}");
                return result;
            }

            var folders = Directory.GetDirectories(fileSavePath)
                .Where(d => Path.GetFileName(d).StartsWith($"SaveGame_{userName}_"))
                .ToList();

            foreach (var folder in folders)
            {
                string imagePath = Path.Combine(folder, "screenshot.png");
                string image = File.Exists(imagePath) ? imagePath : null;
                result.Add((folder, image));
            }

            // Debug.Log($"Found {result.Count} save folders for user: {userName}");
            return result;
        }
    }
}