using Code.Procession;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class DevModeUI : MonoBehaviour
    {
        [SerializeField] private Button devModeButton;

        private void Start()
        {
            devModeButton.onClick.AddListener(() => OnDevModeToggled(true));
        }

        private void OnDevModeToggled(bool isOn)
        {
            if (isOn) ProgressionManager.Instance.ForceCompleteAllPuzzleEvents();
            // Nếu muốn revert thì reload scene hoặc xử lý thêm
        }
    }
}