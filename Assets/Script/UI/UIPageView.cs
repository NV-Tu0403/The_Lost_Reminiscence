using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using echo17.EndlessBook;
using echo17.EndlessBook.Demo02;
using UnityEngine.UIElements;
using TMPro;
using Duckle;
using static echo17.EndlessBook.EndlessBook;


/// <summary>
/// Đại diện cho một mục menu với thông tin về hành động, đối tượng collider và renderer.
/// </summary>
[Serializable]
public struct UIItem
{
    public UIActionType uIActionType;
    public GameObject targetColliderObject;
    public Renderer targetRenderer; // Hỗ trợ Renderer hoặc TMP_Text
    public Color normalColor;
    public Color hoverColor;
    public int targetPage; // Dùng cho TurnToPage
    public float turnTime;

}

/// <summary>
/// Trang main menu tương tác: chứa New Game, Continue, Quit.
/// Xử lý hover màu và click hành động.
/// </summary>
/// 
public class UIPageView : PageView
{
#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// New Input System actions mapping
    /// </summary>
    protected BookInputActions _bookInputActions;
#endif

    //[SerializeField] private Camera overrideCamera;
    [SerializeField] private float checkInterval = 0.1f;
    [SerializeField] private bool debugMode = false;

    [SerializeField] private UIItem[] menuItems;

    private EndlessBook book;
    private BookActionDelegate _bookActionDelegate; // delegate để xử lý hành động từ EndlessBook
    private UIItem? currentHovered;
    private float lastCheckTime;

    protected new void Awake()
    {
        base.Awake();
        book = FindFirstObjectByType<EndlessBook>();
        // giá trị mặc định cho raycastLayerMask hoặc maxRayCastDistance nếu cần
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            TryClick(Input.mousePosition, _bookActionDelegate); // delegate nên cache sẵn
        }
        if (Input.GetMouseButtonUp(0))
        {

        }
#endif

//#if ENABLE_INPUT_SYSTEM
//        if (_bookInputActions.TouchPad.Press.WasPressedThisFrame())
//        {

//        }
//        else if (_bookInputActions.TouchPad.Press.WasReleasedThisFrame())
//        {

//        }
//#endif

        if (Time.time - lastCheckTime > checkInterval)
        {
            lastCheckTime = Time.time;
            TryHover(Input.mousePosition);
        }

