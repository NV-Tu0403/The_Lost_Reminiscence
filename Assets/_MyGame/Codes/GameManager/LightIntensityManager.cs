using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LightIntensityManager : MonoBehaviour
{
    public Slider globalLightSlider; // // tham chiếu slider (0..1)
    public float epsilon = 0.001f;   // // tránh cập nhật nhỏ
    private float currentMultiplier = 1f;

    // lưu intensity gốc theo instanceID để restore đúng
    [SerializeField] private Dictionary<int, float> originalIntensity = new Dictionary<int, float>();
    // tham chiếu Light để cập nhật nhanh
    [SerializeField] private List<Light> trackedLights = new List<Light>();

    void Awake()
    {
        if (globalLightSlider != null)
        {
            globalLightSlider.onValueChanged.AddListener(OnSliderChanged);
            currentMultiplier = globalLightSlider.value;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RegisterAllLightsInLoadedScenes();
        // nếu muốn lấy lights từ các object DontDestroyOnLoad, có thể scan root của đó.
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (globalLightSlider != null)
            globalLightSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    // gọi khi load scene mới
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterLightsInScene(scene);
        ApplyMultiplierToAll(currentMultiplier); // áp ngay cho lights mới
    }

    // slider callback
    private void OnSliderChanged(float value)
    {
        if (Mathf.Abs(value - currentMultiplier) < epsilon) return;
        currentMultiplier = value;
        ApplyMultiplierToAll(currentMultiplier);
    }

    // áp multiplier cho tất cả lights đã track
    private void ApplyMultiplierToAll(float multiplier)
    {
        for (int i = 0; i < trackedLights.Count; i++)
        {
            var light = trackedLights[i];
            if (light == null) continue; // object bị destroy
            int id = light.GetInstanceID();
            if (!originalIntensity.TryGetValue(id, out float orig))
            {
                // nếu chưa có orig (ví dụ mới tạo runtime) thì set và lưu
                orig = light.intensity;
                originalIntensity[id] = orig;
            }
            light.intensity = orig * multiplier; // // apply
        }
    }

    // quét tất cả scene đã load
    private void RegisterAllLightsInLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            RegisterLightsInScene(s);
        }
        ApplyMultiplierToAll(currentMultiplier);
    }

    // lấy root objects của scene và tìm Light trong cây (bao gồm inactive)
    private void RegisterLightsInScene(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var lights = root.GetComponentsInChildren<Light>(true); // include inactive
            foreach (var l in lights)
            {
                AddLightIfNotTracked(l);
            }
        }
    }

    // thêm light vào danh sách theo instanceID
    private void AddLightIfNotTracked(Light l)
    {
        if (l == null) return;
        int id = l.GetInstanceID();
        if (!originalIntensity.ContainsKey(id))
        {
            originalIntensity[id] = l.intensity;
            trackedLights.Add(l);
        }
    }

    // nếu cần reset về gốc
    public void ResetAllToOriginal()
    {
        foreach (var kv in originalIntensity)
        {
            // tìm Light bằng instance id (không có direct mapping nhanh)
            // simpler: loop trackedLights
        }
        for (int i = 0; i < trackedLights.Count; i++)
        {
            var light = trackedLights[i];
            if (light == null) continue;
            int id = light.GetInstanceID();
            if (originalIntensity.TryGetValue(id, out float orig))
                light.intensity = orig;
        }
        currentMultiplier = 1f;
        if (globalLightSlider) globalLightSlider.value = 1f;
    }
}
