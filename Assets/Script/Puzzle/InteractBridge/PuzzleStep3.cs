using System;
using UnityEngine;

namespace Script.Puzzle.InteractBridge
{
    public class PuzzleStep3 : MonoBehaviour, IPuzzleStep
    {
        private Action _onComplete;
        
        public void StartStep(Action onComplete)
        {
            _onComplete = onComplete;
            FaUseSkill();
            _onComplete?.Invoke();
        }

        private void FaUseSkill()
        {
            //TODO: Implement logic for Fa to use skill to interact with the bridge
            Debug.Log("[PuzzleStep3] Fa đang sử dụng kỹ năng để tương tác với cầu.");
        }
    }
}