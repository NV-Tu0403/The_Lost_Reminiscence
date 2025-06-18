using System;
using Events.Puzzle.Scripts;
using Events.Puzzle.Test.PuzzleDemo;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.LightTree
{
    public class PuzzleStep7 : MonoBehaviour, IPuzzleStep
    {
        
        [SerializeField] private IdController[] ids;
        [SerializeField] private SupController[] sups;
        [SerializeField] private FaController faController;

        private Action _onComplete;
        
        public void StartStep(Action onComplete)
        {
            _onComplete = onComplete;
            if (faController != null)
            {
                faController.OnSkillUsed -= HandleSkillUsed;
                faController.OnSkillUsed += HandleSkillUsed;
            }
        }

        private void OnDestroy()
        {
            if (faController != null)
                faController.OnSkillUsed -= HandleSkillUsed;
        }

        private void HandleSkillUsed()
        {
            // Kiểm tra nếu toàn bộ ids và sups đã bị phá hủy
            bool allIdsDestroyed = ids == null || ids.Length == 0 || Array.TrueForAll(ids, x => x == null);
            bool allSupsDestroyed = sups == null || sups.Length == 0 || Array.TrueForAll(sups, x => x == null);
            if (allIdsDestroyed && allSupsDestroyed)
            {
                _onComplete?.Invoke();
            }
        }
    }
}