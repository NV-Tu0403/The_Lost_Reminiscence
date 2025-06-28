using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.Dark
{
    /// <summary>
    /// PanelTabButton là một lớp quản lý các nút tab trong giao diện người dùng, cho phép chuyển đổi hiệu ứng hoạt hình khi người dùng di chuột qua nút.
    /// </summary>
    public class PanelTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Animator buttonAnimator;

        void Start()
        {
            buttonAnimator = gameObject.GetComponent<Animator>();
        }

        /// <summary>
        /// Xử lý sự kiện khi con trỏ chuột di chuyển vào nút, áp dụng hiệu ứng hoạt hình "Normal to Hover" nếu không ở trạng thái "Hover to Pressed".
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover to Pressed"))
                buttonAnimator.Play("Normal to Hover");
        }

        /// <summary>
        /// Xử lý sự kiện khi con trỏ chuột di chuyển ra khỏi nút, áp dụng hiệu ứng hoạt hình "Hover to Normal" nếu không ở trạng thái "Hover to Pressed".
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover to Pressed"))
                buttonAnimator.Play("Hover to Normal");
        }
    }
}
