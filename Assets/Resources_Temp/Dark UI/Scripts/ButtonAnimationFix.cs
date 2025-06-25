using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Dark
{
    /// <summary>
    /// ButtonAnimationFix là một lớp sửa lỗi cho các nút trong giao diện người dùng, giúp khắc phục vấn đề khi nút bị kẹt trong trạng thái hoạt hình "highlighted" sau khi được nhấn.
    /// </summary>
    public class ButtonAnimationFix : MonoBehaviour
    {
        private Button fixButton;

        void Start()
        {
            fixButton = gameObject.GetComponent<Button>();
            fixButton.onClick.AddListener(Fix);
        }

        public void Fix()
        {
            // We need to disable and enable the object, otherwise it'll stuck on highlighted anim.
            // This 'bug' is there since Unity 5, yet no fix available from Unity
            fixButton.gameObject.SetActive(false);
            fixButton.gameObject.SetActive(true);
        }
    }
}