using DuckLe;
using UnityEngine;

public class CharacterCamera : Core
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float followSpeed = 10.0f;
    [SerializeField] private float defaultDistance = 3.0f;
    [SerializeField] public float height = 2f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] public float maxDistance = 3.0f;
    [SerializeField] public bool useInterpolation = false;
    [SerializeField] private bool useDistanceAndHeightInterpolation = true; // Thêm tùy chọn mới

    private float currentDistance;
    private float yaw;
    private float pitch;
    public Camera mainCamera;

    [Header("Camera Settings")]
    private float targetMaxDistance;
    private float targetHeight;
    private float smoothedDistance;
    public bool isAiming = false;
    [SerializeField] private float rightOffset = 0.7f;
    [SerializeField] private float transitionSpeed = 10f;
    [SerializeField] public float distanceSmoothingSpeed = 10f;
    [SerializeField] public float sphereCastRadius = 0.2f;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Camera component không tìm thấy trên PersonCamera_M3!");
        }

        if (Core.Instance.IsOffline)
        {
            InitializeOffline();
        }
        else if (!Core.Instance.IsOffline)
        {
            InitializeOnline();
        }
        else
        {
            Debug.LogWarning("Core chưa được khởi tạo hoặc không phải chế độ offline/online!");
        }
    }

    private void Update()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -40f, 80f);

        if (useDistanceAndHeightInterpolation)
        {
            maxDistance = Mathf.Lerp(maxDistance, targetMaxDistance, Time.deltaTime * transitionSpeed);
            height = Mathf.Lerp(height, targetHeight, Time.deltaTime * transitionSpeed);
        }
    }

    private void LateUpdate()
    {
        UpdateLocationAngle();
    }

    private void InitializeOffline()
    {
        currentDistance = defaultDistance;
        targetMaxDistance = maxDistance;
        targetHeight = height;
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player") ?? FindFirstObjectByType<PlayerController>()?.gameObject;
            if (player != null)
            {
                target = player.transform;
                transform.position = target.position - transform.forward * currentDistance + Vector3.up * height;
            }
            else
            {
                Debug.LogWarning("No Player found for offline mode! Camera will not function.");
            }
        }
    }

    /// <summary>
    /// Khởi tạo camera trong chế độ online.
    /// </summary>
    private void InitializeOnline()
    {
        //If (Object != null && Object.Runner != null) // Online mode
        //{
        //    if (!Object.HasInputAuthority)
        //    {
        //        DisableCameraAndListener();
        //    }
        //    currentDistance = defaultDistance;

        //    targetMaxDistance = maxDistance; // Khởi tạo mục tiêu ban đầu
        //    targetHeight = height;           // Khởi tạo mục tiêu ban đầu
        //    rightOffset = 0f;

        //    if (target != null && Object.HasInputAuthority)
        //    {
        //        transform.position = target.position - transform.forward * currentDistance + Vector3.up * height;
        //    }
        //}
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position - transform.forward * currentDistance + Vector3.up * height;
        }
        else
        {
            Debug.LogWarning("Target được gán là null!");
        }
    }

    public void SetTargetValues(float newMaxDistance, float newHeight, float newRightOffset, bool aiming = false)
    {
        targetMaxDistance = newMaxDistance;
        targetHeight = newHeight;
        rightOffset = newRightOffset;
        isAiming = aiming;
    }

    private void DisableCameraAndListener()
    {
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }
        AudioListener listener = GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }
    }

    private void UpdateLocationAngle()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.back;

        Vector3 desiredPosition = target.position + Vector3.up * height + direction * currentDistance;

        if (/*isAiming && */rightOffset != 0f)
        {
            Vector3 rightDirection = target.right;
            desiredPosition += rightDirection * rightOffset;
        }

        float targetDistance = maxDistance;
        if (Physics.SphereCast(target.position + Vector3.up * height, sphereCastRadius, direction, out RaycastHit hit, maxDistance, collisionLayers))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        smoothedDistance = Mathf.Lerp(smoothedDistance, targetDistance, distanceSmoothingSpeed * Time.deltaTime);
        currentDistance = smoothedDistance;

        //desiredPosition = target.position + Vector3.up * height + direction * smoothedDistance;

        if (useInterpolation)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
            transform.rotation = rotation;
        }
    }
}
