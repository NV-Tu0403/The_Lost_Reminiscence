using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerAudioManager : MonoBehaviour
{
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootstepReference {get; private set;}

    public static PlayerAudioManager Instance {get; private set;}

    [field: SerializeField] public EventInstance playerFootstepInstance {get; private set;}
    
    void Awake()
    {
        playerFootstepInstance = RuntimeManager.CreateInstance(playerFootstepReference);
        playerFootstepInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
    }

    void Start() {
        Debug.Log("PlayerAudioManager.Instance: " + (PlayerAudioManager.Instance != null));
        Debug.Log("FootstepInstance: " + (PlayerAudioManager.Instance?.playerFootstepInstance != null));
        PlayerAudioManager.Instance.playerFootstepInstance.start();
    }
}
