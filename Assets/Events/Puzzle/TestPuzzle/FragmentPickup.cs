using UnityEngine;

namespace Events.Puzzle.TestPuzzle
{
    using UnityEngine;

    public class FragmentPickup : MonoBehaviour
    {
        public PuzzleTest puzzle;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                puzzle.InsertFragment();
                gameObject.SetActive(false); // Ẩn mảnh sau khi gắn
            }
        }
    }

}