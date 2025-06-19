using System;
using UnityEngine;
using Events.Puzzle.Scripts;

namespace Events.Puzzle.StepPuzzle.LightTree
{
    public class PuzzleStep7 : MonoBehaviour, IPuzzleStep
    {
        private int _remainingEnemies;
        private Action _onComplete;
        
        public void StartStep(Action onComplete)
        {
            Debug.Log("[PuzzleStep7] StartStep called");
            _onComplete = onComplete;
            _remainingEnemies = 0;

            var allIds = FindObjectsOfType<IdController>();
            var allSups = FindObjectsOfType<SupController>();

            // Đăng ký DestroyNotifier cho IdController
            foreach (var id in allIds)
            {
                if (id != null && id.gameObject != null)
                {
                    _remainingEnemies++;
                    var notifier = id.gameObject.GetComponent<DestroyNotifier>() 
                                   ?? id.gameObject.AddComponent<DestroyNotifier>();
                    notifier.onDestroyed += OnEnemyDestroyed;
                }
            }

            // Đăng ký DestroyNotifier cho SupController
            foreach (var sup in allSups)
            {
                if (sup != null && sup.gameObject != null)
                {
                    _remainingEnemies++;
                    var notifier = sup.gameObject.GetComponent<DestroyNotifier>() 
                                   ?? sup.gameObject.AddComponent<DestroyNotifier>();
                    notifier.onDestroyed += OnEnemyDestroyed;
                }
            }

            Debug.Log($"[PuzzleStep7] Total enemies to track: {_remainingEnemies}");

            // Kiểm tra trong trường hợp không còn enemy nào từ đầu
            if (_remainingEnemies == 0)
            {
                Debug.Log("[PuzzleStep7] No enemies to track. Step complete.");
                _onComplete?.Invoke();
            }
        }

        private void OnEnemyDestroyed()
        {
            _remainingEnemies--;
            Debug.Log($"[PuzzleStep7] Enemy destroyed. Remaining: {_remainingEnemies}");
            if (_remainingEnemies <= 0)
            {
                Debug.Log("[PuzzleStep7] All enemies destroyed. Step complete.");
                _onComplete?.Invoke();
            }
        }
    }
    
    public class DestroyNotifier : MonoBehaviour
    {
        public event Action onDestroyed;

        private void OnDestroy()
        {
            onDestroyed?.Invoke();
        }
    }
}
