using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/// <summary>
/// Quản lý việc phát các âm thanh chính trong game.
/// Sử dụng Singleton pattern để dễ dàng truy cập từ bất kỳ đâu.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private EventInstance _musicInstance;
    private EventInstance _ambienceInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Phát một âm thanh SFX (3D) tại một vị trí cụ thể.
    /// </summary>
    public void PlayOneShot(EventReference sfxEvent, Vector3 position)
    {
        // KIỂM TRA AN TOÀN: Đảm bảo EventReference đã được gán trong Inspector.
        if (sfxEvent.IsNull) return;
        
        // CÁCH SỬA LỖI:
        // TRƯỚC ĐÂY (gây lỗi): var path = sfxEvent.Path; RuntimeManager.PlayOneShot(path, position);
        // BÂY GIỜ (đã sửa): Truyền thẳng sfxEvent vào hàm của FMOD.
        RuntimeManager.PlayOneShot(sfxEvent, position);
    }

    /// <summary>
    /// Phát một âm thanh UI hoặc âm thanh 2D không cần vị trí.
    /// </summary>
    public void PlayOneShot(EventReference uiEvent)
    {
        if (uiEvent.IsNull) return;
        
        // Tương tự, truyền thẳng uiEvent vào hàm của FMOD.
        RuntimeManager.PlayOneShot(uiEvent);
    }

    /// <summary>
    /// Bắt đầu chơi một bản nhạc nền.
    /// </summary>
    public void PlayMusic(EventReference musicEvent)
    {
        if (musicEvent.IsNull) return;
        
        // Dừng bản nhạc cũ trước khi chơi bản mới (nếu có)
        if(_musicInstance.isValid())
        {
            _musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _musicInstance.release();
        }

        // Tạo instance mới và bắt đầu chơi
        _musicInstance = RuntimeManager.CreateInstance(musicEvent);
        _musicInstance.start();
    }

    /// <summary>
    /// Dừng chơi nhạc nền.
    /// </summary>
    public void StopMusic()
    {
        if(_musicInstance.isValid())
        {
            _musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _musicInstance.release();
        }
    }
}