using UnityEngine;

public class Audio_FieldOfAge : MonoBehaviour
{
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAmbience("ambience_in_game");
        }
    }

}
