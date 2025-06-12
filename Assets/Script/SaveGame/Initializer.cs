using System;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> singletons = new List<MonoBehaviour>();
    public static event Action OnInitializationComplete;

    private void Awake()
    {
        // Kiểm tra các singleton được gán
        foreach (var singleton in singletons)
        {
            if (singleton == null)
            {
                Debug.LogError($"[Initializer] A singleton is not assigned in the Inspector!");
            }
            else
            {
                Debug.Log($"[Initializer] Found singleton: {singleton.GetType().Name}");
            }
        }
    }

    private void Start()
    {
        InitializeSingletons();
    }

    private void InitializeSingletons()
    {
        // Khởi tạo tuần tự các singleton
        foreach (var singleton in singletons)
        {
            if (singleton == null) continue;

            // Kiểm tra xem singleton có implement IInitializable không
            if (singleton is IInitializable initializable)
            {
                try
                {
                    initializable.Initialize();
                    Debug.Log($"[Initializer] Initialized {singleton.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Initializer] Failed to initialize {singleton.GetType().Name}: {ex.Message}");
                }
            }
            else
            {
                // Giả định singleton đã khởi tạo trong Awake
                Debug.Log($"[Initializer] {singleton.GetType().Name} assumed initialized via Awake.");
            }
        }

        // Phát sự kiện khi hoàn tất
        OnInitializationComplete?.Invoke();
        Debug.Log("[Initializer] All singletons initialized successfully.");
    }

    private void OnDestroy()
    {
        OnInitializationComplete = null;
    }
}

// Interface cho các singleton cần khởi tạo đặc biệt
public interface IInitializable
{
    void Initialize();
}