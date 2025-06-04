using UnityEngine;
using FMODUnity;
using FMOD;
using FMOD.Studio;

public class AudioController : MonoBehaviour
{
    VCA vcaMusic;
    VCA vcaSFX;
    VCA vcaAmbience;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vcaMusic = RuntimeManager.GetVCA("vca:/VCA_Music");
        vcaSFX = RuntimeManager.GetVCA("vca:/VCA_SFX");
        vcaAmbience = RuntimeManager.GetVCA("vca:/VCA_Ambience");
    }

    public void SetMusicVolume(float volume)
    {
        vcaMusic.setVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        vcaSFX.setVolume(volume);
    }
    public void SetAmbienceVolume(float volume)
    {
        vcaAmbience.setVolume(volume);
    }
}
