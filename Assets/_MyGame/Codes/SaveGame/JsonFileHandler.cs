using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class JsonFileHandler
{
    public void SaveJsonFile(string saveFolderPath, string fileName, string jsonContent)
    {
        try
        {
            string filePath = Path.Combine(saveFolderPath, fileName);
            File.WriteAllText(filePath, jsonContent);
            //Debug.Log($"Saved JSON to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save JSON {fileName}: {e.Message}");
        }
    }

    public string LoadSingleJsonFile(string folderPath, string fileName)
    {
        try
        {
            string filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath)) return null;
            return File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load JSON file {fileName}: {e.Message}");
            return null;
        }
    }

    public (string fileName, string json)[] LoadJsonFiles(string folderPath)
    {
        var result = new List<(string fileName, string json)>();
        try
        {
            var jsonFiles = Directory.GetFiles(folderPath, "*.json");
            if (jsonFiles.Length == 0)
            {
                Debug.LogWarning($"No JSON files found in {folderPath}");
                return result.ToArray();
            }

            foreach (var file in jsonFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    string fileName = Path.GetFileName(file);
                    result.Add((fileName, json));
                    //Debug.Log($"Loaded JSON from: {file}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to read JSON file {file}: {ex.Message}");
                }
            }
            return result.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load JSON files in {folderPath}: {e.Message}");
            return result.ToArray();
        }
    }
}