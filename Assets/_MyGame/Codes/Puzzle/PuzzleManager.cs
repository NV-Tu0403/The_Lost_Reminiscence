using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Puzzle
{
    public class PuzzleManager : MonoBehaviour
    {
        // Xử lý các câu đố trong game
        public static PuzzleManager Instance { get; private set; }
        private Action onFinish;
        private Dictionary<string, IPuzzleStep> _steps;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
            GetSteps();
        }

        private void GetSteps()      
        {
            if (_steps != null) return; 
            var steps = GetComponentsInChildren<IPuzzleStep>(true);
            _steps = new Dictionary<string, IPuzzleStep>();
            foreach (var step in steps)
            {
                _steps.Add(step.GetType().Name, step);
            }
        }
        
        public void StartPuzzle(string puzzleId, Action onFinish)
        {
            if (puzzleId == null) return;
            
            this.onFinish = onFinish;
            if (_steps.TryGetValue(puzzleId, out var step)) step.StartStep(FinishPuzzle);
            else Debug.LogError($"[PuzzleManager] Không tìm thấy bước câu đố '{puzzleId}'.");
        }
        
        private void FinishPuzzle()
        {
            //Debug.Log("[PuzzleManager] Câu đố đã hoàn thành.");
            onFinish?.Invoke();
            onFinish = null;
        }

        public void ForceCompletePuzzle(string puzzleId)
        {
            if (_steps == null) GetSteps();
            if (_steps != null && _steps.TryGetValue(puzzleId, out var step))
            {
                // Gọi hàm chuẩn hóa cho mọi loại puzzle step
                step.ForceComplete(true); // instant = true: skip hiệu ứng/VFX/audio/camera
                FinishPuzzle();
            }
        }
    }
}