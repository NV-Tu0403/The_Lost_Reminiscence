using UnityEngine;
using UnityEngine.UI;

namespace Tu_Develop.Musical
{
    /// <summary>
    /// Quản lý giao diện điều khiển âm thanh (volume và mute) trong menu setting của game.
    /// Lưu trữ các thiết lập âm thanh vào PlayerPrefs để giữ trạng thái giữa các phiên chơi.
    /// </summary>
    /// 
    public class UIAudioManager : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider ambienceSlider;
        [SerializeField] private Slider uiSlider;
        [SerializeField] private Toggle musicMuteToggle;

        private void Start()
        {
            if (musicSlider == null || sfxSlider == null || ambienceSlider == null || uiSlider == null || musicMuteToggle == null)
            {
                Debug.LogError("One or more UI components are not assigned in UIAudioControl.");
                return;
            }

            // Initialize slider values from PlayerPrefs with default of 0.8f
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.8f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.8f);
            ambienceSlider.value = PlayerPrefs.GetFloat("AmbVol", 0.8f);
            uiSlider.value = PlayerPrefs.GetFloat("UIVol", 0.8f);

            // Apply initial volume settings
            SetMusicVolume(musicSlider.value);
            SetSfxVolume(sfxSlider.value);
            SetAmbienceVolume(ambienceSlider.value);
            SetUIVolume(uiSlider.value);

            // Add listeners for slider changes
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            ambienceSlider.onValueChanged.AddListener(SetAmbienceVolume);
            uiSlider.onValueChanged.AddListener(SetUIVolume);

            // Initialize mute toggle and add listener
            musicMuteToggle.isOn = PlayerPrefs.GetInt("MusicMute", 0) == 0;
            musicMuteToggle.onValueChanged.AddListener(MuteMusic);
        }

        /// <summary>
        /// Thiết lập âm lượng cho bus Music và lưu vào PlayerPrefs.
        /// <para><b>Tooltip:</b> Gắn vào slider để điều chỉnh volume nhạc nền (0-1).</para>
        /// </summary>
        /// <param name="value">Giá trị volume (0-1).</param>
        private void SetMusicVolume(float value)
        {
            value = Mathf.Clamp01(value);
            FMODSystem.Instance.SetBusVolume("Music", value);
            PlayerPrefs.SetFloat("MusicVol", value);
        }

        /// <summary>
        /// Thiết lập âm lượng cho bus SFX và lưu vào PlayerPrefs.
        /// <para><b>Tooltip:</b> Gắn vào slider để điều chỉnh volume hiệu ứng âm thanh (như bước chân, súng).</para>
        /// </summary>
        /// <param name="value">Giá trị volume (0-1).</param>
        private void SetSfxVolume(float value)
        {
            value = Mathf.Clamp01(value);
            FMODSystem.Instance.SetBusVolume("SFX", value);
            PlayerPrefs.SetFloat("SFXVol", value);
        }

        /// <summary>
        /// Thiết lập âm lượng cho bus Ambience và lưu vào PlayerPrefs.
        /// <para><b>Tooltip:</b> Gắn vào slider để điều chỉnh volume âm thanh môi trường (như tiếng mưa, gió).</para>
        /// </summary>
        /// <param name="value">Giá trị volume (0-1).</param>
        private void SetAmbienceVolume(float value)
        {
            value = Mathf.Clamp01(value);
            FMODSystem.Instance.SetBusVolume("Ambience", value);
            PlayerPrefs.SetFloat("AmbVol", value);
        }

        /// <summary>
        /// Thiết lập âm lượng cho bus UI và lưu vào PlayerPrefs.
        /// <para><b>Tooltip:</b> Gắn vào slider để điều chỉnh volume âm thanh giao diện (như tiếng click).</para>
        /// </summary>
        /// <param name="value">Giá trị volume (0-1).</param>
        private void SetUIVolume(float value)
        {
            value = Mathf.Clamp01(value);
            FMODSystem.Instance.SetBusVolume("UI", value);
            PlayerPrefs.SetFloat("UIVol", value);
        }

        /// <summary>
        /// Bật/tắt âm thanh Music và lưu trạng thái vào PlayerPrefs.
        /// <para><b>Tooltip:</b> Gắn vào toggle để mute/unmute nhạc nền. Khi mute, volume Music được đặt về 0.</para>
        /// </summary>
        /// <param name="isOn">Trạng thái toggle (true = bật âm, false = tắt âm).</param>
        private void MuteMusic(bool isOn)
        {
            float volume = isOn ? musicSlider.value : 0f;
            FMODSystem.Instance.SetBusVolume("Music", volume);
            PlayerPrefs.SetInt("MusicMute", isOn ? 0 : 1);
        }
    }
}
