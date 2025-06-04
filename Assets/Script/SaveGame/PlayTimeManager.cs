using System;
using UnityEngine;

public class PlayTimeManager : MonoBehaviour
{
    private float sessionPlayTime;
    public bool isCounting;
    private UserAccountManager userAccountManager;

    public float SessionPlayTime => sessionPlayTime;

    void Awake()
    {
        userAccountManager = GetComponent<UserAccountManager>();
        if (userAccountManager == null)
        {
            Debug.LogError("UserAccountManager is not assigned!");
        }
        sessionPlayTime = 0f;
        isCounting = false;
        //Debug.Log("PlayTimeManager initialized");
    }

    void Update()
    {
        if (isCounting)
        {
            sessionPlayTime += Time.deltaTime;
        }
    }

    public void StartCounting()
    {
        isCounting = true;
        //Debug.Log("Started counting PlayTime");
    }

    public void StopCounting()
    {
        isCounting = false;
        Debug.Log("Stopped counting PlayTime");
    }

    /// <summary>
    /// hàm đặt lại thời gian chơi trong phiên hiện tại.
    /// </summary>
    public void ResetSession()
    {
        sessionPlayTime = 0f;
        Debug.Log("Reset session PlayTime");
    }

    public double GetTotalPlayTime()
    {
        return userAccountManager.GetPlayTime() + sessionPlayTime;
    }

    public string FormatPlayTime(double totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
        int days = timeSpan.Days;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        return $"{days} {hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}