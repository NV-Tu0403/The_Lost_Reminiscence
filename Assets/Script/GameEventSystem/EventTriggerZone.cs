using UnityEngine;

/// <summary>
/// gọi TriggerEvent khi người chơi vào vùng collider.
/// </summary>
public class EventTriggerZone : MonoBehaviour
{
    public string eventId;
    public EventExecutor executor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            executor.TriggerEvent(eventId);
        }
    }
}
