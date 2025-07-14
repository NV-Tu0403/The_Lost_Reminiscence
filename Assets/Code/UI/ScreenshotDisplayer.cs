using System.IO;
using UnityEngine;

public class ScreenshotDisplayer : CoreEventListenerBase
{
    public Renderer[] targetRenderers; // Gán Plane hoặc object 3D cần hiển thị ảnh
    public string screenshotPath;

    private void Update()
    {
        screenshotPath = ProfessionalSkilMenu.Instance.SelectedSaveImagePath;
    }

    public override void RegisterEvent(CoreEvent e)
    {
        e.OnSelectSaveItem += LoadScreenshotToPlane;
    }

    public override void UnregisterEvent(CoreEvent e)
    {
        e.OnSelectSaveItem -= LoadScreenshotToPlane;
    }
    public void LoadScreenshotToPlane()
    {
        if (targetRenderers == null)
        {
            Debug.LogError("[ScreenshotDisplayer] targetRenderer is not assigned!");
            return;
        }

        if (!File.Exists(screenshotPath))
        {
            Debug.LogError($"[ScreenshotDisplayer] File not found at: {screenshotPath}");
            return;
        }

        byte[] imageBytes = File.ReadAllBytes(screenshotPath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes); // Tự resize dựa trên ảnh

        foreach (var renderer in targetRenderers)
        {
            if (renderer == null) continue;

            // Đảm bảo mỗi renderer có vật liệu riêng (tránh ảnh hưởng toàn bộ prefab)
            Material materialInstance = renderer.material; // Tạo instance riêng
            materialInstance.mainTexture = texture;
        }

        Debug.Log($"[ScreenshotDisplayer] Screenshot loaded into Plane from: {screenshotPath}");
    }
}
