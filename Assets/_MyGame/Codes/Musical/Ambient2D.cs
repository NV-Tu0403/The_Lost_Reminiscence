using FMOD.Studio; // Thêm namespace cho EventInstance và STOP_MODE
using FMODUnity;   // Thêm namespace cho EventReference và RuntimeUtils
using UnityEngine;
using System.Collections;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace _MyGame.Codes.Musical
{
    public class Ambient2D : MonoBehaviour
    {
        [SerializeField] private EventReference ambient2D; // Sự kiện âm thanh nền của khu vực 2D
        [SerializeField] private float fadeInTime = 1.0f; // Thời gian fade in (giây)
        [SerializeField] private float fadeOutTime = 1.0f; // Thời gian fade out (giây)

        private EventInstance _ambientInstance; // Instance riêng cho ambient 2D

        private void Start()
        {
            // Kiểm tra EventReference
            if (ambient2D.IsNull)
            {
                Debug.LogWarning("Ambient2D Event chưa được gán trong Inspector!");
                return;
            }

            // Tạo và khởi tạo instance cho ambient 2D
            _ambientInstance = RuntimeManager.CreateInstance(ambient2D);
            _ambientInstance.start();

            // Fade in âm thanh
            _ambientInstance.setVolume(0f);
            StartCoroutine(FadeAudio(fadeInTime, 1.0f));
        }

        private void Update()
        {
            // Không cần cập nhật vị trí 3D vì là âm thanh 2D
            // Chỉ kiểm tra instance hợp lệ
            if (!_ambientInstance.isValid())
            {
                Debug.LogWarning("Ambient2D Instance không hợp lệ!");
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

        private void OnDestroy()
        {
            if (_ambientInstance.isValid())
            {
                StartCoroutine(FadeAudio(fadeOutTime, 0f, () =>
                {
                    _ambientInstance.stop(STOP_MODE.ALLOWFADEOUT);
                    _ambientInstance.release();
                }));
            }
        }
    }
}