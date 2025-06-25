using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

namespace Michsky.UI.Dark
{
    /// <summary>
    /// UIElementSound là một lớp quản lý âm thanh cho các phần tử giao diện người dùng, cho phép phát âm thanh khi người dùng tương tác với các phần tử như nút và hover.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class UIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("RESOURCES")]
        public AudioSource audioSource;
        public AudioClip hoverSound;
        public AudioClip clickSound;
        public AudioClip notificationSound;

        [Header("SETTINGS")]
        public bool enableHoverSound = true;
        public bool enableClickSound = true;

        void Start()
        {
            if (audioSource == null)
            {
                try
                {
                    audioSource = gameObject.GetComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }

                catch
                {
                    Debug.LogError("UI Element Sound - Cannot initalize AudioSource due to missing resources.", this);
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi con trỏ chuột di chuyển vào phần tử, phát âm thanh hover nếu được bật.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableHoverSound == true && audioSource != null)
                audioSource.PlayOneShot(hoverSound);
        }

        /// <summary>
        /// Xử lý sự kiện khi người dùng nhấp vào phần tử, phát âm thanh click nếu được bật.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (enableClickSound == true && audioSource != null)
                audioSource.PlayOneShot(clickSound);
        }

        /// <summary>
        /// Phát âm thanh thông báo, thường được sử dụng để thông báo các sự kiện quan trọng trong giao diện người dùng.
        /// </summary>
        public void Notification()
        {
            if (audioSource != null)
                audioSource.PlayOneShot(notificationSound);
        }
    }
}