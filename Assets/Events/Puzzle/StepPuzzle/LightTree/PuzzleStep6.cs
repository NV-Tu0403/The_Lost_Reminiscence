using System;
using Events.Puzzle.Scripts;
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
        [SerializeField] private Transform respawnPoint;
        
        [Header("UI Dialogue Test")]
        [SerializeField] private UISupDialogue uiSupDialogue;
        
        private Action _onComplete;

        // --- Thêm biến để kiểm tra đã chết ở từng zone ---
        private bool diedInZone1 = false;
        private bool diedInZone2 = false;
        private int playerCurrentZone = 0;

        public void StartStep(Action onComplete)
        {
            Debug.Log("[PuzzleStep6] StartStep called");
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

            Debug.Log("[PuzzleStep6] player die  => reset");
            if (playerSpirit != null)
                playerSpirit.OnSpiritDepleted += HandlePlayerDead;
        }

        private void HandlePlayerDead()
        {
            // Reset player về respawnPoint
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && respawnPoint != null)
                playerObj.transform.position = respawnPoint.position;
            // Reset Id và Sup
            ResetIdAndSup();
            // Hồi máu cho player
            if (playerSpirit != null)
                playerSpirit.ResetSpirit();
            // reset UI, dialogue
            if (uiSupDialogue != null)
                uiSupDialogue.Close();

            NotifyPlayerDiedInZone(playerCurrentZone);
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

        // Gọi từ TriggerZone khi người chơi chết ở zone tương ứng
        public void NotifyPlayerDiedInZone(int zoneIndex)
        {
            if (zoneIndex == 1)
                diedInZone1 = true;
            else if (zoneIndex == 2)
                diedInZone2 = true;
            
            CheckBothZonesDied();
        }

        // Kiểm tra nếu đã chết ở cả 2 zone thì gọi event tiếp theo
        private void CheckBothZonesDied()
        {
            if (diedInZone1 && diedInZone2)
            {
                if (_onComplete != null)
                    _onComplete.Invoke();
            }
        }

        // public bool HasDiedInZone(int zoneIndex)
        // {
        //     if (zoneIndex == 1) return diedInZone1;
        //     if (zoneIndex == 2) return diedInZone2;
        //     return false;
        // }

        // Trả về danh sách các IdController (ghost) để các TriggerZone sử dụng
        public IdController[] GetIds()
        {
            return ids;
        }

        public void SetPlayerCurrentZone(int zoneIndex)
        {
            playerCurrentZone = zoneIndex;
        }

        // Hàm này sẽ được gọi khi bước này hoàn thành
        private void OnDestroy()
        {
            if (playerSpirit != null)
                playerSpirit.OnSpiritDepleted -= HandlePlayerDead;
        }
    }
}