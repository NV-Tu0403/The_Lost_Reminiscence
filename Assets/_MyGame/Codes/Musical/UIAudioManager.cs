using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace _MyGame.Codes.Musical
{
    /// <summary>
    /// Quản lý giao diện điều khiển âm thanh (volume) trong menu setting của game.
    /// Sử dụng một slider master để điều chỉnh âm lượng tổng cho tất cả các bus.
    /// Lưu trữ giá trị vào PlayerPrefs để giữ trạng thái giữa các phiên chơi.
    /// </summary>
    public class UIAudioManager : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider; // Slider duy nhất cho âm lượng tổng
        [SerializeField] private AudioMixer audioMixer; // AudioMixer để điều chỉnh âm lượng

        private void OnEnable()
        {
            if (masterSlider == null)
            {
                Debug.LogWarning("masterSlider chưa được gán trong Inspector!");
                enabled = false;
                return;
            }

            // Tải giá trị từ PlayerPrefs, mặc định 0.8f
            float savedValue = PlayerPrefs.GetFloat("MasterVol", 0.8f);
            masterSlider.value = savedValue;

            // Áp dụng giá trị ban đầu
            SetMasterVolume(savedValue);

            // Thêm listener
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        private void OnDisable()
        {
            if (masterSlider != null)
            {
                masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            }
        }

        /// <summary>
        /// Thiết lập âm lượng tổng cho tất cả các bus (Music, SFX, Ambience, UI) dựa trên giá trị slider.
        /// <para><b>Tooltip:</b> Gắn vào slider để điều chỉnh volume tổng (0-1) cho mọi loại âm thanh.</para>
        /// </summary>
        /// <param name="value">Giá trị volume từ slider (0-1).</param>
        private void SetMasterVolume(float value)
        {
            value = Mathf.Clamp01(value);

            // Áp dụng giá trị cho tất cả các bus nếu FMODSystem.Instance tồn tại
            if (FMODSystem.Instance != null)
            {
                FMODSystem.Instance.SetBusVolume("Music", value);
                FMODSystem.Instance.SetBusVolume("SFX", value);
                FMODSystem.Instance.SetBusVolume("Ambience", value);
                FMODSystem.Instance.SetBusVolume("UI", value);
            }
            else
            {
                Debug.LogWarning("FMODSystem.Instance là null. Kiểm tra xem FMODSystem đã được khởi tạo chưa!");
            }

            // Set giá trị cho AudioMixer nếu cần
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20); // Chuyển đổi sang dB
            }
            else
            {
                Debug.LogWarning("audioMixer chưa được gán trong Inspector!");
            }

            // Lưu giá trị vào PlayerPrefs
            PlayerPrefs.SetFloat("MasterVol", value);
            PlayerPrefs.Save();
        }
    }
}