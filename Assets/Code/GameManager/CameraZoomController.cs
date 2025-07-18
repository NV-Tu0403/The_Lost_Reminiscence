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

    public async Task PerformZoomSequence(int targetIndex)
    {
        if (listTargetObj == null || listTargetObj.Count <= targetIndex)
        {
            Debug.LogWarning("Invalid target index or list is empty.");
            return;
        }

        // đợi zoomDelay giây trước khi bắt đầu zoom
        await Task.Delay(TimeSpan.FromSeconds(zoomDelay));
        ZoomState = true;

        //// Trigger map load (hidden)
        CoreEvent.Instance.triggerContinueSession();

        // Move & zoom camera
        Transform target = listTargetObj[targetIndex];
        Vector3 zoomPos = target.position + offsetFromTarget;

        Sequence zoomIn = DOTween.Sequence();
        zoomIn.Append(targetCamera.transform.DOMove(zoomPos, zoomDuration).SetEase(Ease.InOutSine));
        zoomIn.Join(targetCamera.DOFieldOfView(targetFOV, zoomDuration).SetEase(Ease.InOutSine));

        await zoomIn.AsyncWaitForCompletion();

        // [Optional] Có thể thêm delay giữa zoomIn và zoomOut nếu cần
        await Task.Delay(300); // Ví dụ: giữ trạng thái zoom 0.3 giây

        // Trả camera về trạng thái ban đầu
        Sequence zoomOut = DOTween.Sequence();
        zoomOut.Append(targetCamera.transform.DOMove(originalCamPos, zoomDuration).SetEase(Ease.InOutSine));
        zoomOut.Join(targetCamera.DOFieldOfView(originalFOV, zoomDuration).SetEase(Ease.InOutSine));

        //await zoomOut.AsyncWaitForCompletion();

        ZoomState = false;

        Debug.Log("Zoom sequence complete. Gameplay can resume.");
    }
}
