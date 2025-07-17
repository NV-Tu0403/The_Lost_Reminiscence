using System;
using UnityEngine;

namespace Code.Puzzle.InteractBridge
{
    // Step 4: Fa đi qua cầu, đến trigger bên kia, chuyển lại điều khiển/camera cho player
    public class PuzzleStep4 : MonoBehaviour, IPuzzleStep
    {
        public void StartStep(Action onComplete)
        {
            //TODO: Implement logic for Fa to walk across the bridge and reach the trigger on the other side
            Debug.Log("[PuzzleStep4] Fa đã bay qua cầu và đến trigger bên kia.");
            onComplete?.Invoke();
        }

        // This method is called to force complete the step, if needed
        public void ForceComplete(bool instant = true) { }
    }
}
