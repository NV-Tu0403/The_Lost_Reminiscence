using UnityEngine;

namespace Events.Puzzle.TestPuzzle
{
    public class RockTrigger : MonoBehaviour
    {
        public PuzzleTest puzzle;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Rock"))
            {
                puzzle.TriggerRock();
            }
        }
    }
}