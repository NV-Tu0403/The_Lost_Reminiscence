using System;
using System.Collections.Generic;
using Events.Puzzle.StepPuzzle;
using Events.Puzzle.StepPuzzle.InteractBridge;
using Events.Puzzle.StepPuzzle.OpenGate;
using UnityEngine;

namespace Events.Puzzle.Scripts
{
    public class PuzzleManager : MonoBehaviour
    {
        // Xử lý các câu đố trong game
        public static PuzzleManager Instance { get; private set; }
        private Action _onFinish;
        private Dictionary<string, IPuzzleStep> _steps;
        
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
            GetSteps();
        }

        private void GetSteps()      // Khai báo tất cả các bước câu đố từ scene
        {
            // Lấy tất cả các bước câu đố từ scene
            _steps = new Dictionary<string, IPuzzleStep>();
            
            // Test: Đăng ký bước câu đố 1
            var step1 = GetComponentInChildren<PuzzleStep1>();
            RegisterStep("Puzzle_1", step1);
            
            // Test: Đăng ký bước câu đố 2
            var step2 = GetComponentInChildren<PuzzleStep2>();
            RegisterStep("Puzzle_2", step2);
            
            // Test: Đăng ký bước câu đố 3
            var step3 = GetComponentInChildren<PuzzleStep3>();
            RegisterStep("Puzzle_3", step3);
            
            // Test: Đăng ký bước câu đố 4
            var step4 = GetComponentInChildren<PuzzleStep4>();
            RegisterStep("Puzzle_4", step4);
            
            // Test: Đăng ký bước câu đố 5
            var step5 = GetComponentInChildren<PuzzleStep5>();
            RegisterStep("Puzzle_5", step5);
        }

        public void RegisterStep(string stepId, IPuzzleStep step)
        {
            if (string.IsNullOrEmpty(stepId) || step == null) return;
            if (_steps.ContainsKey(stepId))
            {
                Debug.LogWarning($"[PuzzleManager] Step '{stepId}' đã được đăng ký.");
                return;
            }
            _steps[stepId] = step;
        }
        
        public void StartPuzzle(string puzzleId, Action onFinish)
        {
            if (puzzleId == null) return;
            _onFinish = onFinish;
            if (_steps.TryGetValue(puzzleId, out var step)) step.StartStep(FinishPuzzle);
            else Debug.LogError($"[PuzzleManager] Không tìm thấy bước câu đố '{puzzleId}'.");
        }
        
        private void FinishPuzzle()
        {
            Debug.Log("[PuzzleManager] Câu đố đã hoàn thành.");
            _onFinish?.Invoke();
            _onFinish = null;
        }
    }
}