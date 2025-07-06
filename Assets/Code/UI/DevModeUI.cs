using Code.Procession;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class DevModeUI : MonoBehaviour
    {
        [SerializeField] private Button devModeButton;
        [SerializeField] private string mainId;

        private void Start()
        {
            devModeButton.onClick.AddListener(() => 
                ProgressionManager.Instance.ForceCompleteAllEventsInMain(mainId));
        }
    }
}