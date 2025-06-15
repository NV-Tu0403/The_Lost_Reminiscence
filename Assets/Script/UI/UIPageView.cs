using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using echo17.EndlessBook;
using echo17.EndlessBook.Demo02;
using UnityEngine.UIElements;
using TMPro;
using Duckle;

/// <summary>
/// Trang main menu tương tác: chứa New Game, Continue, Quit.
/// Xử lý hover màu và click hành động.
/// </summary>
public class UIPageView : PageView
{
    /// <summary>
    /// Đại diện cho một mục menu với thông tin về hành động, đối tượng collider và renderer.
    /// </summary>
    [Serializable]
    public struct UIItem
    {
        public UIActionType uIActionType;
        public GameObject targetColliderObject;
        public Renderer targetRenderer;
        public Color normalColor;
        public Color hoverColor;
    }

    /// <summary>
    /// Danh sách các mục menu với thông tin về hành động, đối tượng collider và renderer.
    /// </summary>
    public UIItem[] menuItems;

    /// <summary>
    /// Camera để thực hiện raycast và tương tác với menu.
    /// </summary>
    public Camera overrideCamera;               // Cho phép gán camera thủ công nếu cần

    private UIItem? currentHovered = null;

    private float checkInterval = 0.1f;
    private float lastCheckTime;

    void Update()
    {
        Vector2 screenPoint = Input.mousePosition;
        Camera cam = overrideCamera != null ? overrideCamera : Camera.main;
        Vector2 viewportPoint = cam.ScreenToViewportPoint(screenPoint);
        RayCast(viewportPoint, null);
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
            if (item.targetRenderer != null && hit.collider.gameObject == item.targetRenderer.gameObject)
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

    /// <summary>
    /// Thực hiện raycast từ camera để tìm mục menu tương ứng.
    /// </summary>
    /// <param name="normalizedHitPoint"></param>
    /// <param name="action"></param>
    /// <returns></returns>
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
                if (item.targetColliderObject != null && hit.collider.gameObject == item.targetColliderObject)
                {
                    Highlight(item);
                    return false;
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

    /// <summary>
    /// Highlight mục menu khi hover chuột.
    /// </summary>
    /// <param name="item"></param>
    private void Highlight(UIItem item)
    {

        if (currentHovered.HasValue && currentHovered.Value.targetRenderer == item.targetRenderer)
        {
            Debug.Log("[MainMenu] Already highlighting: " + item.targetRenderer.name);
            return; // Đã highlight mục này rồi, không cần làm gì thêm
        }

        ClearHighlight();

        if (item.targetRenderer != null && item.targetRenderer.material.HasProperty("_Color"))
        {
            item.targetRenderer.material.color = item.hoverColor;
            Debug.Log("[MainMenu] Highlighting: " + item.targetRenderer.name + " with color: " + item.hoverColor);
        }
        else
        {
            var tmp = item.targetRenderer.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = item.hoverColor;
                Debug.Log("[MainMenu] Highlighting TMP_Text: " + tmp.name + " with color: " + item.hoverColor);
            }
            else
            {
                var fallback = item.targetRenderer.GetComponent<Renderer>();
                if (fallback != null && fallback.material.HasProperty("_Color"))
                {
                    fallback.material.color = item.hoverColor;
                    Debug.Log("[MainMenu] Highlighting fallback Renderer: " + fallback.name + " with color: " + item.hoverColor);
                }
            }
        }

        currentHovered = item;
    }

    /// <summary>
    /// Xóa highlight khỏi mục menu hiện tại nếu có.
    /// </summary>
    private void ClearHighlight()
    {
        if (currentHovered.HasValue)
        {
            var item = currentHovered.Value;

            if (item.targetRenderer != null && item.targetRenderer.material.HasProperty("_Color"))
            {
                item.targetRenderer.material.color = item.normalColor;
            }
            else
            {
                var tmp = item.targetRenderer.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.color = item.normalColor;
                }
                else
                {
                    var fallback = item.targetRenderer.GetComponent<Renderer>();
                    if (fallback != null && fallback.material.HasProperty("_Color"))
                    {
                        fallback.material.color = item.normalColor;
                    }
                }
            }

            currentHovered = null;
        }
    }
}
