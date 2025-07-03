using DuckLe;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    private List<string> loadedScenes = new List<string>();
    private string initialSceneName;
    private static readonly string[] ExcludedScenes = { "Menu" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("UnhandledExceptionEventArgs");
        }
    }

    public void LoadAdditiveScene(string sceneName, PlayerCheckPoint playerCheckPoint, Action onComplete)
    {
        if (string.IsNullOrEmpty(sceneName) || !IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"Scene {sceneName} is not valid!");
            onComplete?.Invoke();
            return;
        }

        if (ExcludedScenes.Contains(sceneName))
        {
            Debug.LogWarning($"Attempted to load excluded scene {sceneName}.");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName, playerCheckPoint, onComplete));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, PlayerCheckPoint playerCheckPoint, Action onComplete)
    {
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncOp.isDone);

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            loadedScenes.Add(sceneName);
        }
        onComplete?.Invoke();
    }

    public void UnloadAllAdditiveScenes(Action onComplete)
    {
        StartCoroutine(UnloadAllScenesCoroutine(onComplete));
    }

    private IEnumerator UnloadAllScenesCoroutine(Action onComplete)
    {
        var scenesToUnload = new List<string>(loadedScenes);
        scenesToUnload.RemoveAll(scene => ExcludedScenes.Contains(scene));

        foreach (var sceneName in scenesToUnload)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                var asyncOp = SceneManager.UnloadSceneAsync(sceneName);
                yield return new WaitUntil(() => asyncOp.isDone);
                Debug.Log($"Unloaded scene: {sceneName}");
            }
        }
        loadedScenes.RemoveAll(scene => !ExcludedScenes.Contains(scene));

        if (loadedScenes.Count == 0)
        {
            Scene initialScene = SceneManager.GetSceneByName(initialSceneName);
            if (initialScene.IsValid())
            {
                SceneManager.SetActiveScene(initialScene);
            }
        }
        onComplete?.Invoke();
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName) return true;
        }
        return false;
    }

    public List<string> GetLoadedAdditiveScenes() => new List<string>(loadedScenes);
}