using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using echo17.EndlessBook;
using echo17.EndlessBook.Demo02;
using UnityEngine.UIElements;

/// <summary>
/// Trang main menu tương tác: chứa New Game, Continue, Quit.
/// Xử lý hover màu và click hành động.
/// </summary>
public class MainMenuPageView : PageView
{
    [Serializable]
    public struct MenuItem
    {
        public GameObject targetObject;
        public MenuAction action;
        public Renderer targetRenderer; // Material sẽ đổi màu
        public Color normalColor;
        public Color hoverColor;
    }

    public enum MenuAction
    {
        NewGame,
        ContinueGame,
        QuitGame
    }

    public MenuItem[] menuItems;

    public Camera overrideCamera; // Cho phép gán camera thủ công nếu cần

    private MenuItem? currentHovered = null;

    private float checkInterval = 0.1f;
    private float lastCheckTime;

    void Update()
    {
        Vector2 screenPoint = Input.mousePosition;

        // Chuyển từ Screen (pixel) sang Viewport (0–1)
        Camera cam = overrideCamera != null ? overrideCamera : Camera.main;
        Vector2 normalizedViewportPoint = cam.ScreenToViewportPoint(screenPoint);

        RayCast(normalizedViewportPoint, null);
    }

    /// <summary>
    /// Xử lý va chạm raycast với các mục menu.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    protected override bool HandleHit(RaycastHit hit, BookActionDelegate action)
    {
        foreach (var item in menuItems)
        {
            if (hit.collider.gameObject == item.targetObject)
            {
                Highlight(item);
                return true;
            }
        }
        return false;
    }

    public override void TouchDown()
    {
        // Clear highlight
        ClearHighlight();
    }

    public override void Drag(Vector2 delta, bool release)
    {
        if (release)
        {
            ClearHighlight();
        }
    }

    public override bool RayCast(Vector2 normalizedHitPoint, BookActionDelegate action)
    {
        if (Time.time - lastCheckTime < checkInterval) return false; // Chỉ kiểm tra mỗi 0.1 giây để tránh quá tải
        lastCheckTime = Time.time;

        Camera cam = overrideCamera != null ? overrideCamera : Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(normalizedHitPoint.x, normalizedHitPoint.y, 0));

        Vector3 rayStart = cam.transform.position; // Vị trí bắt đầu raycast là camera
        Vector3 rayDirection = (ray.origin + ray.direction * maxRayCastDistance) - rayStart;

        Debug.DrawLine(rayStart, rayStart + rayDirection, Color.red, 0.1f, true);

        if (Physics.Raycast(rayStart, rayDirection, out var hit, maxRayCastDistance, raycastLayerMask))
        {
            Debug.Log("[MainMenu] Ray hit: " + hit.collider.name);

            foreach (var item in menuItems)
            {
                if (hit.collider.gameObject == item.targetObject)
                {
                    Highlight(item);
                    return false; // Chỉ highlight khi chưa click
                }
            }
        }
        else
        {
            Debug.Log("[MainMenu] Ray hit nothing");
            ClearHighlight();
        }

        return false; // Không click action trong auto raycast
    }

    private void Highlight(MenuItem item)
    {
        ClearHighlight();
        item.targetRenderer.material.color = item.hoverColor;
        currentHovered = item;
    }

    private void ClearHighlight()
    {
        if (currentHovered.HasValue)
        {
            var item = currentHovered.Value;
            item.targetRenderer.material.color = item.normalColor;
            currentHovered = null;
        }
    }
}
