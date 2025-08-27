using FMODUnity;
using UnityEngine;

namespace _MyGame.Codes.Musical
{
    /// <summary>
    /// Quản lý giao diện điều khiển âm thanh (volume và mute) trong menu setting của game.
    /// Lưu trữ các thiết lập âm thanh vào PlayerPrefs để giữ trạng thái giữa các phiên chơi.
    /// Phát âm thanh click chỉ khi BookMenu được kích hoạt và nhấn chuột trái.
    /// </summary>
    public class UIAudioControl : MonoBehaviour
    {
        [SerializeField] private bool onlyInMenu = true; // Chỉ phát âm thanh khi trong menu
        [SerializeField] private EventReference clickEvent; // Sự kiện âm thanh click

        [SerializeField] private GameObject bookMenu; // Tham chiếu đến BookMenu (gán trong Inspector)

        private void Awake()
        {
            // Kiểm tra nếu bookMenu không được gán trong Inspector
            if (bookMenu == null)
            {
                Debug.LogWarning("BookMenu chưa được gán trong Inspector!");
                bookMenu = GameObject.Find("BookMenu"); // Fallback nếu chưa gán
                onlyInMenu = true;
            }
        }

        private bool IsInMenu()
        {
            return bookMenu != null && bookMenu.activeInHierarchy && onlyInMenu;
        }

        private void LateUpdate()
        {
            if (IsInMenu() && Input.GetMouseButtonDown(0))
            {
                if (!clickEvent.IsNull)
                {
                    RuntimeManager.PlayOneShot(clickEvent);
                }
                else
                {
                    Debug.LogWarning("clickEvent chưa được gán trong Inspector!");
                }
            }
        }
    }
}