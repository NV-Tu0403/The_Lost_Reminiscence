using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

namespace _MyGame.Codes.Musical
{
    public class BossFinalAudio : MonoBehaviour
    {
        [SerializeField] private EventReference mapBGMEvent;
        [SerializeField] private EventReference bossState1Event;
        [SerializeField] private EventReference bossState2Event;

        [SerializeField] private float fadeInTime = 1.0f;  // Thời gian fade in (giây)
        [SerializeField] private float fadeOutTime = 1.0f; // Thời gian fade out (giây)

        private EventInstance _bgmInstance;
        private EventInstance _bossStateInstance;

        private void Start()
        {
            // Kiểm tra và khởi tạo bgmInstance
            if (mapBGMEvent.IsNull)
            {
                Debug.LogWarning("mapBGMEvent chưa được gán trong Inspector!");
                return;
            }

            _bgmInstance = RuntimeManager.CreateInstance(mapBGMEvent);
            _bgmInstance.start();
            _bgmInstance.setVolume(0f); // Bắt đầu với volume 0
            StartCoroutine(FadeAudio(_bgmInstance, fadeInTime, 1.0f)); // Fade in bgm
        }

        public void PlayState(int stateIndex)
        {
            if (!bossState1Event.IsNull && !bossState2Event.IsNull)
            {
                StartCoroutine(SwitchState(stateIndex));
            }
            else
            {
                Debug.LogWarning("bossState1Event hoặc bossState2Event chưa được gán!");
            }
        }

        private IEnumerator SwitchState(int stateIndex)
        {
            // Fade out instance cũ
            if (_bossStateInstance.isValid())
            {
                yield return FadeAudio(_bossStateInstance, fadeOutTime, 0f, () =>
                {
                    _bossStateInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    _bossStateInstance.release();
                });
            }

            // Tạo và fade in instance mới
            var newEvent = stateIndex == 1 ? bossState1Event : bossState2Event;
            _bossStateInstance = RuntimeManager.CreateInstance(newEvent);
            _bossStateInstance.start();
            _bossStateInstance.setVolume(0f);
            yield return FadeAudio(_bossStateInstance, fadeInTime, 1.0f);
        }

        private static IEnumerator FadeAudio(EventInstance instance, float duration, float targetVolume, System.Action onComplete = null)
        {
            var currentTime = 0f;
            instance.getVolume(out float currentVolume);

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVolume = Mathf.Lerp(currentVolume, targetVolume, currentTime / duration);
                instance.setVolume(newVolume);
                yield return null;
            }
            instance.setVolume(targetVolume);

            onComplete?.Invoke();
        }

        private void OnDestroy()
        {
            // Dọn dẹp cả bgmInstance và bossStateInstance
            if (_bgmInstance.isValid())
            {
                StartCoroutine(FadeAudio(_bgmInstance, fadeOutTime, 0f, () =>
                {
                    _bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    _bgmInstance.release();
                }));
            }

            if (_bossStateInstance.isValid())
            {
                StartCoroutine(FadeAudio(_bossStateInstance, fadeOutTime, 0f, () =>
                {
                    _bossStateInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    _bossStateInstance.release();
                }));
            }
        }
    }
}