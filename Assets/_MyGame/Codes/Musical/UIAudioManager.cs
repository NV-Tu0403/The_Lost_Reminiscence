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
        
        public AudioMixer audioMixer; // AudioMixer để điều chỉnh âm lượng

        private void Start()
        {
            // Khởi tạo giá trị slider từ PlayerPrefs, mặc định 0.8f nếu chưa có
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.8f);

            // Áp dụng giá trị ban đầu cho tất cả các bus
            SetMasterVolume(masterSlider.value);

            // Thêm listener để cập nhật khi slider thay đổi
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        public void SetMasterVolume(Slider masterValue)
        {
            var value = Mathf.Clamp01(masterValue.value);
            SetMasterVolume(value);
        }
        
        
        /// <summary>
        /// Thiết lập âm lượng tổng cho tất cả các bus (Music, SFX, Ambience, UI) dựa trên giá trị slider.
        /// <para><b>Tooltip:</b> Gắn vào slider để điều chỉnh volume tổng (0-1) cho mọi loại âm thanh.</para>
        /// </summary>
        /// <param name="value">Giá trị volume từ slider (0-1).</param>
        private void SetMasterVolume(float value)
        {
            // Giới hạn giá trị trong khoảng 0-1
            value = Mathf.Clamp01(value);

            // Áp dụng giá trị cho tất cả các bus
            FMODSystem.Instance.SetBusVolume("Music", value);
            FMODSystem.Instance.SetBusVolume("SFX", value);
            FMODSystem.Instance.SetBusVolume("Ambience", value);
            FMODSystem.Instance.SetBusVolume("UI", value);

            // Lưu giá trị vào PlayerPrefs
            PlayerPrefs.SetFloat("MasterVol", value);
            PlayerPrefs.Save(); // Đảm bảo lưu ngay lập tức
            
            // Set giá trị cho AudioMixer nếu cần
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20); // Chuyển đổi sang dB
            }
        }
    }
}