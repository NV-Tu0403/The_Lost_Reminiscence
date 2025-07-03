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
            fixButton.gameObject.SetActive(false);
            fixButton.gameObject.SetActive(true);
        }
    }
}