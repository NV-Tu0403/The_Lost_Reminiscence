using UnityEngine;
using UnityEngine.Serialization;

namespace _MyGame.Codes.Puzzle.Test
{

    public class CheatChangeScene : MonoBehaviour
    {
        [FormerlySerializedAs("TargetSceneName")] [SerializeField] private string targetSceneName;
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
                CoreEvent.Instance.triggerChangeScene(targetSceneName, Vector3.zero);
            }
        }
    }
}

