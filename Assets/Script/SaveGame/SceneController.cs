using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    private List<string> loadedScenes = new List<string>();
    private string initialSceneName;
    private static readonly string[] ExcludedScenes = { "Menu" }; // Scene không được unload

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            initialSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[SceneController] Initial scene set to: {initialSceneName}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    public async Task LoadAdditiveSceneAsync(string sceneName, PlayerCheckPoint playerCheckPoint = null)
    {
        if (string.IsNullOrEmpty(sceneName) || !IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneController] Scene {sceneName} is not valid or not in Build Settings!");
            return;
        }

        if (ExcludedScenes.Contains(sceneName))
        {
            Debug.LogWarning($"[SceneController] Attempted to load excluded scene {sceneName} additively. Skipping.");
            return;
        }

        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncOp.isDone)
        {
            await Task.Yield();
        }

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            loadedScenes.Add(sceneName);
            if (playerCheckPoint != null && playerCheckPoint.PlayerTransform != null)
            {
                Debug.Log($"[SceneController] Player position set: {playerCheckPoint.PlayerTransform.position}");
            }
            else
            {
                Debug.LogWarning("[SceneController] PlayerCheckPoint or PlayerTransform is null!");
            }
        }
    }

    public async Task UnloadAdditiveScenes(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneController] Scene {sceneName} is not valid or not in Build Settings!");
            return;
        }

        if (ExcludedScenes.Contains(sceneName))
        {
            Debug.LogWarning($"[SceneController] Attempted to unload excluded scene {sceneName}. Skipping.");
            return;
        }

        if (!loadedScenes.Contains(sceneName))
        {
            Debug.LogWarning($"[SceneController] Scene {sceneName} is not loaded additively!");
            return;
        }

        var asyncOp = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncOp.isDone)
        {
            await Task.Yield();
        }
        loadedScenes.Remove(sceneName);
        Debug.Log($"[SceneController] Unloaded scene: {sceneName}");

        // Đặt lại scene Menu làm active nếu không còn scene additive nào khác
        if (loadedScenes.Count == 0)
        {
            Scene initialScene = SceneManager.GetSceneByName(initialSceneName);
            if (initialScene.IsValid())
            {
                SceneManager.SetActiveScene(initialScene);
                Debug.Log($"[SceneController] Set active scene to: {initialSceneName}");
            }
        }
    }

    public async Task UnloadAllAdditiveScenesAsync(bool keepSpecificScenes = false, params string[] scenesToKeep)
    {
        // Đồng bộ loadedScenes với Hierarchy
        List<string> actualLoadedScenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && !ExcludedScenes.Contains(scene.name))
            {
                actualLoadedScenes.Add(scene.name);
            }
        }

        // Cảnh báo nếu loadedScenes không đồng bộ
        if (!loadedScenes.SequenceEqual(actualLoadedScenes))
        {
            Debug.LogWarning($"[SceneController] Mismatch in loaded scenes. Expected: {string.Join(",", loadedScenes)}, Actual: {string.Join(",", actualLoadedScenes)}");
            loadedScenes = actualLoadedScenes;
        }

        var scenesToUnload = new List<string>(loadedScenes);
        if (keepSpecificScenes)
        {
            scenesToUnload.RemoveAll(scene => scenesToKeep.Contains(scene) || ExcludedScenes.Contains(scene));
        }
        else
        {
            scenesToUnload.RemoveAll(scene => ExcludedScenes.Contains(scene));
        }

        foreach (var sceneName in scenesToUnload)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                var asyncOp = SceneManager.UnloadSceneAsync(sceneName);
                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }
                Debug.Log($"[SceneController] Unloaded scene: {sceneName}");
            }
        }
        loadedScenes.RemoveAll(scene => !scenesToKeep.Contains(scene) && !ExcludedScenes.Contains(scene));

        if (loadedScenes.Count == 0)
        {
            Scene initialScene = SceneManager.GetSceneByName(initialSceneName);
            if (initialScene.IsValid())
            {
                SceneManager.SetActiveScene(initialScene);
                Debug.Log($"[SceneController] Set active scene to: {initialSceneName}");
            }
        }
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName) return true;
        }
        return false;
    }

    // Lấy danh sách các scene additive đang được tải
    public List<string> GetLoadedAdditiveScenes()
    {
        return new List<string>(loadedScenes);
    }

    public async Task UnloadAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneController] Scene {sceneName} is not valid or not in Build Settings!");
            return;
        }

        if (ExcludedScenes.Contains(sceneName))
        {
            Debug.LogWarning($"[SceneController] Attempted to unload excluded scene {sceneName}. Skipping.");
            return;
        }

        // Kiểm tra scene có trong Hierarchy không
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"[SceneController] Scene {sceneName} is not loaded!");
            loadedScenes.Remove(sceneName); // Đồng bộ danh sách
            return;
        }

        var asyncOp = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncOp.isDone)
        {
            await Task.Yield();
        }
        loadedScenes.Remove(sceneName);
        Debug.Log($"[SceneController] Unloaded scene: {sceneName}");

        // Đặt lại scene Menu nếu không còn scene additive
        if (loadedScenes.Count == 0)
        {
            Scene initialScene = SceneManager.GetSceneByName(initialSceneName);
            if (initialScene.IsValid())
            {
                SceneManager.SetActiveScene(initialScene);
                Debug.Log($"[SceneController] Set active scene to: {initialSceneName}");
            }
        }
    }

    public bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.IsValid() && scene.isLoaded;
    }

    public void LogLoadedScenes()
    {
        Debug.Log($"[SceneController] Loaded scenes: {string.Join(",", loadedScenes)}");
    }
}