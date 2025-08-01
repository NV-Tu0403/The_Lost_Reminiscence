using UnityEngine;

namespace Loc_Backend
{
    /// <summary>
    /// Change to scene BossFinal
    public class CheatChangeScene : MonoBehaviour
    {
        [SerializeField] private string sceneName;
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
                ChangeScene();
            }
        }

        private void ChangeScene()
        {
            // Thay đổi scene sang BossFinal
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
