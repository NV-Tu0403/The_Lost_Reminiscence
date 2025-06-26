using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Singleton duy nhất quản lý âm thanh trong game.
    /// </summary>
    [Tooltip("Singleton duy nhất quản lý âm thanh trong game.")]
    public static AudioManager Instance;

    private EventInstance ambienceInstance;
    private string currentAmbiencePath = "";

    void Awake()
    {
        // Đảm bảo Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Phát âm thanh OneShot (không cần quản lý thêm)
    public void PlayOneShot(string eventPath, Vector3 position)
    {
        RuntimeManager.PlayOneShot(eventPath, position);
    }

    // Phát hoặc chuyển ambience
    public void PlayAmbience(string eventPath)
    {
        if (currentAmbiencePath == eventPath) return;
        StopAmbience(); // Dừng ambience cũ nếu có
        ambienceInstance = RuntimeManager.CreateInstance(eventPath);
        ambienceInstance.start();
        ambienceInstance.release(); // Cho phép tự cleanup sau khi stop
        currentAmbiencePath = eventPath;
    }

    // Dừng ambience hiện tại
    public void StopAmbience()
    {
        if (ambienceInstance.isValid())
        {
            ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambienceInstance.release();
        }
        currentAmbiencePath = "";
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
