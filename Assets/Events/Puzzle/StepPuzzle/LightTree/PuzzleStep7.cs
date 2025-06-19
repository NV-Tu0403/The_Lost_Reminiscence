using System;
using Events.Puzzle.Scripts;
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
            Debug.Log("[PuzzleStep7] StartStep called");
            _onComplete = onComplete;
            if (faController != null)
            {
                Debug.Log($"[PuzzleStep7] faController found: {faController.name}");
                faController.OnSkillUsed -= HandleSkillUsed;
                faController.OnSkillUsed += HandleSkillUsed;
            }
            else
            {
                Debug.LogError("[PuzzleStep7] faController is null!");
            }
        }

        private void OnDestroy()
        {
            if (faController != null)
                faController.OnSkillUsed -= HandleSkillUsed;
        }

        private void HandleSkillUsed()
        {
            Debug.Log("[PuzzleStep7] HandleSkillUsed called");
            // Kiểm tra nếu toàn bộ ids và sups đã bị phá hủy
            bool allIdsDestroyed = ids == null || ids.Length == 0 || Array.TrueForAll(ids, x => x == null);
            bool allSupsDestroyed = sups == null || sups.Length == 0 || Array.TrueForAll(sups, x => x == null);
            if (allIdsDestroyed && allSupsDestroyed)
            {
                Debug.Log("[PuzzleStep7] All IDs and SUPs destroyed, completing step.");
                _onComplete?.Invoke();
            }
        }
    }
}