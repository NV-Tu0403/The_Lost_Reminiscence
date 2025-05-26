using UnityEngine;
using FMODUnity;
public class PlayOneShotAudioFMOD : MonoBehaviour
{
    [SerializeField] private EventReference eraserSound;
    void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!eraserSound.IsNull) AudioManager.instance.PlayOneShot(eraserSound, this.transform.position);
        }
    }
}
