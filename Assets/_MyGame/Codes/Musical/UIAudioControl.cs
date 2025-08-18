using FMODUnity;
using UnityEngine;

namespace _MyGame.Codes.Musical
{
    /// <summary>
    /// Quản lý giao diện điều khiển âm thanh (volume và mute) trong menu setting của game.
    /// Lưu trữ các thiết lập âm thanh vào PlayerPrefs để giữ trạng thái giữa các phiên chơi.
    /// </summary>
    public class UIAudioControl : MonoBehaviour
    {
        [SerializeField] private bool onlyInMenu;
        [SerializeField] private EventReference clickEvent;

        private bool IsInMenu()
        {
            // Thay bằng logic kiểm tra trạng thái menu của game
            // Ví dụ: menu mở khi game tạm dừng
            return Time.timeScale == 0;
        }    
        
    }
}