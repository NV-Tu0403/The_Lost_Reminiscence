using Events.Puzzle.StepPuzzle.LightTree;
using UnityEngine;

// Đã DEPRECATED: Không dùng Puzzle3 để quản lý gameplay nữa.
// Toàn bộ logic gameplay (ghost, NPC, reset, máu, v.v.) nên chuyển sang các component riêng biệt hoặc step mới.
// Script này chỉ giữ lại để tham khảo hoặc migrate dần sang hệ thống event-driven mới.

namespace Events.Puzzle.Test.PuzzleDemo
{
    public class Puzzle3 : MonoBehaviour
    {
        public static Puzzle3 Instance { get; private set; }


        [SerializeField] private int maxSpirits = 5;
        public int currentSpirits;

        private bool _idCleared;

        [SerializeField] private TestController player;
        [SerializeField] private Transform respawnPoint;

        public IdController[] ghosts;
        private Vector3[] ghostStartPositions;

        public SupController[] npcs;
        private Vector3[] npcStartPositions;

        private void Awake()
        {
            Instance = this;
            currentSpirits = maxSpirits;
            // Lưu vị trí ban đầu của ghost
            if (ghosts != null && ghosts.Length > 0)
            {
                ghostStartPositions = new Vector3[ghosts.Length];
                for (int i = 0; i < ghosts.Length; i++)
                {
                    ghostStartPositions[i] = ghosts[i].transform.position;
                }
            }
            // Lưu vị trí ban đầu của NPC
            if (npcs != null && npcs.Length > 0)
            {
                npcStartPositions = new Vector3[npcs.Length];
                for (int i = 0; i < npcs.Length; i++)
                {
                    npcStartPositions[i] = npcs[i].transform.position;
                }
            }
        }

        private void Start()
        {
            UISpirit ui = FindObjectOfType<UISpirit>();
            if (ui != null) ui.SetSpirit(currentSpirits, maxSpirits);
        }

        public void ReduceSpirit(int amount)
        {
            currentSpirits -= amount;
            UISpirit ui = FindObjectOfType<UISpirit>();
            if (ui != null) ui.SetSpirit(currentSpirits, maxSpirits);
            if (currentSpirits <= 0) ResetPuzzle();
        }

        public void ResetPuzzle()
        {
            Debug.Log("Reset puzzle");

            // Đặt lại máu
            currentSpirits = maxSpirits;
            UISpirit ui = FindObjectOfType<UISpirit>();
            if (ui != null) ui.SetSpirit(currentSpirits, maxSpirits);

            // Đóng DialogueSupUI nếu đang mở
            UISupDialogue uiSupDialogueUI = FindObjectOfType<UISupDialogue>();
            if (uiSupDialogueUI != null) uiSupDialogueUI.Close();

            // Dịch chuyển player về vị trí respawn
            if (player != null && respawnPoint != null)
                player.TeleportTo(respawnPoint.position);

            // Đặt lại vị trí ghost và dừng chase
            if (ghosts != null && ghostStartPositions != null)
            {
                for (int i = 0; i < ghosts.Length; i++)
                {
                    ghosts[i].transform.position = ghostStartPositions[i];
                    ghosts[i].ResetChase();
                }
            }

            // Đặt lại vị trí và trạng thái NPC
            if (npcs != null && npcStartPositions != null)
            {
                for (int i = 0; i < npcs.Length; i++)
                {
                    npcs[i].transform.position = npcStartPositions[i];
                    npcs[i].ResetState();
                }
            }
        }
    }
}