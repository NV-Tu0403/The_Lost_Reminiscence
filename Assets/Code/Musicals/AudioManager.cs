using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Singleton duy nhất quản lý âm thanh trong game.
    /// </summary>
    [Tooltip("Singleton duy nhất quản lý âm thanh trong game.")]
    public static AudioManager Instance { get; private set; }

    private EventInstance ambienceInstance;
    private EventReference currentAmbiencePath;

    void Awake()
    {
        // Đảm bảo Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null); // Đặt lại cha để không bị ảnh hưởng bởi các scene khác
        DontDestroyOnLoad(gameObject);
    }

    // Phát âm thanh OneShot (không cần quản lý thêm)
    public void PlayOneShot(EventReference eventPath, Vector3 position)
    {
        RuntimeManager.PlayOneShot(eventPath, position);
    }

    // Phát hoặc chuyển ambience
    public void PlayAmbience(EventReference eventPath)
    {
        if (currentAmbiencePath.Path == eventPath.Path) return;   
        StopAmbience(); // Dừng ambience cũ nếu có
        ambienceInstance = RuntimeManager.CreateInstance(eventPath);
        var result = ambienceInstance.start();
        if (result != FMOD.RESULT.OK)
            Debug.LogError($"FMOD start error: {result}");
        currentAmbiencePath = eventPath;
    }

    // Dừng ambience hiện tại
    public void StopAmbience()
    {
        if (ambienceInstance.isValid())
        {
            var result = ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if (result != FMOD.RESULT.OK)
                Debug.LogError($"FMOD stop error: {result}");
            ambienceInstance.release();
        }
        currentAmbiencePath = default;
    }

    // Đặt tham số cho ambience (ví dụ: mưa nhỏ - mưa to)
    public void SetAmbienceParameter(string paramName, float value)
    {
        if (ambienceInstance.isValid())
        {
            ambienceInstance.setParameterByName(paramName, value);
        }
    }

    // Dừng toàn bộ âm thanh (reset)
    public void StopAll()
    {
        RuntimeManager.GetBus("bus:/").stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}