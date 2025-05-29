using UnityEngine;
using FMODUnity;
using System.Collections;
using Unity.VisualScripting;
public class WorldAudioManager : MonoBehaviour
{
    [Header("SFX Lists")]
    [SerializeField] private EventReference[] sfx;

    private void Start()
    {
        if (sfx == null || sfx.Length == 0)
        {
            Debug.LogWarning("No SFX assigned to WorldAudioManager.");
            return;
        }

        StartCoroutine(RandomSFX());
    }

    IEnumerator RandomSFX()
    {
        while(this.IsDestroyed() == false)
        {
            int randomIndex = Random.Range(0, sfx.Length);
            EventReference randomSFX = sfx[randomIndex];            
            AudioManager.instance.PlayOneShot(randomSFX, transform.position);
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }
}
