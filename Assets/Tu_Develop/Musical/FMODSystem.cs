using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODSystem : MonoBehaviour
{
    public static FMODSystem Instance { get; private set; }

    private Bus musicBus, sfxBus, ambienceBus, uiBus;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        uiBus = RuntimeManager.GetBus("bus:/UI");
    }

    public void SetMusicVolume(float volume) => musicBus.setVolume(volume);
    public void SetSFXVolume(float volume) => sfxBus.setVolume(volume);
    public void SetAmbienceVolume(float volume) => ambienceBus.setVolume(volume);
    public void SetUIVolume(float volume) => uiBus.setVolume(volume);

    public void PlayOneShot(string eventPath, Vector3 position)
    {
        RuntimeManager.PlayOneShot(eventPath, position);
    }
}
