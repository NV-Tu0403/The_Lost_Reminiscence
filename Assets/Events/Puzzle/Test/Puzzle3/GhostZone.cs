using System.Collections.Generic;
using UnityEngine;

namespace Events.Puzzle.Test.Puzzle3
{
    public class GhostZone : MonoBehaviour
    {
        [SerializeField] private List<Ghost> ghosts; // Danh sách các ghost trong zone
        private int talkCount = 0; // Đếm số lần nói chuyện với ghost
        private bool isChasing = false; // Trạng thái toàn bộ ghost có đang đuổi không
        private Transform playerTarget;
        [SerializeField] private Transform teleportTarget;
        private int hitCount = 0;

        private void Awake()
        {
            // Tìm player theo tag khi bắt đầu
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTarget = playerObj.transform;
        }

        public void OnTalkToGhost(Ghost ghost)
        {
            if (isChasing) return;
            talkCount++;
            if (talkCount >= 5)
            {
                StartChaseAllGhosts();
            }
        }

        // Reset toàn bộ ghost về trạng thái ban đầu
        public void ResetAllGhosts()
        {
            talkCount = 0;
            isChasing = false;
            foreach (var ghost in ghosts)
            {
                ghost.ResetState();
            }
        }

        public void OnGhostHitPlayer()
        {
            hitCount++;
            FadeManager.Instance.FadeToBlack(() =>
            {
                if (hitCount >= 3)
                {
                    if (playerTarget != null && teleportTarget != null)
                    {
                        var cc = playerTarget.GetComponent<CharacterController>();
                        var rb = playerTarget.GetComponent<Rigidbody>();
                        if (cc != null)
                        {
                            cc.enabled = false;
                            playerTarget.position = teleportTarget.position;
                            cc.enabled = true;
                        }
                        else if (rb != null)
                        {
                            rb.isKinematic = true;
                            playerTarget.position = teleportTarget.position;
                            rb.isKinematic = false;
                        }
                        else
                        {
                            playerTarget.position = teleportTarget.position;
                        }
                    }
                    hitCount = 0;
                    // Reset toàn bộ ghost về trạng thái ban đầu ngay khi teleport
                    ResetAllGhosts();
                    FadeManager.Instance.FadeFromBlack();
                }
                else
                {
                    FadeManager.Instance.FadeFromBlack();
                }
            });
        }

        private void StartChaseAllGhosts()
        {
            isChasing = true;
            foreach (var ghost in ghosts)
            {
                ghost.SetTarget(playerTarget); // Đảm bảo mọi ghost đều nhận target từ GhostZone
                ghost.StartChase();
            }
        }
    }
}