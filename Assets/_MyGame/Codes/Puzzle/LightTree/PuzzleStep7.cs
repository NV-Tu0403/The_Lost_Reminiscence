using System;
using _MyGame.Codes.Puzzle.LightTree;
using Script.Puzzle.LightTree;
using UnityEngine;

namespace Code.Puzzle.LightTree
{
    public class PuzzleStep7 : MonoBehaviour, IPuzzleStep
    {
        private int remainingEnemies;
        private Action onComplete;
        
        public void StartStep(Action onComplete)
        {
            this.onComplete = onComplete;
            remainingEnemies = 0;
            
            // Đăng ký DestroyNotifier cho IdController
            CheckDestroyIds();

            // Đăng ký DestroyNotifier cho SupController
            CheckDestroySups();

            // Kiểm tra trong trường hợp không còn enemy nào từ đầu
            if (remainingEnemies == 0)
            {
                Debug.Log("[PuzzleStep7] No enemies to track. Step complete.");
                this.onComplete?.Invoke();
            }
        }

        private void CheckDestroySups()
        {
            var allSups = FindObjectsByType<SupController>(FindObjectsSortMode.None);
            foreach (var sup in allSups)
            {
                if (sup == null || sup.gameObject == null) continue;
                remainingEnemies++;
                var notifier = sup.gameObject.GetComponent<DestroyNotifier>() 
                               ?? sup.gameObject.AddComponent<DestroyNotifier>();
                notifier.onDestroyed += OnEnemyDestroyed;
            }
        }

        private void CheckDestroyIds()
        {
            var allIds = FindObjectsByType<IdController>(FindObjectsSortMode.None);
           
            foreach (var id in allIds)
            {
                if (id != null && id.gameObject != null)
                {
                    remainingEnemies++;
                    var notifier = id.gameObject.GetComponent<DestroyNotifier>() 
                                   ?? id.gameObject.AddComponent<DestroyNotifier>();
                    notifier.onDestroyed += OnEnemyDestroyed;
                }
            }
        }

        private void OnEnemyDestroyed()
        {
            remainingEnemies--;
            Debug.Log($"[PuzzleStep7] Enemy destroyed. Remaining: {remainingEnemies}");
            if (remainingEnemies > 0) return;
            Debug.Log("[PuzzleStep7] All enemies destroyed. Step complete.");
            onComplete?.Invoke();
        }
        
        // Phương thức này sẽ được gọi khi người chơi muốn hoàn thành bước này ngay lập tức
        public void ForceComplete(bool instant = true) {}
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
