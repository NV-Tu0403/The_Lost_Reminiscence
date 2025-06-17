using UnityEngine;

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

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!isActive) return;
            HandleMovement();
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
            if (controller == null) controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.position = pos;
            controller.enabled = true;
            verticalVelocity = 0f;
        }
    }
}