using UnityEngine;

namespace Events.TestPuzzle
{
    public class DeadZone : MonoBehaviour
    {
        public BridgePuzzleController puzzleController;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name);
            if (other.CompareTag("Player"))
            {
                puzzleController.ResetPlayer();
                puzzleController.StartCoroutine(puzzleController.ResetPuzzleAfterFail());
            }
        }
    }
}