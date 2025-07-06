using System;
using Script.Puzzle.LightTree;
using UnityEngine;

namespace Code.Puzzle.LightTree
{
    public class PuzzleStep7 : MonoBehaviour, IPuzzleStep
    {
        private int _remainingEnemies;
        private Action _onComplete;
        
        public void StartStep(Action onComplete)
        {
            _onComplete = onComplete;
            _remainingEnemies = 0;
            
            // Đăng ký DestroyNotifier cho IdController
            CheckDestroyIds();

            // Đăng ký DestroyNotifier cho SupController
            CheckDestroySups();

            // Kiểm tra trong trường hợp không còn enemy nào từ đầu
            if (_remainingEnemies == 0)
            {
                Debug.Log("[PuzzleStep7] No enemies to track. Step complete.");
                _onComplete?.Invoke();
            }
        }

        public void ForceComplete(bool instant = true)
        {
            throw new NotImplementedException();
        }

        private void CheckDestroySups()
        {
            var allSups = FindObjectsOfType<SupController>();
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
        }

        private void CheckDestroyIds()
        {
            var allIds = FindObjectsOfType<IdController>();
           
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
