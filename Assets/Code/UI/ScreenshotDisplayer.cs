using System.IO;
using UnityEngine;

public class ScreenshotDisplayer : CoreEventListenerBase
{
    public static ScreenshotDisplayer Instance { get; private set; }

    public Renderer[] targetRenderers; // Gán Plane hoặc object 3D cần hiển thị ảnh
    string defaultPath = "Assets/Art/Background/Image/z6579043891587_74bb0e45eb0ddde2694f7c2c53fd4ad6.jpg";

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            Debug.LogError("[ScreenshotDisplayer] targetRenderers is not assigned or empty!");
        }

        // load ảnh mặc định
        if (File.Exists(defaultPath))
        {
            LoadScreenshotToPlane(defaultPath);
        }
        else
        {
            Debug.LogWarning($"[ScreenshotDisplayer] Default screenshot not found at: {defaultPath}");
        }
    }

    //private void Update()
    //{
    //    screenshotPath = ProfessionalSkilMenu.Instance.SelectedSaveImagePath;
    //}

    public override void RegisterEvent(CoreEvent e)
    {
        //e.OnSelectSaveItem += (path) => LoadScreenshotToPlane(path) ;
        //e.OnSelectSaveItem += LoadScreenshotToPlane;
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        //e.OnSelectSaveItem -= LoadScreenshotToPlane;
    }

    public void LoadScreenshotToPlane(string path)
    {
        if (targetRenderers == null)
        {
            Debug.LogError("[ScreenshotDisplayer] targetRenderer is not assigned!");
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError($"[ScreenshotDisplayer] File not found at: {path}");
            return;
        }

        byte[] imageBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes); // Tự resize dựa trên ảnh

        foreach (var renderer in targetRenderers)
        {
            if (renderer == null) continue;

            // Đảm bảo mỗi renderer có vật liệu riêng (tránh ảnh hưởng toàn bộ prefab)
            Material materialInstance = renderer.material; // Tạo instance riêng
            materialInstance.mainTexture = texture;
        }

        //Debug.Log($"[ScreenshotDisplayer] Screenshot loaded into Plane from: {path}");
    }
}
