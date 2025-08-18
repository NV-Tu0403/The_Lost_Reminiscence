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

[Serializable]
public struct ObiOfPage
{
   public GameObject[] Obj;
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

    [Header ("Item UI")]
    [SerializeField] private UIItem[] menuItems;

    [Header("Obi of Page")]
    [SerializeField] private ObiOfPage[] obiOfPages; // Mảng các Obi của trang

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
            //DebugRayCast();
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
        ActiveObjOfPage(true);
        gameObject.SetActive(true);
        currentHovered = null;
        lastCheckTime = 0f;
        if (debugMode)
        {
            //Debug.Log($"[UIPageView] Activated on {gameObject.name}");
        }
    }

    public void Deactivate()
    {
        ClearHighlight();
        ActiveObjOfPage(false);
        gameObject.SetActive(false);
        if (debugMode)
        {
            //Debug.Log($"[UIPageView] Deactivated on {gameObject.name}");
        }
    }

    private void ActiveObjOfPage(bool oke)
    {
        foreach (var obiPage in obiOfPages)
        {
            foreach (var obj in obiPage.Obj)
            {
                if (obj != null)
                {
                    obj.SetActive(oke);
                }
            }
        }
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
            if (debugMode)
            {
                Debug.Log($"[UIPageView] HOVER - {item.uIActionType} - {item.targetPage}");
            }
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
            if (debugMode)
            {
                Debug.Log($"[UIPageView] TOUCH DOWN - {item.uIActionType} - {item.targetRenderer.gameObject.name}");
            }
            onTouchDownDetected?.Invoke(screenPoint, item);
        }
    }

    private void DetectTouchUp(Vector2 screenPoint)
    {
        if (RaycastAt(screenPoint, out var hit, out var item))
        {
            if (debugMode)
            {
                Debug.Log($"[UIPageView] TOUCH UP - {item.uIActionType} - {item.targetRenderer.gameObject.name}");
            }
            onTouchUpDetected?.Invoke(screenPoint, item, false);
        }
    }

    /// <summary>
    /// nhận vào UIItem và trả về GameObject của renderer mục tiêu.
    /// </summary>
    public static GameObject GetRendererGameObject(UIItem item)
    {
        return item.targetRenderer != null ? item.targetRenderer.gameObject : null;
    }

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
