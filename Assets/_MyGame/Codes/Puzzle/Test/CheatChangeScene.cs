using UnityEngine;

/// <summary>
/// Change to scene BossFinal
public class CheatChangeScene : MonoBehaviour
{
    [SerializeField] private string TargetSceneName;
    [SerializeField] private Portal_Controller portalController;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F12)) return;
        if (portalController != null)
        {
            portalController.TogglePortal(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoreEvent.Instance.triggerChangeScene(TargetSceneName, Vector3.zero);
        }
    }
}