        if (debugMode) // Vẽ raycast để debug
        {
            DebugRayCast();
        }
    }
    #region debug
    private void DebugRayCast()
    {
        Camera cam = pageViewCamera != null ? pageViewCamera : Camera.main;
        // Chuyển vị trí chuột từ không gian màn hình sang không gian thế giới tại mặt phẳng z=0
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
        // Tạo ray với điểm gốc tại chuột và hướng -Z
        Ray ray = new Ray(mouseWorldPos, Vector3.forward);
        // Vẽ ray từ điểm gốc đến điểm cuối (khoảng cách maxRayCastDistance)
        Debug.DrawLine(ray.origin, ray.direction * maxRayCastDistance, Color.green, 0.1f);
    }
    #endregion

    public override void Activate()
    {
        gameObject.SetActive(true);
        currentHovered = null;
        lastCheckTime = 0f;
        if (debugMode)
        {
            Debug.Log($"[UIPageView] Activated on {gameObject.name}");
        }
    }

    public override void Deactivate()
    {
        ClearHighlight();
        gameObject.SetActive(false);
        if (debugMode)
        {
            Debug.Log($"[UIPageView] Deactivated on {gameObject.name}");
        }
    }
    
    public override void TouchDown()
    {
        // Optional: Handle touch down if needed (e.g., start animation)
        ClearHighlight();
    }

    //public override bool RayCast(Vector2 normalizedHitPoint, BookActionDelegate action)
    //{
    //    if (Time.time - lastCheckTime < checkInterval) return false; // Chỉ kiểm tra mỗi 0.1 giây để tránh quá tải
    //    lastCheckTime = Time.time;

    //    Camera cam = pageViewCamera != null ? pageViewCamera : Camera.main;
    //    Ray ray = cam.ViewportPointToRay(new Vector3(normalizedHitPoint.x, normalizedHitPoint.y, 0));

    //    Vector3 rayStart = cam.transform.position; // Vị trí bắt đầu raycast là camera
    //    Vector3 rayDirection = (ray.origin + ray.direction * maxRayCastDistance) - rayStart;

    //    Debug.DrawLine(rayStart, rayStart + rayDirection, Color.red, 0.1f, true);

    //    if (Physics.Raycast(rayStart, rayDirection, out var hit, maxRayCastDistance, raycastLayerMask))
    //    {
    //        if (debugMode)
    //        {
    //            Debug.Log("[MainMenu] Ray hit: " + hit.collider.name);
    //        }
    //        foreach (var item in menuItems)
    //        {
    //            if (item.targetColliderObject != null && hit.collider.gameObject == item.targetColliderObject)
    //            {
    //                Highlight(item);

    //                // gọi HandleHit để xử lý hành động click
    //                return HandleHit(hit, action);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("[MainMenu] Ray hit nothing");
    //        ClearHighlight();
    //    }

    //    return false; // Không click action trong auto raycast
    //}

    private bool RaycastAt(Vector2 screenPoint, out RaycastHit hit, out UIItem item)
    {
        hit = default;
        item = default;

        Camera cam = pageViewCamera != null ? pageViewCamera : Camera.main;
        Ray ray = cam.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out hit, maxRayCastDistance, raycastLayerMask))
        {
            foreach (var i in menuItems)
            {
                if (i.targetColliderObject == hit.collider.gameObject)
                {
                    item = i;
                    return true;
                }
            }
        }

        return false;
    }

    private void TryHover(Vector2 screenPoint)
    {
        if (RaycastAt(screenPoint, out var hit, out var item))
        {
            Highlight(item);
        }
        else
        {
            ClearHighlight();
        }
    }

    public void TryClick(Vector2 screenPoint, BookActionDelegate action)
    {
        if (RaycastAt(screenPoint, out var hit, out var item))
        {
            HandleClick(item, action);
        }
    }

    /// <summary>
    /// xác nhận hit từ raycast và xử lý hành động tương ứng với mục tiêu.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    protected override bool HandleHit(RaycastHit hit, BookActionDelegate action)
    {
        foreach (var item in menuItems)
        {
            if (item.targetColliderObject != null && hit.collider.gameObject == item.targetColliderObject)
            {
                //Highlight(item);
                if (Input.GetMouseButtonDown(0))
                {
                    if (book != null)
                    {
                        HandleClick(item, action);
                    }
                }
                return true;
            }
        }

        ClearHighlight();
        return false;
    }

    /// <summary>
    /// cấu hình hành động khi người dùng click vào một mục tiêu.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="action"></param>
    private void HandleClick(UIItem item, BookActionDelegate action)
    {
        if (debugMode)
        {
            Debug.Log($"[UIPageView] Clicked on {item.uIActionType}");
        }

        //EnsureBookOpenMiddle(); // bắt lỗi test

        switch (item.uIActionType)
        {
            case UIActionType.NewGame:
                togglePage(item);
                //action?.Invoke(BookActionTypeEnum.ChangeState, 2); // OpenMiddle
                break;

            case UIActionType.Continue:
                togglePage(item);
                //action?.Invoke(BookActionTypeEnum.TurnPage, item.targetPage); 
                break;

            case UIActionType.SavePanel:
                togglePage(item);
                break;

            case UIActionType.QuitGame:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;

            case UIActionType.TurnToPage: // điều hướng đến trang cụ thể (test mục lục số)
                action?.Invoke(BookActionTypeEnum.TurnPage, item.targetPage);
                break;
        }
    }

    /// <summary>
    /// truy cập trực tiếp vào trang sách EndlessBook để chuyển đến trang cụ thể. (test)
    /// </summary>
    /// <param name="item"></param>
    private void togglePage(UIItem item)
    {
        book.TurnToPage(item.targetPage, PageTurnTimeTypeEnum.TotalTurnTime, item.turnTime);

    }

    //private void EnsureBookOpenMiddle()
    //{
    //    EndlessBook book = FindFirstObjectByType<EndlessBook>();
    //    if (book != null && book.CurrentState != EndlessBook.StateEnum.OpenMiddle)
    //    {
    //        book.SetState(EndlessBook.StateEnum.OpenMiddle); // Trực tiếp gọi vào book
    //    }
    //}

    /// <summary>
    /// Highlight mục menu khi hover chuột.
    /// </summary>
    /// <param name="item"></param>
    private void Highlight(UIItem item)
    {

        if (currentHovered.HasValue && currentHovered.Value.targetRenderer == item.targetRenderer)
        {
            //Debug.Log("[MainMenu] Already highlighting: " + item.targetRenderer.name);
            return; // Đã highlight mục này rồi, không cần làm gì thêm
        }

        ClearHighlight();

        if (item.targetRenderer != null && item.targetRenderer.material.HasProperty("_Color"))
        {
            item.targetRenderer.material.color = item.hoverColor;
            //Debug.Log("[MainMenu] Highlighting: " + item.targetRenderer.name + " with color: " + item.hoverColor);
        }
        else
        {
            var tmp = item.targetRenderer.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = item.hoverColor;
                //Debug.Log("[MainMenu] Highlighting TMP_Text: " + tmp.name + " with color: " + item.hoverColor);
            }
            else
            {
                var fallback = item.targetRenderer.GetComponent<Renderer>();
                if (fallback != null && fallback.material.HasProperty("_Color"))
                {
                    fallback.material.color = item.hoverColor;
                    //Debug.Log("[MainMenu] Highlighting fallback Renderer: " + fallback.name + " with color: " + item.hoverColor);
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
