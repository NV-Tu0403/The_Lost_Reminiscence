using System;
using UnityEngine;

public class PlayTimeManager : MonoBehaviour, ISaveable
{
    private float sessionPlayTime;
    public bool isCounting;
    private readonly object saveLock = new object(); // Khóa để tránh race condition

    public string FileName => "playTime.json";
    public float SessionPlayTime => sessionPlayTime;

    private void Awake()
    {
        sessionPlayTime = 0f;
        isCounting = false;
    }

    private void Update()
    {
        if (isCounting)
        {
            lock (saveLock) // Bảo vệ sessionPlayTime
            {
                sessionPlayTime += Time.deltaTime;
            }
        }
    }

    public void StartCounting()
    {
        isCounting = true;
    }

    public void StopCounting()
    {
        isCounting = false;
    }

    public void ResetSession()
    {
        lock (saveLock)
        {
            sessionPlayTime = 0f;
        }
        Debug.Log("Reset session PlayTime");
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
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize PlayTimeData: {e.Message}");
                sessionPlayTime = 0f;
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