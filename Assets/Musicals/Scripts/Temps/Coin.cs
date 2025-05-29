using UnityEngine;
using FMODUnity;
public class Coin : MonoBehaviour
{
    [field: Header("Coin Pickup Sound")]
    [field: SerializeField] public EventReference coinPickupSound { get; private set; }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.instance.PlayOneShot(coinPickupSound, this.transform.position);
        }
    }
}
