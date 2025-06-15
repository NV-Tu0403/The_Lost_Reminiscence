using System;
using System.Collections.Generic;
using Events.Puzzle.Config.Base;
using Events.Puzzle.Scripts;
using Events.Puzzle.SO;
using TMPro;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.InteractBridge
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