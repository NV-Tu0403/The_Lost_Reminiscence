using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class JsonFileHandler
{
    /// <summary>
    /// lưu một tệp JSON vào thư mục đã cho.
    /// </summary>
    /// <param name="saveFolderPath"></param>
    /// <param name="fileName"></param>
    /// <param name="jsonContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SaveJsonFileAsync(string saveFolderPath, string fileName, string jsonContent, CancellationToken cancellationToken = default)
    {
        try
        {
            string filePath = Path.Combine(saveFolderPath, fileName);
            await File.WriteAllTextAsync(filePath, jsonContent, cancellationToken);
            Debug.Log($"Saved JSON to: {filePath}");
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"Save JSON operation canceled for {fileName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save JSON {fileName}: {e.Message}");
        }
    }

    /// <summary>
    /// Tải một tệp JSON đơn từ thư mục đã cho.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> LoadSingleJsonFileAsync(string folderPath, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            string filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath)) return null;
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load single JSON file {fileName}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Tải tất cả các tệp JSON từ thư mục đã cho.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(string fileName, string json)[]> LoadJsonFilesAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var result = new List<(string fileName, string json)>();
        try
        {
            var jsonFiles = await Task.Run(() => Directory.GetFiles(folderPath, "*.json"), cancellationToken);
            if (jsonFiles.Length == 0)
            {
                Debug.LogWarning($"No JSON files found in {folderPath}");
                return result.ToArray();
            }

            foreach (var file in jsonFiles)
            {
                try
                {
                    string json = await File.ReadAllTextAsync(file, cancellationToken);
                    string fileName = Path.GetFileName(file);
                    result.Add((fileName, json));
                    Debug.Log($"Loaded JSON from: {file}");
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning($"Load JSON operation canceled for {file}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to read JSON file {file}: {ex.Message}");
                }
            }
            return result.ToArray();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"Load JSON files operation canceled in {folderPath}");
            return result.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load JSON files in {folderPath}: {e.Message}");
            return result.ToArray();
        }
    }


}