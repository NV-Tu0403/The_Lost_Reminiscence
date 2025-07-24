using System.Collections;
using UnityEngine;

public class ProtectiveAura : MonoBehaviour
{
    private float duration = 3f;

    void OnEnable()
    {
        // Bắt đầu coroutine để đếm giờ và tự hủy
        StartCoroutine(SelfDestructRoutine());
    }

    private IEnumerator SelfDestructRoutine()
    {
        float timer = 0;

        // Lặp cho đến khi timer đạt đủ duration
        while (timer < duration)
        {
            // Cộng thêm thời gian của frame vừa qua
            timer += Time.deltaTime;

            // Tạm dừng coroutine và chờ đến frame tiếp theo
            yield return null;
        }

        // Sau khi vòng lặp kết thúc, tự hủy GameObject
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}