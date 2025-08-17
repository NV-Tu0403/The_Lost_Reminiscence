using _MyGame.Codes.Procession;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.DevMode
{
    public class ButtonDevMode : MonoBehaviour
    {
        [SerializeField] private Button jumpMainButton;
        [SerializeField] private string mainId;
        
        private void Start()
        {
            if (jumpMainButton != null)
                jumpMainButton.onClick.AddListener(() =>
                    ProgressionManager.Instance.JumpToMainProcess(mainId));
        }
    }
}