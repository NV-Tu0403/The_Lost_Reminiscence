using System;
using Events.Puzzle.Scripts;
using Events.Puzzle.Test.PuzzleDemo;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.LightTree
{
    public class PuzzleStep6 :  MonoBehaviour, IPuzzleStep
    { 
        [Header("Puzzle Step 6 Settings")]
        [Tooltip("Danh sách các Id cần quản lí trong bước này.")]
        [SerializeField] private IdController[] ids;
        
        [Tooltip("Danh sách các NPC Sup cần quản lí trong bước này.")]
        [SerializeField] private SupController[] sups;
        
        [Header("Vị trí ban đầu của Id và Sup, có thể để trống nếu không cần thiết.")]
        [SerializeField] private Vector3[] idStartPositions;
        [SerializeField] private Vector3[] supStartPositions; 
        
        [Header("Player & Máu")]
        [SerializeField] private PlayerSpirit playerSpirit;
        [SerializeField] private TestController player;
        [SerializeField] private Transform respawnPoint;
        
        [Header("UI Dialogue Test")]
        [SerializeField] private UISupDialogue uiSupDialogue;
        
        private Action _onComplete;

        public void StartStep(Action onComplete)
        {
            _onComplete = onComplete;
            // Lưu vị trí ban đầu của id và sup
            if (ids != null && ids.Length > 0 && (idStartPositions == null || idStartPositions.Length != ids.Length))
            {
                idStartPositions = new Vector3[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                    idStartPositions[i] = ids[i].transform.position;
            }
            if (sups != null && sups.Length > 0 && (supStartPositions == null || supStartPositions.Length != sups.Length))
            {
                supStartPositions = new Vector3[sups.Length];
                for (int i = 0; i < sups.Length; i++)
                    supStartPositions[i] = sups[i].transform.position;
            }
            // Không cần lưu playerStartPosition nữa
            if (playerSpirit != null)
                playerSpirit.OnSpiritDepleted += HandlePlayerDead;
        }

        private void HandlePlayerDead()
        {
            // Reset player về respawnPoint
            if (player != null && respawnPoint != null)
                player.transform.position = respawnPoint.position;
            // Reset Id và Sup
            ResetIdAndSup();
            // Hồi máu cho player
            if (playerSpirit != null)
                playerSpirit.ResetSpirit();
            // reset UI, dialogue
            if (uiSupDialogue != null)
                uiSupDialogue.Close();
        }

        // Hàm reset Id và sup về vị trí ban đầu và trạng thái mặc định
        public void ResetIdAndSup()
        {
            if (ids != null && idStartPositions != null)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i].transform.position = idStartPositions[i];
                    ids[i].ResetChase();
                }
            }
            if (sups != null && supStartPositions != null)
            {
                for (int i = 0; i < sups.Length; i++)
                {
                    sups[i].transform.position = supStartPositions[i];
                    sups[i].ResetState();
                }
            }
        }

        // Hàm này sẽ được gọi khi bước này hoàn thành
        private void OnDestroy()
        {
            if (playerSpirit != null)
                playerSpirit.OnSpiritDepleted -= HandlePlayerDead;
        }
    }
}