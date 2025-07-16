using Code.GameEventSystem;
using UnityEngine;
using DuckLe;

namespace Code.Character
{
    public class PlayerRespawnManager : MonoBehaviour
    {
        public static PlayerRespawnManager Instance { get; private set; }

        private Vector3 _lastCheckpointPosition;
        private Quaternion _lastCheckpointRotation;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            //DontDestroyOnLoad(gameObject);

            EventBus.Subscribe("Respawn", OnRespawnEvent);
            EventBus.Subscribe("Checkpoint", OnCheckpointEvent);
            //Debug.Log("[PlayerRespawnManager] Subscribed to Respawn and Checkpoint events.");
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe("Respawn", OnRespawnEvent);
            EventBus.Unsubscribe("Checkpoint", OnCheckpointEvent);
        }

        // Lưu lại vị trí checkpoint khi player đi qua CheckpointZone
        private void OnCheckpointEvent(object data)
        {
            var player = PlayerController.Instance;
            if (player != null)
            {
                _lastCheckpointPosition = player.transform.position;
                _lastCheckpointRotation = player.transform.rotation;
            }
        }

        // Xử lý respawn khi nhận sự kiện
        private void OnRespawnEvent(object data)
        {
            var player = PlayerController.Instance;
            if (player != null)
            {
                player.Teleport(_lastCheckpointPosition, _lastCheckpointRotation);
                // TODO: Thêm hiệu ứng respawn, reset trạng thái, v.v.
            }
        }

        // Thêm hàm này để cho phép teleport đến checkpoint bất kỳ
        public void TeleportToCheckpoint(Vector3 position, Quaternion rotation)
        {
            var player = PlayerController.Instance;
            if (player != null)
            {
                player.Teleport(position, rotation);
                _lastCheckpointPosition = position;
                _lastCheckpointRotation = rotation;
                Debug.Log("[PlayerRespawnManager] Teleported player to checkpoint (DevMode skip).");
            }
        }
    }
}