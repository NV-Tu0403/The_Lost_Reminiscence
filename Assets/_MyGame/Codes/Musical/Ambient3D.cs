using FMOD.Studio; // Thêm namespace cho EventInstance và STOP_MODE
using FMODUnity;   // Thêm namespace cho EventReference và RuntimeUtils
using UnityEngine;
using System.Collections;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace _MyGame.Codes.Musical
{
    public class Ambient3D : MonoBehaviour
    {
        [SerializeField] private EventReference ambient3D; // Sự kiện âm thanh nền của khu vực 3D
        [SerializeField] private float fadeInTime = 1.0f; // Thời gian fade in (giây)
        [SerializeField] private float fadeOutTime = 1.0f; // Thời gian fade out (giây)

        private EventInstance _ambientInstance; // Instance riêng cho ambient 3D

        private void Start()
        {
            // Kiểm tra EventReference
            if (ambient3D.IsNull)
            {
                Debug.LogWarning("Ambient3D Event chưa được gán trong Inspector!");
                return;
            }

            // Tạo và khởi tạo instance cho ambient 3D
            _ambientInstance = RuntimeManager.CreateInstance(ambient3D);
            _ambientInstance.set3DAttributes(transform.position.To3DAttributes()); // Đặt vị trí 3D
            _ambientInstance.start();

            // Fade in âm thanh
            _ambientInstance.setVolume(0f);
            StartCoroutine(FadeAudio(fadeInTime, 1.0f));
        }

        private void Update()
        {
            // Cập nhật vị trí 3D theo GameObject (nếu di chuyển)
            if (_ambientInstance.isValid())
            {
                _ambientInstance.set3DAttributes(transform.position.To3DAttributes());
            }
        }

        private void OnDestroy()
        {
            // Fade out trước khi dừng
            if (_ambientInstance.isValid())
            {
                StartCoroutine(FadeAudio(fadeOutTime, 0f, () =>
                {
                    _ambientInstance.stop(STOP_MODE.ALLOWFADEOUT);
                    _ambientInstance.release();
                }));
            }
        }

        private IEnumerator FadeAudio(float duration, float targetVolume, System.Action onComplete = null)
        {
            float currentTime = 0f;
            _ambientInstance.getVolume(out float currentVolume);

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVolume = Mathf.Lerp(currentVolume, targetVolume, currentTime / duration);
                _ambientInstance.setVolume(newVolume);
                yield return null;
            }
            _ambientInstance.setVolume(targetVolume);

            onComplete?.Invoke();
        }
    }
}