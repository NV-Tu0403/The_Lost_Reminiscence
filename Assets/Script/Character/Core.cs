using UnityEngine;
using System;

public class Core : MonoBehaviour
{
    public static Core Instance { get; private set; }
    public static bool IsInitialized => Instance != null;   
    public bool IsOffline { get; private set; } = true;     // Mặc định là online khi khởi động
    //public event Action OnModeChanged;                      // Sự kiện khi chế độ thay đổi

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            // Đặt game object cha (nếu có) thành DontDestroyOnLoad
            if (transform.parent != null)
            {
                DontDestroyOnLoad(transform.parent.gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }

            // Đặt gameObject "canvas" thành DontDestroyOnLoad nếu tồn tại trong scene
            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                DontDestroyOnLoad(canvasObj);
            }
            //Debug.Log("Core initialized.");
        }
        else
        {
            Debug.LogWarning("Core instance already exists!");
        }
    }

    public virtual void update() { }
}

/// <summary>
/// Bộ đếm thời gian.
/// </summary>
public class Timer
{
    public float _startTime;
    public bool _isCounting;

    /// <summary>
    /// Cập nhật bộ đếm thời gian.
    /// </summary>
    /// <param name="isCount"></param>
    /// <returns></returns>
    public float UpdateTimer(bool isCount)
    {
        if (isCount && !_isCounting) // Nếu bắt đầu đếm
        {
            _startTime = Time.time;
            _isCounting = true;
        }
        else if (!isCount && _isCounting) // Nếu dừng đếm
        {
            _isCounting = false;
            return Time.time - _startTime; // Trả về thời gian đã đếm
        }
        return 0f; // Nếu không đếm thì trả về 0
    }
}
