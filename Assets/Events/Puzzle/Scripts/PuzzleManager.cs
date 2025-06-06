using UnityEngine;

namespace Events.Puzzle.Scripts
{
    public class PuzzleManager : MonoBehaviour
    {
        // Xử lý các câu đố trong game
        
        public static PuzzleManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }
        
        public void StartPuzzle(string puzzleId)
        {
            Debug.Log($"Starting puzzle {puzzleId}");
        }
    }
}