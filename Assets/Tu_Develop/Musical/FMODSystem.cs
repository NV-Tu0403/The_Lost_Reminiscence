using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using System.Collections.Generic;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Tu_Develop.Musical
{
    /// <summary>
    /// Hệ thống quản lý âm thanh trung tâm sử dụng FMOD cho toàn bộ game.
    /// Quản lý volume, phát nhạc, snapshot và các instance động.
    /// </summary>
    public class FMODSystem : MonoBehaviour
    {
        public static FMODSystem Instance { get; private set; }

        private readonly Dictionary<string, Bus> _buses = new();
        private readonly Dictionary<string, EventInstance> _eventPool = new();
        private readonly Dictionary<string, EventInstance> _snapshots = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeBus("Music", "bus:/Music");
            InitializeBus("SFX", "bus:/SFX");
            InitializeBus("Ambience", "bus:/Ambience");
            InitializeBus("UI", "bus:/UI");
        }

        #region Volume Control

        private void InitializeBus(string busName, string busPath)
        {
            _buses[busName] = RuntimeManager.GetBus(busPath);
        }

        public void SetBusVolume(string busName, float volume)
        {
            if (_buses.TryGetValue(busName, out var bus))
            {
                bus.setVolume(volume);
            }
            else
            {
                Debug.LogWarning($"Bus {busName} not found.");
            }
        }

        #endregion

        #region Basic Sound Playback

        // SỬA LỖI: Không cần hàm private PlayOneShot(string) nữa

        /// <summary>
        /// Phát âm thanh OneShot tại vị trí 3D sử dụng EventReference.
        /// </summary>
        public void PlayOneShot(EventReference eventReference, Vector3 position)
        {
            // SỬA LỖI: Gọi thẳng RuntimeManager với EventReference
            if (IsEventPathValid(eventReference))
            {
                RuntimeManager.PlayOneShot(eventReference, position);
            }
            else
            {
                Debug.LogWarning($"Invalid FMOD event reference provided.");
            }
        }
        
        /// <summary>
        /// Phát âm thanh OneShot 2D sử dụng EventReference (không cần vị trí).
        /// </summary>
        public void PlayOneShot(EventReference eventReference)
        {
            // SỬA LỖI: Gọi thẳng RuntimeManager với EventReference
            if (IsEventPathValid(eventReference))
            {
                RuntimeManager.PlayOneShot(eventReference);
            }
            else
            {
                Debug.LogWarning($"Invalid FMOD event reference provided.");
            }
        }
        
        /// <summary>
        /// Tạo và phát một EventInstance sử dụng EventReference.
        /// </summary>
        public EventInstance PlayEventInstance(EventReference eventReference)
        {
            // SỬA LỖI: Gọi thẳng RuntimeManager với EventReference
            if (!IsEventPathValid(eventReference))
            {
                Debug.LogWarning($"Invalid FMOD event reference provided.");
                return default;
            }
            var instance = RuntimeManager.CreateInstance(eventReference);
            instance.start();
            return instance;
        }

        /// <summary>
        /// Dừng một EventInstance với lựa chọn fade hoặc dừng gấp.
        /// </summary>
        public void StopEventInstance(EventInstance instance, bool fadeOut = true)
        {
            if (!instance.isValid()) return;
            instance.stop(fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            instance.release();
        }

        /// <summary>
        /// Thiết lập giá trị tham số cho một instance đang phát.
        /// </summary>
        public void SetParameter(EventInstance instance, string parameterName, float value)
        {
            if (!instance.isValid()) return;
            instance.setParameterByName(parameterName, value);
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của EventReference.
        /// </summary>
        public bool IsEventPathValid(EventReference eventReference)
        {
            // TỐI ƯU: Dùng thuộc tính IsNull của EventReference, hiệu quả hơn nhiều
            return !eventReference.IsNull;
        }

        #endregion

        #region Snapshot Management

        /// <summary>
        /// Bắt đầu phát một snapshot sử dụng EventReference.
        /// </summary>
        public void StartSnapshot(string tag, EventReference snapshotReference)
        {
            if (_snapshots.TryGetValue(tag, out var existing) && existing.isValid())
            {
                existing.stop(STOP_MODE.ALLOWFADEOUT);
                existing.release();
            }
            
            // SỬA LỖI: Gọi thẳng RuntimeManager với EventReference
            var snapshot = RuntimeManager.CreateInstance(snapshotReference);
            snapshot.start();
            _snapshots[tag] = snapshot;
        }

        /// <summary>
        /// Kết thúc snapshot hiện tại.
        /// </summary>
        public void StopSnapshot(string tag)
        {
            if (_snapshots.TryGetValue(tag, out var snapshot) && snapshot.isValid())
            {
                snapshot.stop(STOP_MODE.ALLOWFADEOUT);
                snapshot.release();
                _snapshots.Remove(tag);
            }
        }

        #endregion

        #region Event Pool Management

        /// <summary>
        /// Phát một event và lưu lại trong pool với EventReference.
        /// </summary>
        public void PlayAndCache(string tag, EventReference eventReference)
        {
            if (_eventPool.TryGetValue(tag, out var existing) && existing.isValid())
            {
                existing.stop(STOP_MODE.IMMEDIATE);
                existing.release();
            }

            // SỬA LỖI: Gọi thẳng RuntimeManager với EventReference
            var instance = RuntimeManager.CreateInstance(eventReference);
            instance.start();
            _eventPool[tag] = instance;
        }

        /// <summary>
        /// Dừng và xóa event trong pool theo key.
        /// </summary>
        public void StopCached(string tag)
        {
            if (_eventPool.TryGetValue(tag, out var instance) && instance.isValid())
            {
                instance.stop(STOP_MODE.ALLOWFADEOUT);
                instance.release();
                _eventPool.Remove(tag);
            }
        }

        /// <summary>
        /// Lấy một instance đã được cache từ pool.
        /// </summary>
        public EventInstance GetCachedInstance(string tag)
        {
            _eventPool.TryGetValue(tag, out var instance);
            return instance;
        }

        #endregion

        #region Debugging

        /// <summary>
        /// In log trạng thái valid và playing của 1 EventInstance.
        /// </summary>
        public void DebugEvent(EventInstance instance, string label = "[FMOD Debug]")
        {
            if (!instance.isValid())
            {
                Debug.LogWarning($"{label}: Instance không hợp lệ.");
                return;
            }

            instance.getPlaybackState(out var state);
            Debug.Log($"{label}: PlaybackState = {state}");
        }

        #endregion
    }
}