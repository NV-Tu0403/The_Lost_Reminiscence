using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;

public class CameraZoomController : MonoBehaviour
{
    public static CameraZoomController Instance { get; private set; }

    [Header("Zoom Targets")]
    [SerializeField] private List<Transform> listTargetObj;
    //[SerializeField] private int targetIndex = 1;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomDelay = 1f;
    [SerializeField] private float zoomDuration = 1.5f;
    [SerializeField] private float targetFOV = 30f;
    [SerializeField] private Vector3 offsetFromTarget = new Vector3(0, 2, -5);

    [Header("Debug / State")]
    public bool ZoomState;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    private float originalFOV;
    private Vector3 originalCamPos;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Lưu giá trị mặc định lúc khởi động
        originalFOV = targetCamera.fieldOfView;
        originalCamPos = targetCamera.transform.position;
    }

    /// <summary>
    /// zoom vào
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <param name="logicBeforeZoom"></param>
    /// <returns></returns>
    public async Task PerformZoomSequence(int targetIndex, Func<Task> logicBeforeZoom = null, bool ZoomIn = true)
    {
        if (listTargetObj == null || listTargetObj.Count <= targetIndex)
        {
            Debug.LogWarning("Invalid target index or list is empty.");
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(zoomDelay));
        ZoomState = true;

        // Logic async bên ngoài truyền vào
        if (logicBeforeZoom != null)
            await logicBeforeZoom.Invoke();

        Transform target = listTargetObj[targetIndex];
        Vector3 zoomPos = target.position + offsetFromTarget;

        if (ZoomIn)
        {
            Sequence zoom = DOTween.Sequence();
            zoom.Append(targetCamera.transform.DOMove(zoomPos, zoomDuration).SetEase(Ease.InOutSine));
            zoom.Join(targetCamera.DOFieldOfView(targetFOV, zoomDuration + 2.8f).SetEase(Ease.InOutSine));

            await zoom.AsyncWaitForCompletion(); await Task.Delay(300);

            targetCamera.transform.position = originalCamPos;
            targetCamera.fieldOfView = originalFOV;
        }
        else
        {
            targetCamera.transform.position = zoomPos;
            targetCamera.fieldOfView = targetFOV;

            await Task.Delay(300);

            Sequence zoom = DOTween.Sequence();
            zoom.Append(targetCamera.transform.DOMove(originalCamPos, zoomDuration).SetEase(Ease.InOutSine));
            zoom.Join(targetCamera.DOFieldOfView(originalFOV, zoomDuration / 2.8f).SetEase(Ease.InOutSine));

            //await zoom.AsyncWaitForCompletion();
            //await Task.Delay(30);
        }
        ZoomState = false;

    }

    public Task PerformZoomSequence(int targetIndex, Action logicBeforeZoom, bool ZoomIn)
    {
        // Chuyển Action → Func<Task> bằng cách đóng gói lại
        Func<Task> wrapped = () =>
        {
            logicBeforeZoom?.Invoke();
            return Task.CompletedTask;
        };

        return PerformZoomSequence(targetIndex, wrapped, ZoomIn);
    }

    /// <summary>
    /// Zoom vào một vị trí mục tiêu với FOV cụ thể.
    /// WaitForCompletion = true: Chờ hoàn thành trước khi tiếp tục
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="targetFOV"></param>
    /// <param name="WaitForCompletion"></param>
    /// <returns></returns>
    public async Task ZoomToTargetAsync(Vector3 targetPos, float targetFOV, bool WaitForCompletion = true)
    {
        Sequence zoom = DOTween.Sequence();
        zoom.Append(targetCamera.transform.DOMove(targetPos, zoomDuration).SetEase(Ease.InOutSine));
        zoom.Join(targetCamera.DOFieldOfView(targetFOV, zoomDuration * 2).SetEase(Ease.InOutSine));

        await zoom.AsyncWaitForCompletion();
    }
}
