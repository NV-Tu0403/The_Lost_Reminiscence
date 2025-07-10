using UnityEngine;
using FMOD.Studio;

namespace Events
{
    [RequireComponent(typeof(TestController))]
    public class TestController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float gravity = -9.81f;
        private float verticalVelocity;
        private CharacterController controller;

        [Header("Control")]
        public bool isActive = true;

        [Header("Audio")]
        public EventInstance footstepEvent;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            Debug.Log("FootstepEvent: " + footstepEvent);
            if (!PlayerAudioManager.Instance.playerFootstepInstance.isValid())
            {
                Debug.LogWarning("Khứa PlayerFootstepIntance bên [PlayAudioManager] null nè trời");
            }
            else
                footstepEvent = PlayerAudioManager.Instance.playerFootstepInstance;
        }

        void Update()
        {
            if (!isActive) return;
            HandleMovement();

            UpdateSound();
        }

        private void HandleMovement()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 inputDirection = new Vector3(h, 0, v);
            if (inputDirection.magnitude > 1f)
                inputDirection = inputDirection.normalized;
            inputDirection = transform.TransformDirection(inputDirection) * moveSpeed;

            if (controller.isGrounded)
            {
                verticalVelocity = -1f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
            inputDirection.y = verticalVelocity;
            controller.Move(inputDirection * Time.deltaTime);
        }

        public void TeleportTo(Vector3 pos)
        {
            Debug.Log($"[TestController] TeleportTo called, pos = {pos}");
            if (controller == null) controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.position = pos;
            controller.enabled = true;
            verticalVelocity = 0f;
        }

        private void UpdateSound()
        {
            if (controller.velocity.magnitude > 0.1f && controller.isGrounded)
            {
                PLAYBACK_STATE playbackState;
                footstepEvent.getPlaybackState(out playbackState);
                if (playbackState != PLAYBACK_STATE.PLAYING)
                {
                    footstepEvent.start();
                }
            } else 
            {
                footstepEvent.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    

}