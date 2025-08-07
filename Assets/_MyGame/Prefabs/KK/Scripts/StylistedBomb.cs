using UnityEngine;
using UnityEngine.VFX;

public class StylistedBomb : MonoBehaviour
{
    [SerializeField] private VisualEffect Bomb;

    private void Awake()
    {
        Bomb.Stop();
    }

    private void StartExplosion()
    {
        Bomb.Play();
        
    }
}
