using _MyGame.Codes.GameEventSystem;
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
        private GameObject playerObject;

        [SerializeField] private string playerTag = "Player";

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
            // Tìm player qua tag
            FindPlayerByTag();
        }

        private void FindPlayerByTag()
        {
            playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerNavAgent = playerObject.GetComponent<NavMeshAgent>();
                playerRigidbody = playerObject.GetComponent<Rigidbody>();
            }
            else
            {
                Debug.LogWarning($"[PlayerRespawnManager] Không tìm thấy GameObject với tag '{playerTag}'");
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe("Respawn", OnRespawnEvent);
            EventBus.Unsubscribe("Checkpoint", OnCheckpointEvent);
        }

        private void OnCheckpointEvent(object data)
        {
            if (playerObject == null)
            {
                FindPlayerByTag(); // Thử tìm lại nếu null
                if (playerObject == null) return;
            }
            
            lastCheckpointPosition = playerObject.transform.position;
            lastCheckpointRotation = playerObject.transform.rotation;
        }

        private void OnRespawnEvent(object data)
        {
            if (playerObject != null)
            {
                TeleportWithComponentCheck(lastCheckpointPosition, lastCheckpointRotation);
            }
        }

        public void TeleportToCheckpoint(Vector3 position, Quaternion rotation)
        {
            if (playerObject == null)
            {
                FindPlayerByTag();
                if (playerObject == null) return;
            }

            TeleportWithComponentCheck(position, rotation);
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
            Debug.Log("[PlayerRespawnManager] Teleported player to checkpoint (DevMode skip).");
        }

        private void TeleportWithComponentCheck(Vector3 position, Quaternion rotation)
        {
            if (playerObject == null) return;

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
            playerObject.transform.position = position;
            playerObject.transform.rotation = rotation;

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

            if (playerRigidbody == null) return;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = false;
        }
    }
}