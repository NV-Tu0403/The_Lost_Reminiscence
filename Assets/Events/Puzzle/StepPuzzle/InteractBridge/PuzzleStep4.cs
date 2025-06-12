using System;
using Events.Puzzle.Scripts;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.InteractBridge
{
    // Step 4: Fa đi qua cầu, đến trigger bên kia, chuyển lại điều khiển/camera cho player
    public class PuzzleStep4 : MonoBehaviour, IPuzzleStep
    {
        public void StartStep(Action onComplete, bool isRetry = false)
        {
            if (!isRetry)
            {
                // TODO: Hiện dialogue/cutscene nếu cần
                // Sau khi dialogue xong, chuyển lại điều khiển/camera cho player
                // (Có thể dùng callback hoặc coroutine để chờ dialogue xong)
            }
            // Chuyển lại điều khiển/camera cho player (gọi PlayerControllerManager.Instance.SwitchToPlayer() hoặc tương tự)
            // Khi xong, gọi onComplete();
            onComplete?.Invoke();
        }
    }
}

