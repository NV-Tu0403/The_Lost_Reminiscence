using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneController: singleton quản lý load/unload scene additive,
/// và đảm bảo scene "fully ready" trước khi thông báo cho hệ thống khác.
/// 
/// - ISceneLoadHandler: interface để component trong scene báo "ready"
/// </summary>
public sealed class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Exclude & options")]
    [SerializeField] private List<string> excludedScenes = new List<string> { "BookMenu" };
    [SerializeField] private bool setNewLoadedSceneActive = true;

    [Header("Fallback frame-based options (nhanh, ít thay đổi)")]
    [Tooltip("Số frame tối thiểu chờ sau khi load nếu không có SceneInitializationManager")]
    [SerializeField] private int minimumFramesAfterLoad = 2;
    [Tooltip("Thời gian delay nhỏ (s) sau khi chờ frame")]
    [SerializeField] private float extraDelaySeconds = 0.03f;

    [Header("SceneInitializationManager options (deterministic)")]
    [Tooltip("Timeout (s) chờ SceneInitializationManager báo ready trước khi fallback")]
    [SerializeField] private float sceneInitManagerTimeout = 5f;

    private readonly HashSet<string> _loadedAdditiveScenes = new HashSet<string>(StringComparer.Ordinal);
    private string _initialSceneName;

    /// <summary> Sự kiện khi scene thực sự đã sẵn sàng (render + init handlers) </summary>
    public event Action<string> OnSceneFullyReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        // Xử lý danh sách excludedScenes
        excludedScenes = excludedScenes
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Start()
    {
        // Lưu tên scene ban đầu để có thể set lại nếu cần
        _initialSceneName = SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }

    #region Public API

    /// <summary>
    /// API để load scene additive.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onComplete"></param>
    public void LoadAdditiveScene(string sceneName, Action onComplete = null)
    {
        if (!ValidateSceneName(sceneName, out string reason))
        {
            Debug.LogError($"[SceneController] Không thể load '{sceneName}': {reason}");
            onComplete?.Invoke();
            return;
        }

        if (IsExcluded(sceneName))
        {
            Debug.LogWarning($"[SceneController] Bỏ qua scene bị exclude: {sceneName}");
            onComplete?.Invoke();
            return;
        }

        // kiểm tra nếu scene đã được load
        Scene existing = SceneManager.GetSceneByName(sceneName);
        if (existing.IsValid() && existing.isLoaded)
        {
            _loadedAdditiveScenes.Add(sceneName);
            if (setNewLoadedSceneActive) SceneManager.SetActiveScene(existing); // set active scene

            // đảm bảo fully ready even if already loaded
            StartCoroutine(CoEnsureSceneReady(sceneName, onComplete));
            return;
        }

        StartCoroutine(CoLoadAdditive(sceneName, onComplete));
    }

    /// <summary>
    /// API để unload scene additive chỉ định.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onComplete"></param>
    public void UnloadAdditiveScene(string sceneName, Action onComplete = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneController] Tên scene rỗng.");
            onComplete?.Invoke();
            return;
        }

        if (IsExcluded(sceneName))
        {
            Debug.LogWarning($"[SceneController] Không unload scene bị exclude: {sceneName}");
            onComplete?.Invoke();
            return;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"[SceneController] Scene '{sceneName}' chưa load hoặc không hợp lệ.");
            _loadedAdditiveScenes.Remove(sceneName);  // đảm bảo không còn trong danh sách
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(CoUnload(sceneName, onComplete));
    }

    /// <summary>
    /// API để unload tất cả các scene additive đã load, trừ những scene bị exclude.
    /// </summary>
    /// <param name="onComplete"></param>
    public void UnloadAllAdditiveScenes(Action onComplete = null)
    {
        StartCoroutine(CoUnloadAll(onComplete));
    }

    /// <summary>
    /// Lấy danh sách các scene additive đã load, sắp xếp theo tên.
    /// </summary>
    /// <returns></returns>
    public List<string> GetLoadedAdditiveScenes() => _loadedAdditiveScenes.OrderBy(n => n).ToList();

    #endregion

    #region Coroutines: load/unload + ensure ready

    /// <summary>
    /// Load scene additive và đảm bảo nó đã sẵn sàng (render + init handlers).
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator CoLoadAdditive(string sceneName, Action onComplete)
    {
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneController] Scene '{sceneName}' không có trong Build Settings.");
            onComplete?.Invoke();
            yield break;
        }

        AsyncOperation op = null;
        try
        {
            op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SceneController] LoadSceneAsync exception: {ex}");
            onComplete?.Invoke();
            yield break;
        }

        if (op == null)
        {
            Debug.LogError($"[SceneController] LoadSceneAsync trả về null cho '{sceneName}'.");
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitUntil(() => op.isDone);

        var newScene = SceneManager.GetSceneByName(sceneName);
        if (!newScene.IsValid() || !newScene.isLoaded)
        {
            Debug.LogError($"[SceneController] Scene '{sceneName}' không hợp lệ sau khi load.");
            onComplete?.Invoke();
            yield break;
        }

        _loadedAdditiveScenes.Add(sceneName);

        if (setNewLoadedSceneActive)
            SceneManager.SetActiveScene(newScene);

        // đảm bảo scene được render và init (cơ chế hybrid: tìm SceneInitializationManager nếu có, spawn nếu không)
        yield return StartCoroutine(CoEnsureSceneReady(sceneName, onComplete));
    }

    /// <summary>
    /// Đảm bảo scene đã sẵn sàng (render + init handlers).
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator CoEnsureSceneReady(string sceneName, Action onComplete)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            Debug.LogWarning($"[SceneController] CoEnsureSceneReady: scene '{sceneName}' không hợp lệ.");
            onComplete?.Invoke();
            yield break;
        }

        // 1) tìm SceneInitializationManager trong scene
        SceneInitializationManager initManager = FindInitManagerInScene(scene);

        // 2) nếu không có -> spawn 1 instance (auto) gắn vào scene
        if (initManager == null)
        {
            // spawn GameObject trong scene để SceneInitializationManager hoạt động và tự dò handlers.
            GameObject go = new GameObject($"_SceneInitializationManager_{sceneName}");
            // Move go into the loaded scene
            SceneManager.MoveGameObjectToScene(go, scene);
            initManager = go.AddComponent<SceneInitializationManager>();

            // set parameters từ controller (dùng thiết lập inspector của SceneController)
            initManager.overallTimeoutSeconds = Mathf.Max(0.01f, sceneInitManagerTimeout);
            // runHandlersInParallel mặc định true trong class; nếu muốn expose nhiều hơn, chỉnh class hoặc Controller
        }

        // 3) subscribe and wait for ready (or timeout)
        bool done = false;
        Action<string> onReady = (_) => done = true;
        initManager.OnSceneReady += onReady;

        float start = Time.realtimeSinceStartup;
        while (!done && (Time.realtimeSinceStartup - start) < sceneInitManagerTimeout)
            yield return null;

        initManager.OnSceneReady -= onReady;

        if (!done)
        {
            // fallback: chờ một vài frame + small delay
            Debug.LogWarning($"[SceneController] Timeout waiting SceneInitializationManager in '{sceneName}'. Falling back to frame-wait.");
            for (int i = 0; i < Mathf.Max(1, minimumFramesAfterLoad); i++)
                yield return new WaitForEndOfFrame();
            yield return null;
            if (extraDelaySeconds > 0f) yield return new WaitForSeconds(extraDelaySeconds);
        }
        else
        {
            // đảm bảo 1 frame render cuối
            yield return new WaitForEndOfFrame();
            yield return null;
        }

        // emit event
        OnSceneFullyReady?.Invoke(sceneName);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Unload scene additive chỉ định và gọi onComplete khi xong.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator CoUnload(string sceneName, Action onComplete)
    {
        AsyncOperation op = null;
        try
        {
            op = SceneManager.UnloadSceneAsync(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SceneController] UnloadSceneAsync exception: {ex}");
            onComplete?.Invoke();
            yield break;
        }

        if (op == null)
        {
            Debug.LogError($"[SceneController] UnloadSceneAsync trả về null cho '{sceneName}'.");
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitUntil(() => op.isDone);

        _loadedAdditiveScenes.Remove(sceneName);

        if (_loadedAdditiveScenes.Count == 0)
            TrySetActiveInitialScene(); // set lại active scene nếu không còn scene nào khác

        onComplete?.Invoke();
    }

    /// <summary>
    /// Unload tất cả các scene additive đã load, trừ những scene bị exclude.
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator CoUnloadAll(Action onComplete)
    {
        var toUnload = _loadedAdditiveScenes.Where(n => !IsExcluded(n)).ToList();
        foreach (var sceneName in toUnload)
        {
            yield return CoUnload(sceneName, null);
        }
        onComplete?.Invoke();
    }

#endregion

    #region Scene events

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid()) return;
        if (IsExcluded(scene.name)) return;
        if (mode == LoadSceneMode.Additive)
            _loadedAdditiveScenes.Add(scene.name);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (!scene.IsValid()) return;
        _loadedAdditiveScenes.Remove(scene.name);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Kiểm tra tên scene hợp lệ trước khi load.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    private bool ValidateSceneName(string sceneName, out string reason)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            reason = "Tên scene rỗng.";
            return false;
        }
        if (!IsSceneInBuildSettings(sceneName))
        {
            reason = "Scene không nằm trong Build Settings.";
            return false;
        }
        reason = null;
        return true;
    }

    /// <summary>
    /// Kiểm tra xem scene có nằm trong danh sách excluded hay không.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool IsExcluded(string sceneName) => excludedScenes.Contains(sceneName);

    /// <summary>
    /// Kiểm tra xem scene có nằm trong Build Settings hay không.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string nameInBuild = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(nameInBuild, sceneName, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Nếu không có scene nào được load, set active scene về scene ban đầu đã lưu.
    /// </summary>
    private void TrySetActiveInitialScene()
    {
        if (!string.IsNullOrEmpty(_initialSceneName))
        {
            var initial = SceneManager.GetSceneByName(_initialSceneName);
            if (initial.IsValid() && initial.isLoaded)
            {
                SceneManager.SetActiveScene(initial);
                return;
            }
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.IsValid() && s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
                return;
            }
        }
    }

    /// <summary>
    /// Tìm SceneInitializationManager trong scene đã load.
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private SceneInitializationManager FindInitManagerInScene(Scene scene)
    {
        if (!scene.IsValid()) return null;
        var roots = scene.GetRootGameObjects();
        foreach (var go in roots)
        {
            var m = go.GetComponentInChildren<SceneInitializationManager>(true);
            if (m != null) return m;
        }
        return null;
    }

    #endregion

    #region Nested types (interface + per-scene manager) 

    /// <summary>
    /// Interface cho các component muốn báo "ready" sau khi load.
    /// Implement IEnumerator WaitForReady() để SceneInitializationManager yield cho đến khi component sẵn sàng.
    /// Nếu component không cần chờ, return null hoặc yield break.
    /// </summary>
    public interface ISceneLoadHandler
    {
        IEnumerator WaitForReady();
    }

    /// <summary>
    /// SceneInitializationManager: tìm các ISceneLoadHandler trong scene và chờ chúng hoàn thành.
    /// Nếu không có handler thì chờ 1 frame để đảm bảo Start() chạy xong.
    /// Phát OnSceneReady khi hoàn tất hoặc timeout (vẫn phát nhưng sẽ log).
    /// </summary>
    public class SceneInitializationManager : MonoBehaviour
    {
        [Tooltip("Thời gian tối đa (s) chờ tất cả handlers sẵn sàng trước khi fallback")]
        public float overallTimeoutSeconds = 5f;

        [Tooltip("Chạy handlers song song nếu true, nếu false sẽ chạy tuần tự")]
        public bool runHandlersInParallel = true;

        public event Action<string> OnSceneReady;

        private bool _isReady = false;

        private void Start()
        {
            StartCoroutine(CoInitializeScene());
        }

        private IEnumerator CoInitializeScene()
        {
            string sceneName = gameObject.scene.name;

            var handlers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<ISceneLoadHandler>()
                .ToArray();


            if (handlers == null || handlers.Length == 0)
            {
                yield return new WaitForEndOfFrame();
                yield return null;
                _isReady = true;
                OnSceneReady?.Invoke(sceneName);
                yield break;
            }

            float startTime = Time.realtimeSinceStartup;

            if (runHandlersInParallel)
            {
                var completions = new bool[handlers.Length];

                for (int i = 0; i < handlers.Length; i++)
                {
                    int idx = i;
                    StartCoroutine(HandlerWrapper(handlers[idx], () => completions[idx] = true));
                }

                while (!completions.All(x => x) && (Time.realtimeSinceStartup - startTime) < overallTimeoutSeconds)
                    yield return null;

                if (!completions.All(x => x))
                {
                    Debug.LogWarning($"[SceneInitializationManager] Timeout waiting handlers in scene '{sceneName}'.");
                }
            }
            else
            {
                for (int i = 0; i < handlers.Length; i++)
                {
                    var wait = handlers[i].WaitForReady();
                    if (wait != null)
                        yield return StartCoroutine(wait);

                    if ((Time.realtimeSinceStartup - startTime) >= overallTimeoutSeconds)
                    {
                        Debug.LogWarning($"[SceneInitializationManager] Timeout while waiting handler {i} in '{sceneName}'.");
                        break;
                    }
                }
            }

            // final small wait to ensure rendering ready
            yield return new WaitForEndOfFrame();
            yield return null;

            _isReady = true;
            OnSceneReady?.Invoke(sceneName);
        }

        private IEnumerator HandlerWrapper(ISceneLoadHandler handler, Action onComplete)
        {
            var wait = handler.WaitForReady();
            if (wait != null)
                yield return StartCoroutine(wait);
            onComplete?.Invoke();
        }

        public bool IsReady => _isReady;
    }

    #endregion
}
