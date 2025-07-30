using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace Tu_Develop.Musical
{
    /// <summary>
    /// Quản lý giao diện điều khiển âm thanh (volume và mute) trong menu setting của game.
    /// Lưu trữ các thiết lập âm thanh vào PlayerPrefs để giữ trạng thái giữa các phiên chơi.
    /// </summary>
    public class UIAudioControl : MonoBehaviour
    {
        [SerializeField] private bool onlyInMenu = false;
        [SerializeField] private EventReference clickEvent;
        private void Update()
        {
            // // Kiểm tra nhấn chuột trái
            // if (Input.GetMouseButtonDown(0))
            // {
            //     // Chỉ phát nếu không giới hạn ở menu hoặc đang ở menu
            //     if (!onlyInMenu)
            //     {
            //         AudioManager.Instance.PlaySfx2D(clickEvent);
            //     }
            // }
        }

        private bool IsInMenu()
        {
            // Thay bằng logic kiểm tra trạng thái menu của game
            // Ví dụ: menu mở khi game tạm dừng
            return Time.timeScale == 0;
        }    
        
    }
}