using System;
using UnityEngine;
using echo17.EndlessBook;
using echo17.EndlessBook.Demo02;
using TMPro;
using static echo17.EndlessBook.EndlessBook;


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
    public int targetPage;
    public float turnTime;

}

/// <summary>
/// Trang main menu tương tác: chứa New Game, Continue, Quit.
/// Xử lý hover màu và click hành động.
/// </summary>
/// 
public class UIPageView : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// New Input System actions mapping
    /// </summary>
    protected BookInputActions _bookInputActions;
#endif

    [SerializeField] private Camera pageViewCamera; // Camera để raycast
    [SerializeField] private LayerMask raycastLayerMask = -1; // Layer mask cho raycast
    [SerializeField] private float maxRayCastDistance = 100f; // Khoảng cách tối đa raycast
    [SerializeField] private float checkInterval = 0.1f;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private UIItem[] menuItems;


    private EndlessBook book;
    private UIItem? currentHovered;
    private float lastCheckTime;

    // Delegates để thông báo sự kiện tới Demo02, tương tự TouchPad
    public Action<Vector2, UIItem> onTouchDownDetected;
    public Action<Vector2, UIItem, bool> onTouchUpDetected;
    public Action<Vector2> onHoverDetected;

    private void Awake()
    {
        //base.Awake();
        book = FindFirstObjectByType<EndlessBook>();
        // giá trị mặc định cho raycastLayerMask hoặc maxRayCastDistance nếu cần
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            DetectTouchDown(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            DetectTouchUp(Input.mousePosition);
        }
#endif

        if (Time.time - lastCheckTime > checkInterval)
        {
            lastCheckTime = Time.time;
            DetectHover(Input.mousePosition);
        }

        if (debugMode)
        {
            DebugRayCast();
        }
    }

    #region debug
    private void DebugRayCast()
    {
        Camera cam = pageViewCamera != null ? pageViewCamera : Camera.main;
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
        Ray ray = new Ray(mouseWorldPos, Vector3.forward);
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * maxRayCastDistance, Color.green, 0.1f);
    }
    #endregion

    public void Activate()
    {
        gameObject.SetActive(true);
        currentHovered = null;
        lastCheckTime = 0f;
        if (debugMode)
        {
            Debug.Log($"[UIPageView] Activated on {gameObject.name}");
        }
    }

    public void Deactivate()
    {
        ClearHighlight();
        gameObject.SetActive(false);
        if (debugMode)
        {
            Debug.Log($"[UIPageView] Deactivated on {gameObject.name}");
        }
    }

    public void TouchDown()
    {
        ClearHighlight();
    }

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
                    if (debugMode)
                    {
                        Debug.Log($"[UIPageView] Ray hit: {hit.collider.name} with item {i.uIActionType}");
                    }
                    return true;
                }
            }
        }

        return false;
    }

    private void DetectHover(Vector2 screenPoint)
    {
        if (RaycastAt(screenPoint, out var hit, out var item))
        {
            Highlight(item);
        }
        else
        {
            ClearHighlight();
        }
        onHoverDetected?.Invoke(screenPoint);
    }

    private void DetectTouchDown(Vector2 screenPoint)
    {
        if (RaycastAt(screenPoint, out var hit, out var item))
        {
            onTouchDownDetected?.Invoke(screenPoint, item);
        }
    }

    private void DetectTouchUp(Vector2 screenPoint)
    {
        if (RaycastAt(screenPoint, out var hit, out var item))
        {
            onTouchUpDetected?.Invoke(screenPoint, item, false); // chua Không hỗ trợ drag trong này
        }
    }

//    #region tạm không dùng nũa

//    public void TryHover(Vector2 screenPoint)
//    {
//        if (RaycastAt(screenPoint, out var hit, out var item))
//        {
//            Highlight(item);
//        }
//        else
//        {
//            ClearHighlight();
//        }
//    }

//    public void TryClick(Vector2 screenPoint, BookActionDelegate action)
//    {
//        if (RaycastAt(screenPoint, out var hit, out var item))
//        {
//            HandleClick(item, action);
//        }
//    }

//    //public override bool RayCast(Vector2 normalizedHitPoint, BookActionDelegate action)
//    //{
//    //    if (Time.time - lastCheckTime < checkInterval) return false; // Chỉ kiểm tra mỗi 0.1 giây để tránh quá tải
//    //    lastCheckTime = Time.time;

//    //    Camera cam = pageViewCamera != null ? pageViewCamera : Camera.main;
//    //    Ray ray = cam.ViewportPointToRay(new Vector3(normalizedHitPoint.x, normalizedHitPoint.y, 0));

//    //    Vector3 rayStart = cam.transform.position; // Vị trí bắt đầu raycast là camera
//    //    Vector3 rayDirection = (ray.origin + ray.direction * maxRayCastDistance) - rayStart;

//    //    Debug.DrawLine(rayStart, rayStart + rayDirection, Color.red, 0.1f, true);

