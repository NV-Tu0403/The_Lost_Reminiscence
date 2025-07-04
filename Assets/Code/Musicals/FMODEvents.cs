using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootstepEvent {get; private set;}

    public static FMODEvents Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one instance of FMODEvents found!");
        }
        Instance = this;
    }
}
