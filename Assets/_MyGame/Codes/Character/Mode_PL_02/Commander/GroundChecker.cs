using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GroundChecker : MonoBehaviour
{
    private Collider _collider;

    // Khoảng cách mong muốn từ tâm collider tới mặt đất
    [SerializeField] private float maxCheck = 1.5f;
    [SerializeField] private float desiredDistance = 1.5f;

    // layer của ground
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        // Gốc ray là tâm collider
        Vector3 origin = _collider.bounds.center;
        Vector3 direction = Vector3.down;

        // Bắn tia xuống dưới (5f để chắc chắn bắt được ground)
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, maxCheck, groundLayer))
        {
            // Vẽ tia debug
            Debug.DrawLine(origin, hitInfo.point, Color.red);

            // Tính khoảng cách hiện tại
            float currentDistance = Vector3.Distance(origin, hitInfo.point);

            // Sai số so với desiredDistance
            float offset = currentDistance - desiredDistance;

            // Nếu khác thì dịch Player lên/xuống
            if (Mathf.Abs(offset) > 0.001f)
            {
                transform.position += Vector3.up * offset;
            }
        }
    }

    // vẽ gizmo để dễ debug
    private void OnDrawGizmos()
    {
        if (_collider == null) _collider = GetComponent<Collider>();
        Vector3 origin = _collider.bounds.center;
        Vector3 direction = Vector3.down;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + direction * desiredDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin + direction * desiredDistance, origin + direction * maxCheck);
    }
}
