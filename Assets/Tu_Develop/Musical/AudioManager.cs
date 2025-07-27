using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Tu_Develop.Musical
{
    /// <summary>
    /// Lớp giao diện cấp cao để điều khiển hệ thống âm thanh FMOD.
    /// Các hệ thống game khác sử dụng AudioManager để phát âm thanh và điều chỉnh tham số.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance của AudioManager, đảm bảo chỉ có một instance duy nhất trong game.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Phát một hiệu ứng âm thanh 2D (như tiếng click UI, âm thanh menu).
        /// <para><b>Tooltip:</b> Sử dụng cho âm thanh không cần định vị không gian. Chọn FMOD event trong Inspector.</para>
        /// </summary>
        /// <param name="eventReference">FMOD EventReference từ Inspector (VD: event:/UI/Click).</param>
        public void PlaySfx2D(EventReference eventReference)
        {
            FMODSystem.Instance.PlayOneShot(eventReference);
        }

        /// <summary>
        /// Phát một hiệu ứng âm thanh 3D ngắn tại một vị trí cụ thể trong thế giới game.
        /// <para><b>Tooltip:</b> Sử dụng cho âm thanh định vị không gian như bước chân, va chạm. Chọn FMOD event trong Inspector.</para>
        /// </summary>
        /// <param name="eventReference">FMOD EventReference từ Inspector (VD: event:/SFX/Footstep).</param>
        /// <param name="position">Vị trí trong thế giới game để phát âm thanh (Vector3).</param>
        public void PlaySfx(EventReference eventReference, Vector3 position)
        {
            FMODSystem.Instance.PlayOneShot(eventReference, position);
        }

        /// <summary>
        /// Phát một âm thanh persistent (như BGM, ambience) và lưu lại để điều khiển sau.
        /// <para><b>Tooltip:</b> Sử dụng cho âm thanh liên tục như nhạc nền. Chọn FMOD event trong Inspector.</para>
        /// </summary>
        /// <param name="tag">Tên định danh duy nhất để quản lý âm thanh (VD: "level1_bgm").</param>
        /// <param name="eventReference">FMOD EventReference từ Inspector (VD: event:/BGM/Level1).</param>
        public void PlayPersistentSound(string tag, EventReference eventReference)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogWarning("Tag cannot be empty for persistent sound.");
                return;
            }
            if (FMODSystem.Instance.GetCachedInstance(tag).isValid())
            {
                Debug.LogWarning($"Persistent sound with tag {tag} already exists. Restarting.");
            }
            FMODSystem.Instance.PlayAndCache(tag, eventReference);
        }

        /// <summary>
        /// Dừng một âm thanh persistent đang phát dựa trên tag.
        /// <para><b>Tooltip:</b> Dùng để dừng âm thanh như BGM hoặc ambience. Tag phải khớp với tag khi gọi PlayPersistentSound.</para>
        /// </summary>
        /// <param name="tag">Tên định danh của âm thanh (VD: "level1_bgm").</param>
        public void StopPersistentSound(string tag)
        {
            FMODSystem.Instance.StopCached(tag);
        }

        /// <summary>
        /// Thay đổi giá trị của một parameter trên một âm thanh persistent đang phát.
        /// <para><b>Tooltip:</b> Dùng để điều chỉnh tham số FMOD như "Intensity". Ví dụ: tag="level1_bgm", paramName="Intensity", value=0.5f.</para>
        /// </summary>
        /// <param name="tag">Tên định danh của âm thanh (VD: "level1_bgm").</param>
        /// <param name="paramName">Tên của parameter trong FMOD (VD: "Intensity").</param>
        /// <param name="value">Giá trị muốn đặt cho parameter (thường từ 0 đến 1).</param>
        public void SetParameter(string tag, string paramName, float value)
        {
            EventInstance instance = FMODSystem.Instance.GetCachedInstance(tag);
            if (instance.isValid())
            {
                instance.setParameterByName(paramName, value);
            }
        }

        /// <summary>
        /// Phát một hiệu ứng âm thanh 3D với các parameter tùy chỉnh tại một vị trí cụ thể.
        /// <para><b>Tooltip:</b> Sử dụng cho âm thanh 3D cần tham số, như bước chân trên bề mặt khác nhau. Chọn FMOD event trong Inspector.</para>
        /// </summary>
        /// <param name="eventReference">FMOD EventReference từ Inspector (VD: event:/SFX/Footstep).</param>
        /// <param name="position">Vị trí trong thế giới game để phát âm thanh (Vector3).</param>
        /// <param name="parameters">Danh sách tham số FMOD (tên và giá trị). Có thể null nếu không cần tham số.</param>
        public void PlaySfxWithParameters(EventReference eventReference, Vector3 position, Dictionary<string, float> parameters = null)
        {
            if (!FMODSystem.Instance.IsEventPathValid(eventReference))
            {
                Debug.LogWarning($"Invalid FMOD event: {eventReference.Path}");
                return;
            }

            var instance = FMODSystem.Instance.PlayEventInstance(eventReference);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    FMODSystem.Instance.SetParameter(instance, param.Key, param.Value);
                }
            }
            instance.set3DAttributes(position.To3DAttributes());
            instance.start();
            instance.release();
        }
    }
}