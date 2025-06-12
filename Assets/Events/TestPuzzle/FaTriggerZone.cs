using UnityEngine;

namespace Events.TestPuzzle
{
    public class FaTriggerZone : MonoBehaviour
    {
        [SerializeField] private BridgePuzzleController bridgeController;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fa"))
            {
                bridgeController.StartPuzzle();
            }
        }
    }
}