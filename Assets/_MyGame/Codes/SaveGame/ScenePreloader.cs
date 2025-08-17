using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility để preload một danh sách scene (tải additive, chờ ready) rồi unload hết.
/// - Designed to work with SceneController (tận dụng OnSceneFullyReady).
/// - Mặc định tải tuần tự để giảm lag; có option tải parallel.
/// </summary>
public class ScenePreloader : MonoBehaviour
{
    /// <summary>
    /// Coroutine chính:
    /// - sceneNames: tên các scene (phải tồn tại trong Build Settings)
    /// - perSceneTimeout: timeout (s) chờ mỗi scene ready (mặc định 10s)
    /// - parallel: nếu true thì load tất cả cùng lúc rồi chờ mọi scene, nếu false thì load tuần tự
    /// - onComplete: callback khi toàn bộ quy trình (preload -> unload) hoàn tất
    /// </summary>
    public IEnumerator PreloadScenesAndRelease(
        IEnumerable<string> sceneNames,
        float perSceneTimeout = 10f,
        bool parallel = false,
        Action onComplete = null)
    {
        if (sceneNames == null)
        {
            Debug.LogWarning("[ScenePreloader] sceneNames null -> abort.");
            onComplete?.Invoke();
            yield break;
        }

        var list = sceneNames.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        if (list.Count == 0)
        {
            Debug.LogWarning("[ScenePreloader] No scenes given -> nothing to do.");
            onComplete?.Invoke();
            yield break;
        }

        // validate scenes are in build settings; remove invalid ones and warn
        var validList = list.Where(IsSceneInBuildSettings).ToList();
        var invalid = list.Except(validList).ToList();
        if (invalid.Count > 0)
        {
            Debug.LogWarning($"[ScenePreloader] Some scenes are not in Build Settings and will be skipped: {string.Join(", ", invalid)}");
        }

        if (validList.Count == 0)
        {
            Debug.LogWarning("[ScenePreloader] No valid scenes to preload.");
            onComplete?.Invoke();
            yield break;
        }

        // ensure SceneController exists
        if (SceneController.Instance == null)
        {
            Debug.LogError("[ScenePreloader] SceneController.Instance is null. Ensure SceneController is present in scene and initialized.");
            onComplete?.Invoke();
            yield break;
        }

        // choose strategy
        if (!parallel)
        {
            // sequential load -> wait ready -> continue
            foreach (var scene in validList)
            {
                //bool started = false;
                bool finished = false;

                Action<string> onReady = null;
                onReady = (loadedName) =>
                {
                    if (!string.Equals(loadedName, scene, StringComparison.Ordinal)) return;
                    finished = true;
                };

                // subscribe and trigger load
                SceneController.Instance.OnSceneFullyReady += onReady;
                //started = true;
                SceneController.Instance.LoadAdditiveScene(scene);

                // wait for ready or timeout
                float start = Time.realtimeSinceStartup;
                while (!finished && (Time.realtimeSinceStartup - start) < perSceneTimeout)
                    yield return null;

                // cleanup subscription
                SceneController.Instance.OnSceneFullyReady -= onReady;

                if (!finished)
                {
                    Debug.Log($"[ScenePreloader] Timeout waiting for scene '{scene}' to become fully ready after {perSceneTimeout}s. Continuing.");
                }
                else
                {
                    Debug.Log($"[ScenePreloader] Scene '{scene}' fully ready.");
                }

                // optional tiny yield to relieve frame
                yield return null;
            }
        }
        else
        {
            // parallel: load all, then wait for all ready
            var toWait = new HashSet<string>(validList, StringComparer.Ordinal);
            Action<string> onReadyParallel = null;
            onReadyParallel = (loadedName) =>
            {
                if (toWait.Contains(loadedName))
                {
                    toWait.Remove(loadedName);
                }
            };

            SceneController.Instance.OnSceneFullyReady += onReadyParallel;

            // trigger load all
            foreach (var scene in validList)
            {
                SceneController.Instance.LoadAdditiveScene(scene);
                yield return null; // spread start across frames a little
            }

            // wait for all or timeout
            float startPar = Time.realtimeSinceStartup;
            float maxTimeout = perSceneTimeout * Mathf.Max(1, validList.Count); // total reasonable upperbound
            while (toWait.Count > 0 && (Time.realtimeSinceStartup - startPar) < maxTimeout)
                yield return null;

            SceneController.Instance.OnSceneFullyReady -= onReadyParallel;

            if (toWait.Count > 0)
                Debug.LogWarning($"[ScenePreloader] Timeout waiting for scenes {string.Join(", ", toWait)} to become ready. Continuing.");
            else
                Debug.Log($"[ScenePreloader] All scenes ready (parallel).");
        }

        // At this point: all scenes either reported ready or timed out. 
        // Now we unload all additive scenes to free memory (SceneController will fallback excluded scenes).
        bool unloadDone = false;
        SceneController.Instance.UnloadAllAdditiveScenes(() => unloadDone = true);

        // wait for unload completion (or fallback timeout)
        float unloadWaitStart = Time.realtimeSinceStartup;
        float unloadTimeout = Mathf.Max(5f, perSceneTimeout); // minimal 5s
        while (!unloadDone && (Time.realtimeSinceStartup - unloadWaitStart) < unloadTimeout)
            yield return null;

        if (!unloadDone)
            Debug.LogWarning("[ScenePreloader] Timeout waiting for UnloadAllAdditiveScenes completion.");
        else
            Debug.Log("[ScenePreloader] All additive scenes unloaded (preload complete).");

        onComplete?.Invoke();
    }

    // Utility: check Build Settings
    private bool IsSceneInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = PathGetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    // Helper to avoid using System.IO.Path (to be robust across platforms)
    private string PathGetFileNameWithoutExtension(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        int lastSlash = path.Replace('\\', '/').LastIndexOf('/');
        string file = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
        int dot = file.LastIndexOf('.');
        return dot >= 0 ? file.Substring(0, dot) : file;
    }
}
