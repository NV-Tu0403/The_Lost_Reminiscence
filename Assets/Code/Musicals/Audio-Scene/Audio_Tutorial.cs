using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_Tutorial : MonoBehaviour
{
    public EventReference tutorial_ambienceEvent;
    public EventReference tutorial_randomAmbienceEvent;


    void Start()
    {
        AudioManager.Instance.PlayOneShot(tutorial_ambienceEvent, this.transform.position);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance.PlayOneShot(tutorial_randomAmbienceEvent, this.transform.position);
        }
    }


}
