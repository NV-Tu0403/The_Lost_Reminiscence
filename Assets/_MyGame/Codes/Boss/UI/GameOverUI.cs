using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Code.Boss
{
    /// <summary>
    /// UI Game Over với nút restart
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button restartButton;
        [SerializeField] private TextMeshProUGUI gameOverText;
        
        private void Start()
        {
            SetupButtons();
            SetupText();
        }
        
        private void SetupButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }
        
        private void SetupText()
        {
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER";
                gameOverText.color = Color.red;
            }
        }
        
        private void OnRestartClicked()
        {
            Debug.Log("[GameOverUI] Restart button clicked");
            
            if (BossGameManager.Instance != null)
            {
                BossGameManager.Instance.RestartGame();
            }
        }
        
    }
}
