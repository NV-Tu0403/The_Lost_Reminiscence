using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [field: Header("Audio Events")]
    [field: SerializeField] public EventReference walk_footstepsSound { get; private set; }
    [field: SerializeField] public EventReference run_footstepsSound { get; private set; }
    private void Awake()
    {
        if (instance != null)
            Debug.LogError("Found more than one Audio Manager in the scene");
        instance = this;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }


    public EventInstance CreateInstance (EventReference eventReference)
    {
        if (eventReference.IsNull)
        {
            Debug.LogError("EventReference is null or empty.");
            return default;
        }
        EventInstance instance = RuntimeManager.CreateInstance(eventReference);
        if (!instance.isValid())
        {
            Debug.LogError($"Failed to create FMOD instance for event: {eventReference}");
            return default;
        }
        
        return instance;
    }

}
