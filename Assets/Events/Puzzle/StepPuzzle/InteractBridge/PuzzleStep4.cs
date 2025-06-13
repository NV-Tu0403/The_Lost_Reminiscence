using System;
using Events.Puzzle.Config.Base;
using Events.Puzzle.Scripts;
using Events.Puzzle.SO;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.InteractBridge
{
    // Step 4: Fa đi qua cầu, đến trigger bên kia, chuyển lại điều khiển/camera cho player
    public class PuzzleStep4 : MonoBehaviour, IPuzzleStep
    {
        [Tooltip("Camera của Fa.")]
        public GameObject faFollowCamera;
        [Tooltip("Camera của người chơi.")]
        public GameObject playerFollowCamera;

        public void StartStep(Action onComplete)
        {
            SwitchToPlayer();
            onComplete?.Invoke();
        }

        private void SwitchToPlayer()
        {
            // Bật camera follow của Player, tắt camera follow của Fa
            if (playerFollowCamera != null) playerFollowCamera.SetActive(true);
            if (faFollowCamera != null) faFollowCamera.SetActive(false);
            
            // Chuyển điều khiển input sang Player
            var fa = GameObject.FindGameObjectWithTag("Fa");
            var player = GameObject.FindGameObjectWithTag("Player");
            if (fa != null && player != null)
            {
                var faController = fa.GetComponent<TestController>();
                var playerController = player.GetComponent<TestController>();
                if (faController != null) faController.isActive = false;
                if (playerController != null) playerController.isActive = true;
            }
            else
            {
                Debug.LogWarning("[PuzzleStep4] Không tìm thấy Fa hoặc Player để chuyển điều khiển!");
            }
        }
    }
}
