using System;
using UnityEngine;

namespace Code.Puzzle.InteractBridge
{
    public class PuzzleStep3 : MonoBehaviour, IPuzzleStep
    {
        private Action onComplete;
        
        public void StartStep(Action onComplete)
        {
            this.onComplete = onComplete;
            FaUseSkill();
            this.onComplete?.Invoke();
        }

        private void FaUseSkill()
        {
            //TODO: Implement logic for Fa to use skill to interact with the bridge
            Debug.Log("[PuzzleStep3] Fa đang sử dụng kỹ năng để tương tác với cầu.");
        }
        
        // This method is called to force complete the step, if needed
        public void ForceComplete(bool instant = true) {}
    }
}