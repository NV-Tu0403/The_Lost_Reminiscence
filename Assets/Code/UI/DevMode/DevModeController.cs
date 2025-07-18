using UnityEngine;

namespace Code.UI.DevMode
{
    public class DevModeController : MonoBehaviour
    {
        [SerializeField] private GameObject devModePanel;
        [SerializeField] private KeyCode enableDevModeUI;

        private void Start()
        {
            if (devModePanel != null) devModePanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(enableDevModeUI) && devModePanel != null)
            {
                devModePanel.SetActive(!devModePanel.activeSelf);
            }
        }
    }
}