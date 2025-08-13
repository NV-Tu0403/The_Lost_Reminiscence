using Code.GameEventSystem;
using UnityEngine;
using UnityEngine.AI;
using DuckLe;

namespace Code.Character
{
    public class PlayerRespawnManager : MonoBehaviour
    {
        public static PlayerRespawnManager Instance { get; private set; }

        private Vector3 lastCheckpointPosition;
        private Quaternion lastCheckpointRotation;
        
        // Thêm các component references
        private NavMeshAgent playerNavAgent;
        private Rigidbody playerRigidbody;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EventBus.Subscribe("Respawn", OnRespawnEvent);
            EventBus.Subscribe("Checkpoint", OnCheckpointEvent);
        }

        private void Start()
        {
            // Cache component references
            var player = PlayerController.Instance;
            if (player != null)
            {
                playerNavAgent = player.GetComponent<NavMeshAgent>();
                playerRigidbody = player.GetComponent<Rigidbody>();
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe("Respawn", OnRespawnEvent);
            EventBus.Unsubscribe("Checkpoint", OnCheckpointEvent);
        }

        private void OnCheckpointEvent(object data)
        {
            var player = PlayerController.Instance;
            if (player == null) return;
            lastCheckpointPosition = player.transform.position;
            lastCheckpointRotation = player.transform.rotation;
        }

        private void OnRespawnEvent(object data)
        {
            var player = PlayerController.Instance;
            if (player != null)
            {
                TeleportWithComponentCheck(lastCheckpointPosition, lastCheckpointRotation);
            }
        }

        public void TeleportToCheckpoint(Vector3 position, Quaternion rotation)
        {
            var player = PlayerController.Instance;
            if (player == null) return;
            
            TeleportWithComponentCheck(position, rotation);
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
            Debug.Log("[PlayerRespawnManager] Teleported player to checkpoint (DevMode skip).");
        }

        private void TeleportWithComponentCheck(Vector3 position, Quaternion rotation)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            // Disable NavMeshAgent nếu có
            if (playerNavAgent != null && playerNavAgent.enabled)
            {
                playerNavAgent.enabled = false;
            }

            // Disable Rigidbody physics nếu có
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = true;
            }

            // Thực hiện teleport
            player.transform.position = position;
            player.transform.rotation = rotation;

            // Re-enable components
            if (playerNavAgent != null)
            {
                // Warp NavMeshAgent đến vị trí mới trước khi enable
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    playerNavAgent.Warp(hit.position);
                }
                playerNavAgent.enabled = true;
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.isKinematic = false;
            }
        }
    }
}