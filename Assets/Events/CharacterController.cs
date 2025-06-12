using UnityEngine;

namespace Events
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float gravity = -9.81f;
        private float verticalVelocity;
        private UnityEngine.CharacterController controller;

        [Header("Control")]
        public bool isActive = true;

        void Awake()
        {
            controller = GetComponent<UnityEngine.CharacterController>();
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
    }
}