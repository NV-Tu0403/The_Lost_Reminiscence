using UnityEngine;

namespace Michsky.UI.Dark
{
    /// <summary>
    /// ModalWindowManager quản lý các cửa sổ modal trong giao diện người dùng, cho phép hiển thị và ẩn cửa sổ với hiệu ứng hoạt hình.
    /// </summary>
    public class ModalWindowManager : MonoBehaviour
    {
        [Header("BRUSH ANIMATION")]
        public Animator brushAnimator;
        public bool enableSplash = true;

        private Animator mWindowAnimator;

        void Start()
        {
            mWindowAnimator = gameObject.GetComponent<Animator>();
        }

        /// <summary>
        /// Hiển thị cửa sổ modal với hiệu ứng hoạt hình vào, đồng thời có thể áp dụng hiệu ứng làm mờ nền nếu được bật.
        /// </summary>
        public void ModalWindowIn()
        {
            mWindowAnimator.Play("Modal Window In");

            if(enableSplash == true)
            {
                brushAnimator.Play("Transition Out");
            }
        }

        /// <summary>
        /// Ẩn cửa sổ modal với hiệu ứng hoạt hình ra, đồng thời có thể áp dụng hiệu ứng làm mờ nền nếu được bật.
        /// </summary>
        public void ModalWindowOut()
        {
            mWindowAnimator.Play("Modal Window Out");

            if (enableSplash == true)
            {
                brushAnimator.Play("Transition In");
            }
        }
    }
}