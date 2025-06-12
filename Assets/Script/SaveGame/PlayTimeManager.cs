using System;
using UnityEngine;

public class PlayTimeManager : MonoBehaviour, ISaveable
{
    public static PlayTimeManager Instance { get; private set; }
    private float sessionPlayTime;
    private bool _isDirty;
    public bool isCounting;
    private readonly object saveLock = new object(); // Lock to avoid race condition

    public string FileName => "playTime.json";
    public float SessionPlayTime => sessionPlayTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        sessionPlayTime = 0f;
        isCounting = false;
        _isDirty = false;
    }

    private void Update()
    {
        if (isCounting)
        {
            lock (saveLock)
            {
                sessionPlayTime += Time.deltaTime;
                _isDirty = true; // Mark as dirty when time changes
            }
        }
    }

    public void StartCounting()
    {
        lock (saveLock)
        {
            isCounting = true;
            _isDirty = true; // Mark as dirty when counting starts
        }
    }

    public void StopCounting()
    {
        lock (saveLock)
        {
            isCounting = false;
            _isDirty = true; // Mark as dirty when counting stops
        }
    }

    public void ResetSession()
    {
        lock (saveLock)
        {
            sessionPlayTime = 0f;
            _isDirty = true; // Mark as dirty on reset
        }
        Debug.Log("Reset session PlayTime");
    }

    public bool ShouldSave()
    {
        lock (saveLock)
        {
            // Only save if counting is active or time has been modified
            return isCounting || _isDirty;
        }
    }

    public bool IsDirty
    {
        get
        {
            lock (saveLock)
            {
                return _isDirty;
            }
        }
    }

    public void BeforeSave()
    {
        lock (saveLock)
        {
            // No additional prep needed, but can add logic if required
            _isDirty = false; // Reset dirty flag before saving
        }
    }

    public void AfterLoad()
    {
        lock (saveLock)
        {
            _isDirty = false; // Reset dirty flag after loading
            // Optionally adjust isCounting based on loaded state
            isCounting = sessionPlayTime > 0f; // Resume counting if time exists
        }
    }

    public string SaveToJson()
    {
        lock (saveLock)
        {
            try
            {
                return JsonUtility.ToJson(new PlayTimeData { sessionPlayTime = sessionPlayTime });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize PlayTimeData: {e.Message}");
                return string.Empty;
            }
        }
    }

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("JSON data is empty or null");
            return;
        }

        lock (saveLock)
        {
            try
            {
                var data = JsonUtility.FromJson<PlayTimeData>(json);
                sessionPlayTime = data.sessionPlayTime;
                _isDirty = false; // Data is clean after load
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize PlayTimeData: {e.Message}");
                sessionPlayTime = 0f;
                _isDirty = false;
            }
        }
    }

    public string FormatPlayTime()
    {
        lock (saveLock)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(sessionPlayTime);
            return $"{timeSpan.Days} {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }

    [Serializable]
    private class PlayTimeData
    {
        public float sessionPlayTime;
    }
}