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
        [Header("Camera Follow")]
        [Tooltip("Camera của Fa.")]
        public GameObject faFollowCamera;
        [Tooltip("Camera của người chơi.")]
        public GameObject playerFollowCamera;
        
        private Action _onComplete;
        
        public void StartStep(Action onComplete)
        {
            _onComplete = onComplete;
            SwitchToFa();
            _onComplete?.Invoke();
        }

        private void SwitchToFa()
        {
            // Bật camera follow của Fa, tắt camera follow của Player
            if (faFollowCamera != null) faFollowCamera.SetActive(true);
            if (playerFollowCamera != null) playerFollowCamera.SetActive(false);
            
            // Chuyển điều khiển input sang Fa
            var fa = GameObject.FindGameObjectWithTag("Fa");
            var player = GameObject.FindGameObjectWithTag("Player");
            if (fa != null && player != null)
            {
                var faController = fa.GetComponent<CharacterController>();
                var playerController = player.GetComponent<CharacterController>();
                if (faController != null) faController.isActive = true;
                if (playerController != null) playerController.isActive = false;
            }
            else
            {
                Debug.LogWarning("[PuzzleStep3] Không tìm thấy Fa hoặc Player để chuyển điều khiển!");
            }
        }
    }
}