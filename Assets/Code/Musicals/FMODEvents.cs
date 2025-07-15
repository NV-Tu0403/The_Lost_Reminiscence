using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{  
    [field: Header("Music")]
    [field: SerializeField] public EventReference m_MusicGame {get; private set;}

    public static FMODEvents Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one instance of FMODEvents found!");
        }
        Instance = this;
    }

    private void Start()
    {
        AudioManager.Instance.PlayAmbience(m_MusicGame);
    }



}
