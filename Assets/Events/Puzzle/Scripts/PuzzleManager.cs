using System;
using UnityEngine;

namespace Events.Puzzle.Scripts
{
    public class PuzzleManager : MonoBehaviour
    {
        // Xử lý các câu đố trong game
        public static PuzzleManager Instance { get; private set; }
        
        private Action _onFinish;
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }
        
        public void StartPuzzle(string puzzleId, Action onFinish)
        {
            if (puzzleId == null) return;
            
            _onFinish = onFinish;
            //Logic xử lí puzzle sẽ được thực hiện ở đây
            Debug.Log($"Starting puzzle {puzzleId}");
        }
    }
}