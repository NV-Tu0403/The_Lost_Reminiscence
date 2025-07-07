using UnityEngine;

public class Core : CoreEventListenerBase
{
    public static Core Instance { get; private set; }
    public bool IsOffline { get; private set; } = true;     // Mặc định là online khi khởi động                                               

    public GameObject menuCamera;
    public GameObject MainMenu;


    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //else
        //{
        //    Destroy(gameObject);
        //}

        InitializeMenuCamera();
    }

    public override void RegisterEvent(CoreEvent e)
    {
        e.TurnOnMenu += TurnOnMenu;
        e.TurnOffMenu += TurnOffMenu;

        e.OnQuitGame += QuitGame;
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.TurnOnMenu -= TurnOnMenu;
        e.TurnOffMenu -= TurnOffMenu;

        e.OnQuitGame -= QuitGame;
    }

    /// <summary>
    /// đảm bảo rằng MenuCamera đã được khởi tạo và kích hoạt.
    /// </summary>
    private void InitializeMenuCamera()
    {
        if (menuCamera == null)
        {
            menuCamera = GameObject.Find("MenuCamera");
            if (menuCamera == null)
            {
                menuCamera = Resources.Load<GameObject>("Prefab Loaded/MenuCamera");
                //Debug.Log("MenuCamera loaded from Resources.");
            }
            if (!menuCamera.activeSelf)
            {
                menuCamera.SetActive(true);
                //Debug.Log("MenuCamera initialized and activated successfully.");
            }
        }
    }

    private void TurnOnMenu()
    {
        menuCamera.SetActive(true);
        MainMenu.SetActive(true);
    }

    private void TurnOffMenu()
    {
        menuCamera.SetActive(false);
        MainMenu.SetActive(false);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
    }
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


// ----------------------------------------------------LUỒNG DỮ LIỆU:--------------------------------------------------

// - 

// ----------------------------------------------------TỐI ƯU:--------------------------------------------------

// - Attribute + Reflection	Tự động hóa 100%, ít lỗi	(Dùng Reflection, cần setup kỹ)
// -

// ----------------------------------------------------NOTE:--------------------------------------------------

// -
