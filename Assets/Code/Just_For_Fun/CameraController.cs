using UnityEngine;
using System.Collections; // Cần thiết để sử dụng Coroutine

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2, -4);
    public float mouseSensitivity = 100f;
    public float smoothSpeed = 0.125f;

    private float xRotation = 0f;
    private Vector3 originalPosition;

    void LateUpdate()
    {
        if (target == null) return;
        
        // Logic di chuyển và xoay camera như cũ
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -40f, 60f);

        Quaternion rotation = Quaternion.Euler(xRotation, target.eulerAngles.y, 0);
        Vector3 desiredPosition = target.position + rotation * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    // Hàm mới để rung camera
    public void ShakeCamera(float duration = 0.15f, float magnitude = 0.1f)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    // Coroutine để thực hiện việc rung
    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Chú ý: chúng ta tác động lên localPosition
            transform.localPosition += new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null; // Chờ đến frame tiếp theo
        }
    }
}