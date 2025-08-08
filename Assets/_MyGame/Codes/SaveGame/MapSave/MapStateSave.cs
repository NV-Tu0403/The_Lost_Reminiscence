using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class MapStateWrapper
{
    public string mapName;
    public List<MapObjectState> objects;
}

[System.Serializable]
public class MapObjectState
{
    public string id;

    public bool hasIsActive;
    public bool isActive;

    public bool hasPosition;
    public Vector3 position;

    public bool hasRotation;
    public Vector3 rotation;
}


public class MapStateSave : MonoBehaviour, ISaveable
{
    public static MapStateSave Instance { get; private set; }
    public string FileName => "MapState.json";
    private string currentMap => PlayerCheckPoint.Instance?.CurrentMap ?? "Unknown";
    private bool _isDirty;

    public bool IsDirty => _isDirty;

    private Dictionary<string, MapObjectState> objectStates = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public bool ShouldSave() => currentMap != "Unknown";

    public void BeforeSave()
    {
        objectStates.Clear();

        var allObjects = FindObjectsByType<MapStateObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            string id = obj.UniqueID;
            if (string.IsNullOrEmpty(id)) continue;

            objectStates[id] = obj.CaptureState();
        }
    }

    public string SaveToJson()
    {
        if (!ShouldSave()) return string.Empty;

        var wrapper = new MapStateWrapper
        {
            mapName = currentMap,
            objects = objectStates.Values.ToList()
        };

        return JsonUtility.ToJson(wrapper, true);
    }

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        var wrapper = JsonUtility.FromJson<MapStateWrapper>(json);
        if (wrapper == null || wrapper.objects == null) return;

        objectStates = wrapper.objects.ToDictionary(o => o.id, o => o);
    }

    public void AfterLoad()
    {
        // Apply lại trạng thái khi map load xong
        ApplyMapState();
    }

    /// <summary>
    /// Áp dụng trạng thái đã lưu cho tất cả các đối tượng trong map hiện tại.
    /// </summary>
    public void ApplyMapState()
    {
        var allObjects = FindObjectsByType<MapStateObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (objectStates.TryGetValue(obj.UniqueID, out var state))
            {
                obj.ApplyState(state);
                //Debug.Log($"[MapStateSave] Applied state for object {obj.UniqueID} in map {currentMap}");   
            }
        }
    }
}