//    //    if (Physics.Raycast(rayStart, rayDirection, out var hit, maxRayCastDistance, raycastLayerMask))
//    //    {
//    //        if (debugMode)
//    //        {
//    //            Debug.Log("[MainMenu] Ray hit: " + hit.collider.name);
//    //        }
//    //        foreach (var item in menuItems)
//    //        {
//    //            if (item.targetColliderObject != null && hit.collider.gameObject == item.targetColliderObject)
//    //            {
//    //                Highlight(item);

//    //                // gọi HandleHit để xử lý hành động click
//    //                return HandleHit(hit, action);
//    //            }
//    //        }
//    //    }
//    //    else
//    //    {
//    //        Debug.Log("[MainMenu] Ray hit nothing");
//    //        ClearHighlight();
//    //    }

//    //    return false; // Không click action trong auto raycast
//    //}

//    /// <summary>
//    /// xác nhận hit từ raycast và xử lý hành động tương ứng với mục tiêu.
//    /// </summary>
//    /// <param name="hit"></param>
//    /// <param name="action"></param>
//    /// <returns></returns>
//    //    private void HandleClick(UIItem item, BookActionTypeEnum actionType, int actionValue)
//    //    {
//    //        if (debugMode)
//    //        {
//    //            Debug.Log($"[UIPageView] Clicked on {item.uIActionType}");
//    //        }

//    //        switch (item.uIActionType)
//    //        {
//    //            case UIActionType.NewGame:
//    //            case UIActionType.Continue:
//    //            case UIActionType.SavePanel:
//    //                book.TurnToPage(item.targetPage, PageTurnTimeTypeEnum.TotalTurnTime, item.turnTime);
//    //                break;

//    //            case UIActionType.QuitGame:
//    //#if UNITY_EDITOR
//    //                UnityEditor.EditorApplication.isPlaying = false;
//    //#else
//    //                Application.Quit();
//    //#endif
//    //                break;

//    //            case UIActionType.TurnToPage:
//    //                book.TurnToPage(item.targetPage, PageTurnTimeTypeEnum.TotalTurnTime, item.turnTime);
//    //                break;
//    //        }
//    //    }

//    /// <summary>
//    /// cấu hình hành động khi người dùng click vào một mục tiêu.
//    /// </summary>
//    /// <param name="item"></param>
//    /// <param name="action"></param>
//    private void HandleClick(UIItem item, BookActionDelegate action)
//    {
//        if (debugMode)
//        {
//            Debug.Log($"[UIPageView] Clicked on {item.uIActionType}");
//        }

//        //EnsureBookOpenMiddle(); // bắt lỗi test

//        switch (item.uIActionType)
//        {
//            case UIActionType.NewGame:
//                togglePage(item);
//                //action?.Invoke(BookActionTypeEnum.ChangeState, 2); // OpenMiddle
//                break;

//            case UIActionType.Continue:
//                togglePage(item);
//                //action?.Invoke(BookActionTypeEnum.TurnPage, item.targetPage); 
//                break;

//            case UIActionType.SavePanel:
//                togglePage(item);
//                break;

//            case UIActionType.QuitGame:
//#if UNITY_EDITOR
//                UnityEditor.EditorApplication.isPlaying = false;
//#else
//                Application.Quit();
//#endif
//                break;

//            case UIActionType.TurnToPage: // điều hướng đến trang cụ thể (test mục lục số)
//                action?.Invoke(BookActionTypeEnum.TurnPage, item.targetPage);
//                break;
//        }
//    }

//    /// <summary>
//    /// truy cập trực tiếp vào trang sách EndlessBook để chuyển đến trang cụ thể. (test)
//    /// </summary>
//    /// <param name="item"></param>
//    private void togglePage(UIItem item)
//    {
//        book.TurnToPage(item.targetPage, PageTurnTimeTypeEnum.TotalTurnTime, item.turnTime);

//    }

//    //private void EnsureBookOpenMiddle()
//    //{
//    //    EndlessBook book = FindFirstObjectByType<EndlessBook>();
//    //    if (book != null && book.CurrentState != EndlessBook.StateEnum.OpenMiddle)
//    //    {
//    //        book.SetState(EndlessBook.StateEnum.OpenMiddle); // Trực tiếp gọi vào book
//    //    }
//    //}

//    #endregion

    private void Highlight(UIItem item)
    {
        if (currentHovered.HasValue && currentHovered.Value.targetRenderer == item.targetRenderer)
        {
            return;
        }

        ClearHighlight();

        if (item.targetRenderer != null && item.targetRenderer.material.HasProperty("_Color"))
        {
            item.targetRenderer.material.color = item.hoverColor;
        }
        else
        {
            var tmp = item.targetRenderer.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = item.hoverColor;
            }
            else
            {
                var fallback = item.targetRenderer.GetComponent<Renderer>();
                if (fallback != null && fallback.material.HasProperty("_Color"))
                {
                    fallback.material.color = item.hoverColor;
                }
            }
        }

        currentHovered = item;
    }

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
